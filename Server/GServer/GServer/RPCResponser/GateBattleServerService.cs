using System;
using System.Collections.Generic;
using System.Linq;
using GServer;
using GServer.Managers;
using Proto;
using Proto.GateBattleServerService;
using ServerUtility;
using XNet.Libs.Net;

namespace GateServer
{
    [Handle(typeof(IGateBattleServerService))]
    public class GateBattleServerService : Responser, IGateBattleServerService
    {
        public GateBattleServerService(Client c) : base(c) { }

        [IgnoreAdmission]
        public G2B_BattleReward BattleReward(B2G_BattleReward request)
        {
            ErrorCode code = ErrorCode.Ok;

            var t = UserDataManager.S
                .ProcessBattleReward(request.AccountUuid, request.Items, request.Exp, request.Level, request.Gold);
            t.Wait();
            var uuid = t.Result;
            if (!string.IsNullOrEmpty(uuid))
            {
                var userClient = Application.Current.GetClientByUserID(uuid);
                if (userClient != null)
                {
                    UserDataManager.S.SyncToClient(userClient, uuid,true,true).Wait();
                }
            }
            else
            {
                code = ErrorCode.Error;
            }

            return new G2B_BattleReward { Code = code };
        }

        [IgnoreAdmission]
        public G2B_GetPlayerInfo GetPlayerInfo(B2G_GetPlayerInfo request)
        {

            // var manager = MonitorPool.G<UserDataManager>();
            var player = UserDataManager.S.FindPlayerByAccountId(request.AccountUuid).GetAwaiter().GetResult();

            if (player == null)
            {
                return new G2B_GetPlayerInfo
                {
                    Code = ErrorCode.NoGamePlayerData
                };
            }

            var package = UserDataManager.S.FindPackageByPlayerID(player.Uuid).GetAwaiter().GetResult();

            var hero = UserDataManager.S.FindHeroByPlayerId(player.Uuid).GetAwaiter().GetResult();

            return new G2B_GetPlayerInfo
            {
                Code = ErrorCode.Ok,
                Gold = player.Gold,
                Package = package.ToPackage(),
                Hero = hero.ToDhero(package)
            };
        }
    }
}
