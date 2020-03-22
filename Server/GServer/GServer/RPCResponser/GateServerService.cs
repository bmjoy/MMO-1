#define USEGM

using EConfig;
using ExcelConfig;
using GServer;
using GServer.Managers;
using MongoDB.Driver;
using Proto;
using Proto.GateServerService;
using Proto.LoginBattleGameServerService;
using XNet.Libs.Net;
using static GateServer.DataBase;
using static Proto.ItemsShop.Types;

namespace GateServer
{
    [Handle(typeof(IGateServerService))]
    public class GateServerService : Responser, IGateServerService
    {
        public GateServerService(Client c) : base(c) { }

        public string AccountUuid
        {
            get { return (string)Client.UserState; }
            set
            {
                Client.UserState = value;
            }
        }

        public G2C_BeginGame BeginGame(C2G_BeginGame request)
        {
            var userID = (string)Client.UserState;
            var req = new G2L_BeginBattle
            {
                LevelId = request.LevelID,
                UserID = userID
            };
            var r = BeginBattle.CreateQuery().GetResult(Application.Current.Client, req);
            return new G2C_BeginGame
            {
                Code = r.Code, //  ErrorCode.Error
                ServerInfo = r.BattleServer
            };
        }

      

        public G2C_BuyPackageSize BuyPackageSize(C2G_BuyPackageSize req)
        {
            var task = UserDataManager.S.BuyPackageSize(AccountUuid, req.SizeCurrent);
            task.Wait();
            if (task.Result < 0) return new G2C_BuyPackageSize { Code = ErrorCode.Error };
            return new G2C_BuyPackageSize { Code = ErrorCode.Ok, OldCount = req.SizeCurrent, PackageCount = task.Result };
        }

        public G2C_CreateHero CreateHero(C2G_CreateHero request)
        {
            var manager = UserDataManager.S;
            var task = manager.TryToCreateUser(AccountUuid, request.HeroID, request.HeroName);
            task.Wait();

            if (!string.IsNullOrEmpty( task.Result))
            {
                manager.SyncToClient(Client, task.Result).Wait();
            }
            return new G2C_CreateHero { Code = !string.IsNullOrEmpty(task.Result) ? ErrorCode.Ok : ErrorCode.Error };
        }

        public G2C_EquipmentLevelUp EquipmentLevelUp(C2G_EquipmentLevelUp request)
        {
            var task = UserDataManager.S.EquipLevel(Client, AccountUuid, request.Guid, request.Level);
            task.Wait();
            return task.Result;
        }

        public G2C_GetLastBattle GetLastBattle(C2G_GetLastBattle request)
        {
            var response = new G2C_GetLastBattle { Code = ErrorCode.Error };

            var req = Proto.LoginBattleGameServerService.GetLastBattle.CreateQuery()
                .GetResult(Application.Current.Client, new G2L_GetLastBattle { UserID = request.AccountUuid });

            if (req.Code == ErrorCode.Ok)
            {
                response.BattleServer = req.BattleServer;
                response.MapID = req.LevelId;
            }

            return response;
        }

        public G2C_GMTool GMTool(C2G_GMTool request)
        {
#if USEGM
            if (!Application.Current.EnableGM) return new G2C_GMTool() { Code = ErrorCode.Error };
            var task = UserDataManager.S.FindPlayerByAccountId(this.AccountUuid);
            task.Wait();
            var player = task.Result;

            var args = request.GMCommand.Split(' ');
            if (args.Length == 0) return new G2C_GMTool { Code = ErrorCode.Error };

            switch (args[0].ToLower())
            {
                case "level":
                    {
                        if (int.TryParse(args[1], out int level))
                        {
                            var filter = Builders<GameHeroEntity>.Filter.Eq(t => t.PlayerUuid, player.Uuid);
                            var update = Builders<GameHeroEntity>.Update.Set(t => t.Level, level);
                            DataBase.S.Heros.UpdateOne(filter, update);
                        }
                    }
                    break;
                case "make":
                    {
                        int id = int.Parse(args[1]);
                        var num = 1;
                        if (args.Length > 2) num = int.Parse(args[2]);

                        UserDataManager.S.AddItems(player.Uuid, new PlayerItem { ItemID = id, Num = num })
                            .Wait();
                    }
                    break;
                case "addgold":
                    {
                        var gold = int.Parse(args[1]);
                        UserDataManager.S.AddGoldAndCoin(player.Uuid, 0, gold).Wait();
                    }
                    break;
                case "addcoin":
                    {
                        var coin = int.Parse(args[1]);
                        UserDataManager.S.AddGoldAndCoin(player.Uuid, coin, 0).Wait();
                    }
                    break;
                case "addexp":
                    {
                        var exp = int.Parse(args[1]);
                        UserDataManager.S.HeroGetExprise(player.Uuid, exp).Wait();
                    }
                    break;
            }

            UserDataManager.S.SyncToClient(Client, player.Uuid).Wait();
            return new G2C_GMTool
            {
                Code = ErrorCode.Ok
            };
#else
            return new G2C_GMTool { Code = ErrorCode.Error };
#endif
        }

