#define USEGM

using System.Threading.Tasks;
using EConfig;
using ExcelConfig;
using GServer;
using GServer.Managers;
using MongoDB.Driver;
using Proto;
using Proto.GateServerService;
using Proto.LoginBattleGameServerService;
using Proto.MongoDB;
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
            return DoBeginGame(request).GetAwaiter().GetResult();
        }

        private async Task<G2C_BeginGame> DoBeginGame(C2G_BeginGame request)
        {
            var userID = (string)Client.UserState;
            var level = ExcelToJSONConfigManager.Current.GetConfigByID<BattleLevelData>(request.LevelID);
            if (level == null)
            {
                return new G2C_BeginGame { Code = ErrorCode.NofoundServerId };
            }
            var hero = await UserDataManager.S.FindHeroByAccountId(AccountUuid);
            if (hero.Level > level.LimitLevel)
            {
                return new G2C_BeginGame { Code = ErrorCode.PlayerLevelLimit };
            }
            var req = new G2L_BeginBattle
            {
                LevelId = request.LevelID,
                UserID = userID
            };
            var r = await BeginBattle.CreateQuery().SendAsync(Application.Current.Client, req);
            return new G2C_BeginGame
            {
                Code = r.Code, //  ErrorCode.Error
                ServerInfo = r.BattleServer
            };
        }

        public G2C_BuyPackageSize BuyPackageSize(C2G_BuyPackageSize req)
        {
            return UserDataManager.S.BuyPackageSize(Client, AccountUuid, req.SizeCurrent).GetAwaiter().GetResult();
        }

        public G2C_CreateHero CreateHero(C2G_CreateHero request)
        {
            return UserDataManager.S
                .TryToCreateUser(Client, AccountUuid, request.HeroID, request.HeroName)
                .GetAwaiter().GetResult();
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
                response.LevelID = req.LevelId;
            }

            return response;
        }

        public G2C_GMTool GMTool(C2G_GMTool request)
        {
            return ExecuteGM(request).GetAwaiter().GetResult();
        }

        private async Task<G2C_GMTool> ExecuteGM(C2G_GMTool request)
        {

#if USEGM
            if (!Application.Current.EnableGM) return new G2C_GMTool() { Code = ErrorCode.Error };
            var args = request.GMCommand.Split(' ');
            if (args.Length == 0) return new G2C_GMTool { Code = ErrorCode.Error };
            var player = await UserDataManager.S.FindPlayerByAccountId(this.AccountUuid);
            switch (args[0].ToLower())
            {
                case "level":
                    {
                        if (int.TryParse(args[1], out int level))
                        {
                            var update = Builders<GameHeroEntity>.Update.Set(t => t.Level, level);
                            await DataBase.S.Heros.FindOneAndUpdateAsync(t=>t.PlayerUuid == player.Uuid, update);
                        }
                    }
                    break;
                case "make":
                    {
                        int id = int.Parse(args[1]);
                        var num = 1;
                        if (args.Length > 2) num = int.Parse(args[2]);
                        await UserDataManager.S.AddItems(player.Uuid, new PlayerItem { ItemID = id, Num = num });
                    }
                    break;
                case "addgold":
                    {
                        var gold = int.Parse(args[1]);
                        await UserDataManager.S.AddGoldAndCoin(player.Uuid, 0, gold);//.Wait();
                    }
                    break;
                case "addcoin":
                    {
                        var coin = int.Parse(args[1]);
                        await UserDataManager.S.AddGoldAndCoin(player.Uuid, coin, 0);//.Wait();
                    }
                    break;
                case "addexp":
                    {
                        var exp = int.Parse(args[1]);
                        await UserDataManager.S.HeroGetExprise(player.Uuid, exp);
                    }
                    break;
            }

            await UserDataManager.S.SyncToClient(Client, player.Uuid, true, true);
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
            return DoLogin(request).GetAwaiter().GetResult();
        }

        private async Task<G2C_Login> DoLogin(C2G_Login request)
        {
            if (string.IsNullOrWhiteSpace(request.Session)) return new G2C_Login { Code = ErrorCode.Error };
            var check = new G2L_GateCheckSession
            {
                Session = request.Session,
                UserID = request.UserID
            };
            var req = await GateServerSession.CreateQuery().SendAsync(Application.Current.Client, check);
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
                var player = await DataBase.S.Playes.FindOneAndUpdateAsync(t => t.AccountUuid == AccountUuid,
                     Builders<GamePlayerEntity>.Update.Set(t => t.LastIp, Client.Socket.RemoteEndPoint.ToString()));
                if (player != null) await UserDataManager.S.SyncToClient(Client, player.Uuid, true, true);
                return new G2C_Login { Code = ErrorCode.Ok, HavePlayer = player != null };
            }
            else
            {
                return new G2C_Login { Code = req.Code };
            }
        }


        public G2C_MagicLevelUp MagicLevelUp(C2G_MagicLevelUp req)
        {
           return UserDataManager.S.MagicLevelUp(Client, req.MagicId, req.Level, AccountUuid)
                .GetAwaiter().GetResult();
        }

        public G2C_OperatorEquip OperatorEquip(C2G_OperatorEquip request)
        {
            var task = UserDataManager.S.FindPlayerByAccountId(AccountUuid);
            task.Wait();
            var player = task.Result;
            if (player == null)  return new G2C_OperatorEquip { Code = ErrorCode.NoGamePlayerData };
             var result = UserDataManager.S.OperatorEquip(player.Uuid, request.Guid, request.Part, request.IsWear)
                .GetAwaiter().GetResult();
            if (result) UserDataManager.S.SyncToClient(Client, player.Uuid,true).Wait();
            return new G2C_OperatorEquip
            {
                Code = !result ? ErrorCode.Error : ErrorCode.Ok,
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
            ShopItem item = null;
            foreach (var i in itemShop.Items)
            {
                if (i.ItemId == req.ItemId)
                {
                    item = i;
                    break;
                }
            }
            if (item == null) return new G2C_BuyItem { Code = ErrorCode.NoFoundItemInShop };

            return new G2C_BuyItem
            {
                Code = UserDataManager.S.BuyItem(Client, AccountUuid, item).GetAwaiter().GetResult()
            };
        }

        public G2C_SaleItem SaleItem(C2G_SaleItem req)
        {
            return UserDataManager
               .S
               .SaleItem(this.Client, AccountUuid, req.Items)
               .GetAwaiter()
               .GetResult();
        }

        public G2C_BuyGold BuyGold(C2G_BuyGold req)
        {
            return UserDataManager
                .S
                .BuyGold(Client, AccountUuid, req.ShopId)
                .GetAwaiter()
                .GetResult();
        }

        public G2C_RefreshEquip RefreshEquip(C2G_RefreshEquip req)
        {
            return UserDataManager.S.RefreshEquip(Client, AccountUuid, req.EquipUuid, req.CoustomItem).GetAwaiter().GetResult();
        }

        public G2C_ActiveMagic ActiveMagic(C2G_ActiveMagic req)
        {
            return UserDataManager.S.ActiveMadic(Client, AccountUuid, req.MagicId).GetAwaiter().GetResult();
        }
    }
}
