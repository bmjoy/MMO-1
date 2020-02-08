using System;
using Proto;
using Proto.LoginServerService;
using Proto.MongoDB;
using XNet.Libs.Net;
using LoginServer;
using MongoDB.Driver;
using MongoTool;
using XNet.Libs.Utility;
using Google.Protobuf.WellKnownTypes;

namespace RPCResponsers
{
    /// <summary>
    /// Login server handle client request
    /// </summary>
    [Handle(typeof(ILoginServerService))]
    public class LoginServerService : Responser, ILoginServerService
    {

        public LoginServerService(Client c) : base(c) { }

        [IgnoreAdmission]
        public L2C_Login Login(C2L_Login request)
        {

            var users = DataBase.S.Account;

            var pwd = Md5Tool.GetMd5Hash(request.Password);
            var pwdfilter = Builders<AccountEntity>.Filter.Eq(t => t.Password, pwd);
            var namefilter = Builders<AccountEntity>.Filter.Eq(t=>t.UserName, request.UserName);
            var filter = Builders<AccountEntity>.Filter.And(namefilter, pwdfilter);
            var query = users.Find(filter);

            if (!query.Any()) return new L2C_Login { Code = ErrorCode.LoginFailure };


            var user = query.First();
            var s_filter = Builders<UserSessionInfoEntity>.Filter.Eq(t => t.AccountUuid, user.Uuid);
            DataBase.S.Session.DeleteMany(s_filter);
            if (DataBase.S.GetSessionInfo(user.Uuid, out UserSessionInfoEntity info))
            {
                if (info.BattleServerId > 0)
                {
                    var b_filter = Builders<PlayerBattleServerEntity>.Filter.Eq(t => t.ServerId, info.BattleServerId);
                    var server = DataBase.S.BattleServer.Find(b_filter).SingleOrDefault();
                    if (server != null)
                    {
                        var task = new Task_L2B_ExitUser
                        {
                            UserID = user.Uuid
                        };
                        Appliaction.Current.GetServerConnectByClientID(server.ClientId)?
                            .CreateTask(task)
                            .Send();
                    }
                }
            }

            user.LoginCount += 1;
            var update = Builders<AccountEntity>.Update
                .Set(u => u.LastLoginTime,  DateTime.Now)
                .Set(t=>t.LoginCount, user.LoginCount);

            var upfilter = Builders<AccountEntity>.Filter.Eq(t => t.Uuid, user.Uuid);
            users.UpdateOne(upfilter, update);

            var g_filter = Builders<GateServerInfoEntity>.Filter.Eq(t => t.ServerId, user.ServerID);
            var gate = DataBase.S.GateServer.Find(g_filter).SingleOrDefault();
            if (gate == null)  return new L2C_Login { Code = ErrorCode.NofoundServerId };
            var session = SaveSession(user.Uuid, user.ServerID);
            return new L2C_Login
            {
                Code = ErrorCode.Ok,
                GateServer = DataBase.S.ToServerInfo(gate),
                Session = session,
                UserID = user.Uuid
            };
        }


        private string SaveSession(string uuid,int gateServer)
        {
            var session = Md5Tool.GetMd5Hash(DateTime.UtcNow.Ticks.ToString());
            var us = new UserSessionInfoEntity
            {
                AccountUuid = uuid,
                Token = session,
                LevelId = -1,
                BattleServerId = -1,
                GateServerId = gateServer
            };
            DataBase.S.Session.InsertOne(us);
            return session;
        }

        [IgnoreAdmission]
        public L2C_Reg Reg(C2L_Reg request)
        {
            if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
                return new L2C_Reg { Code = ErrorCode.RegInputEmptyOrNull };
            if (request.UserName.Length > 100 || request.UserName.Length < 2)
                return new L2C_Reg { Code = ErrorCode.NameOrPwdLeghtIncorrect };
            if (request.Password.Length > 100 || request.Password.Length < 2)
                return new L2C_Reg { Code = ErrorCode.NameOrPwdLeghtIncorrect };

            var users = DataBase.S.Account;
            var filter = Builders<AccountEntity>.Filter.Eq(t=>t.UserName, request.UserName);
            var query = users.Find(filter);

            if (query.Any())return new L2C_Reg{Code = ErrorCode.RegExistUserName };
            var free  = Builders<GateServerInfoEntity>.Filter.Lt(t => t.CurrentPlayerCount ,15000);
            var data = DataBase.S.GateServer.Find(free).FirstOrDefault();
            if (data == null) return new L2C_Reg() { Code = ErrorCode.NoFreeGateServer };
    
            var serverID = data.ServerId;
            var pwd = Md5Tool.GetMd5Hash(request.Password);
            var acc = new AccountEntity
            {
                UserName = request.UserName,
                Password = pwd,
                CreateTime = DateTime.Now,
                LoginCount = 0,
                LastLoginTime =DateTime.Now,
                ServerID = serverID
            };

            users.InsertOne(acc);

            var session = SaveSession(acc.Uuid, acc.ServerID);
            return new L2C_Reg
            {
                Code = ErrorCode.Ok,
                Session = session,
                UserID = acc.Uuid,
                GateServer = DataBase.S.ToServerInfo(data)
            };
        }
    }
}
