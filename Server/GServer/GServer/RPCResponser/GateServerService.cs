#define USEGM

using System;
using GServer;
using GServer.Managers;
using MongoDB.Driver;
using Proto;
using Proto.GateServerService;
using Proto.LoginBattleGameServerService;
using Proto.MongoDB;
using ServerUtility;
using XNet.Libs.Net;

namespace GateServer
{
    [Handle(typeof(IGateServerService))]
    public class GateServerService: Responser, IGateServerService
    {
        public GateServerService(Client c) : base(c) { }

        public string AccountUuid { get { return (string)Client.UserState; } set {
                Client.UserState = value;
            } }

        public G2C_BeginGame BeginGame(C2G_BeginGame request)
        {
            var userID = (string)Client.UserState;
            var req = new G2L_BeginBattle
            {
                LevelId = request.LevelID,
                UserID = userID
            };
            var r = BeginBattle.CreateQuery().GetResult(Appliaction.Current.Client, req);
            return new G2C_BeginGame
            {
                Code = r.Code, //  ErrorCode.Error
                ServerInfo = r.BattleServer
            };
        }

        public G2C_CreateHero CreateHero(C2G_CreateHero request)
        {
            var manager = MonitorPool.G<UserDataManager>();
            var task = manager.TryToCreateUser(AccountUuid, request.HeroID, request.HeroName);
            task.Wait();
            return new G2C_CreateHero { Code = task.Result ? ErrorCode.Ok : ErrorCode.Error };
        }

        public G2C_EquipmentLevelUp EquipmentLevelUp(C2G_EquipmentLevelUp request)
        {
            var task = MonitorPool.G<UserDataManager>()
                .EquipLevel(AccountUuid, request.Guid, request.Level);

            task.Wait();
            return task.Result;
        }

        public G2C_GetLastBattle GetLastBattle(C2G_GetLastBattle request)
        {
            var response = new G2C_GetLastBattle { Code = ErrorCode.Error };

            var req = Proto.LoginBattleGameServerService.GetLastBattle.CreateQuery()
                .GetResult(Appliaction.Current.Client, new G2L_GetLastBattle { UserID = request.AccountUuid });
   
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
            if (!Appliaction.Current.EnableGM) return new G2C_GMTool() { Code = ErrorCode.Error };
            var task = MonitorPool.G<UserDataManager>()
                                .FindPlayerByAccountId(this.AccountUuid);
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
                            var filter = Builders<GameHeroEntity>.Filter.Eq(t => t.PlayerUuid,player.Uuid);
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
                        var dic = new System.Collections.Generic.Dictionary<int, PlayerItem>();
                        dic.Add(id, new PlayerItem { ItemID = id, Num = num });
                        MonitorPool.G<UserDataManager>().ProcessItem(player.Uuid, 0, 0, dic).Wait();
                    }
                    break;
                case "addgold":
                    {
                        //userData.AddGold(int.Parse(args[1]));
                    }
                    break;
                case "addcoin":
                    {
                        //userData.AddCoin(int.Parse(args[1]));
                    }
                    break;
                case "addexp":
                    {
                        //int level = 0;
                        //userData.AddExp(int.Parse(args[1]), out level);
                    }
                    break;
            }

            MonitorPool.G<UserDataManager>().SyncToClient(Client, player.Uuid)
                                .Wait();
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
                .GetResult(Appliaction.Current.Client, check);

            if (req.Code == ErrorCode.Ok)
            {
                var clients = Appliaction.Current.ListenServer.CurrentConnectionManager.AllConnections;
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
                var manager = MonitorPool.G<UserDataManager>();
                var task = manager.FindPlayerByAccountId(AccountUuid);
                task.Wait();
                var player = task.Result;
                if (player != null)
                    manager.SyncToClient(Client, player.Uuid).Wait();

                return new G2C_Login { Code = ErrorCode.Ok, HavePlayer = player != null };
            }
            else
            {
                return new G2C_Login { Code = req.Code };
            }
        }

        public G2C_OperatorEquip OperatorEquip(C2G_OperatorEquip request)
        {
            var manager = MonitorPool.G<UserDataManager>();
            var task = manager.FindPlayerByAccountId(AccountUuid);
            task.Wait();
            var player = task.Result;
            if (player !=null)
                return new G2C_OperatorEquip { Code = ErrorCode.NoGamePlayerData };

            var op = manager.OperatorEquip(AccountUuid, request.Guid, request.Part, request.IsWear);
            op.Wait();
            var result = op.Result;
            if (result) manager.SyncToClient(Client,player.Uuid).Wait();

            return new G2C_OperatorEquip
            {
                Code = !result ? ErrorCode.Error : ErrorCode.Ok,
                // Hero = 
            };
        }

        public G2C_SaleItem SaleItem(C2G_SaleItem req)
        {
            var task = MonitorPool.G<UserDataManager>()
                .SaleItem(AccountUuid, req.Items);
            task.Wait();
            return task.Result;
        }
    }
}
