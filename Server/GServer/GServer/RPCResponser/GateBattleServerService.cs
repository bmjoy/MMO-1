﻿using System;
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

            //var manager = MonitorPool.G<UserDataManager>();
            var task = UserDataManager.S.FindPlayerByAccountId(request.AccountUuid);
            task.Wait();
            var player = task.Result;
            if (player == null)
            {
                return new G2B_BattleReward { Code = ErrorCode.NoGamePlayerData };
            }

            var diff = new Dictionary<int, PlayerItem>();
            foreach (var i in request.DropItems)
            {
                if (diff.ContainsKey(i.ItemID))
                {
                    diff[i.ItemID].Num += i.Num;
                }
                else
                {
                    diff.Add(i.ItemID, i);
                }
            }
            foreach (var i in request.ConsumeItems)
            {
                if (diff.ContainsKey(i.ItemID))
                {
                    diff[i.ItemID].Num -= i.Num;
                }
                else
                {
                    diff.Add(i.ItemID, i);
                }
            }
            ErrorCode code = ErrorCode.Ok;

            var t = UserDataManager.S.ProcessItem(player.Uuid, diff.Values.ToList());
            t.Wait();
            var r = t.Result;
            if (r)
            {
                var userClient = Application.Current.GetClientByUserID(player.Uuid);
                if (userClient != null)
                {
                    UserDataManager.S.SyncToClient(userClient, player.Uuid).Wait();
                }
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
                Package = package.ToPackage(),
                Hero = hero.ToDhero(package)
            };
        }
    }
}
