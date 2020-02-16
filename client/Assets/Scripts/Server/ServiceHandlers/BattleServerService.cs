using Proto;
using Proto.BattleServerService;
using Proto.GateBattleServerService;
using Proto.LoginBattleGameServerService;
using UnityEngine;
using XNet.Libs.Net;

[Handle(typeof(IBattleServerService))]
public class BattleServerService : Responser, IBattleServerService
{
    public BattleServerService(Client c) : base(c) { }

    public B2C_ExitBattle ExitBattle(C2B_ExitBattle req)
    {
        var account_uuid = (string)Client.UserState;
        BattleSimulater.S.KickUser(account_uuid);
        return new B2C_ExitBattle { Code = ErrorCode.Ok };
    }

    [IgnoreAdmission]
    public B2C_JoinBattle JoinBattle(C2B_JoinBattle request)
    {
        var gate = BattleSimulater.S;
        var re = new B2L_CheckSession
        {
            UserID = request.AccountUuid,
            SessionKey = request.Session,

        };

        var seResult = CheckSession.CreateQuery().GetResult(gate.CenterServerClient, re);
        ErrorCode result = seResult.Code;
        if (seResult.Code == ErrorCode.Ok)
        {
            Client.UserState = request.AccountUuid;
            Client.HaveAdmission = true;
            var gateClient = new RequestClient<TaskHandler>(seResult.GateServer.ServicesHost,
                seResult.GateServer.ServicesPort);
            gateClient.ConnectAsync().Wait();
            if (!gateClient.IsConnect)
            {
                Debug.LogError($"Gate Server {seResult.GateServer} nofound");
                result = ErrorCode.Error;
            }

            var pack = GetPlayerInfo.CreateQuery().GetResult(gateClient,
                new B2G_GetPlayerInfo { AccountUuid = request.AccountUuid });

            Debug.Log($"{pack}");

            gateClient.Disconnect();

            if (!gate.BindUser(request.AccountUuid, Client, pack.Package, pack.Hero, seResult.GateServer))
            {
                result = ErrorCode.NofoundUserOnBattleServer;
            }
        }
        return new B2C_JoinBattle { Code = result };
    }
}