        [IgnoreAdmission]
        public G2C_Login Login(C2G_Login request)
        {
            if (string.IsNullOrWhiteSpace(request.Session)) return new G2C_Login { Code = ErrorCode.Error };
            var check = new G2L_GateCheckSession
            {
                Session = request.Session,
                UserID = request.UserID
            };

            var req = GateServerSession.CreateQuery()
                .GetResult(Application.Current.Client, check);

            if (req.Code == ErrorCode.Ok)
            {
                var clients = Application.Current.ListenServer.CurrentConnectionManager.AllConnections;
                foreach (var i in clients)
                {
                    if (i.UserState != null
                        && (string)i.UserState == request.UserID)
                    {
                        i.Close();
                    }
                }
                Client.HaveAdmission = true;
                AccountUuid = request.UserID;
            }
            else { return new G2C_Login { Code = ErrorCode.Error }; };

            if (Client.HaveAdmission)
            {
                //var manager = MonitorPool.G<UserDataManager>();
                var task = UserDataManager.S.FindPlayerByAccountId(AccountUuid);
                task.Wait();
                var player = task.Result;
                if (player != null) UserDataManager.S.SyncToClient(Client, player.Uuid).Wait();
                return new G2C_Login { Code = ErrorCode.Ok, HavePlayer = player != null };
            }
            else
            {
                return new G2C_Login { Code = req.Code };
            }
        }

        public G2C_MagicLevelUp MagicLevelUp(C2G_MagicLevelUp req)
        {
            throw new System.NotImplementedException();
        }

        public G2C_OperatorEquip OperatorEquip(C2G_OperatorEquip request)
        {
            //var manager = MonitorPool.G<UserDataManager>();
            var task = UserDataManager.S.FindPlayerByAccountId(AccountUuid);
            task.Wait();
            var player = task.Result;
            if (player == null)  return new G2C_OperatorEquip { Code = ErrorCode.NoGamePlayerData };

            var op = UserDataManager.S.OperatorEquip(player.Uuid, request.Guid, request.Part, request.IsWear);
            op.Wait();
            var result = op.Result;
            if (result) UserDataManager.S.SyncToClient(Client, player.Uuid).Wait();

            return new G2C_OperatorEquip
            {
                Code = !result ? ErrorCode.Error : ErrorCode.Ok,
                // Hero = 
            };
        }

        public G2C_Shop QueryShop(C2G_Shop req)
        {
            var shops = ExcelToJSONConfigManager.Current.GetConfigs<ItemShopData>();

            if (shops.Length == 0) return new G2C_Shop { Code = ErrorCode.NoItemsShop };

            var res = new G2C_Shop { Code = ErrorCode.Ok };
            foreach (var i in shops)
            {
                res.Shops.Add(i.ToItemShop());
            }
            return res;
        }

        public G2C_BuyItem BuyItem(C2G_BuyItem req)
        {
            var shop = ExcelToJSONConfigManager.Current.GetConfigByID<ItemShopData>(req.ShopId);
            if (shop == null)
            {
                return new G2C_BuyItem { Code = ErrorCode.NoItemsShop };
            }

            var itemShop = shop.ToItemShop();
            ShopItem item =null;
            foreach (var i in itemShop.Items)
            {
                if (i.ItemId == req.ItemId)
                {
                    item = i;
                    break;
                }
            }
            if (item == null) return new G2C_BuyItem { Code = ErrorCode.NoFoundItemInShop };
            var task = UserDataManager.S.BuyItem(Client, AccountUuid, item);
            task.Wait();
            ErrorCode res = task.Result;
            
            return new G2C_BuyItem { Code = res };
        }

        public G2C_SaleItem SaleItem(C2G_SaleItem req)
        {
            var task = UserDataManager.S
                .SaleItem(this.Client, AccountUuid, req.Items);
            task.Wait();
            
            return task.Result;
        }
    }
}
