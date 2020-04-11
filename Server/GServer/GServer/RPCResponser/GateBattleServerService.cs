using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            var uuid = UserDataManager.S.ProcessBattleReward(
                request.AccountUuid,
                request.ModifyItems,
                request.RemoveItems,
                request.Exp,
                request.Level,
                request.DiffGold,
                request.HP,
                request.MP)
                .GetAwaiter().GetResult();

            if (string.IsNullOrEmpty(uuid)) return new G2B_BattleReward { Code = ErrorCode.Error };

            var userClient = Application.Current.GetClientByUserID(uuid);
            if (userClient != null)
            {
                //send to client
                UserDataManager.S.SyncToClient(userClient, uuid, true, true).Wait();
            }

            return new G2B_BattleReward { Code = code };
        }

        [IgnoreAdmission]
        public G2B_GetPlayerInfo GetPlayerInfo(B2G_GetPlayerInfo request)
        {
            return GetPlayer(request.AccountUuid).GetAwaiter().GetResult();
        }

        private async Task<G2B_GetPlayerInfo> GetPlayer(string accountID)
        {
            var player = await UserDataManager.S.FindPlayerByAccountId(accountID);

            if (player == null)
            {
                return new G2B_GetPlayerInfo
                {
                    Code = ErrorCode.NoGamePlayerData
                };
            }

            var package = await UserDataManager.S.FindPackageByPlayerID(player.Uuid);//.GetAwaiter().GetResult();
            var hero = await UserDataManager.S.FindHeroByPlayerId(player.Uuid);//.GetAwaiter().GetResult();
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
