using System.Collections;
using System.Collections.Generic;
using EngineCore.Simulater;
using GameLogic;
using GameLogic.Game.Perceptions;
using GameLogic.Game.States;
using Proto;
using Proto.LoginBattleGameServerService;
using UnityEngine;
using XNet.Libs.Net;
using System.Collections.Concurrent;
using GameLogic.Game.Elements;
using XNet.Libs.Utility;
using Google.Protobuf;
using GameLogic.Game.AIBehaviorTree;
using EConfig;
using UGameTools;
using UnityEngine.SceneManagement;
using P = Proto.HeroPropertyType;
using CM = ExcelConfig.ExcelToJSONConfigManager;
using Layout.AITree;
using Layout;
using System.Threading.Tasks;
using Proto.GateBattleServerService;
using System;
using System.IO;
using GameLogic.Game.LayoutLogics;
using Server;

public class BattleSimulater : XSingleton<BattleSimulater>
{

    private SocketServer Server;

    [Header("Server ID")]
    public int ServerID;

    public ConnectionManager Manager { get { return Server.CurrentConnectionManager; } }
    public RequestClient<LoginServerTaskServiceHandler> CenterServerClient { private set; get; }

    private readonly ConcurrentQueue<BindPlayer> _addTemp = new ConcurrentQueue<BindPlayer>();
    private readonly ConcurrentQueue<string> _kickUsers = new ConcurrentQueue<string>();
    private readonly Dictionary<string, BattlePlayer> BattlePlayers = new Dictionary<string, BattlePlayer>();

    private void Start()
    {
        Application.targetFrameRate = 30;
        StartCoroutine(Begin());
    }

    private BattleLevelSimulater Simulater;

    private IEnumerator Begin()
    {

        string CommandLine = Environment.CommandLine;
        string[] CommandLineArgs = Environment.GetCommandLineArgs();
        var config = ResourcesManager.S.ReadStreamingFile("server.json");

        if (CommandLineArgs.Length >1)
        {
            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CommandLineArgs[1]);
            config = File.ReadAllText(file);
            Debug.Log($"{file}");
        }

        Debug.Log($"{config}");

        var battleServer = BattleServerConfig.Parser.ParseJson(config); ;
        new CM(ResourcesManager.S);
        Simulater = BattleLevelSimulater.Create(battleServer.Level);

        yield return StartCoroutine( Simulater.Start());
        var handler = new RequestHandle<BattleServerService>();
        Server = new SocketServer(new ConnectionManager(), battleServer.ListenPort)
        {
            HandlerManager = handler
        };
        Server.Start();

        yield return new WaitForEndOfFrame();
        CenterServerClient = new RequestClient<LoginServerTaskServiceHandler>(battleServer.LoginServiceHost, battleServer.LoginServerPort);
        bool connecting = true;
        CenterServerClient.OnConnectCompleted = (e) =>
        {
            connecting = false;
        };
        CenterServerClient.OnDisconnect = () => { ExitApp(); };
        CenterServerClient.Connect();

        yield return new WaitUntil(() => !connecting);

        if (!CenterServerClient.IsConnect)
        {
            ExitApp();
            yield break;
        }

        var req = RegBattleServer.CreateQuery();

        yield return req.Send(CenterServerClient,
            new B2L_RegBattleServer
            {
                Maxplayer = battleServer.MaxPlayer,
                Host = battleServer.ListenHost,
                Port = battleServer.ListenPort,
                LevelId = battleServer.Level,
                Version = MessageTypeIndexs.Version
            });

        if (req.QueryRespons.Code.IsOk())
        {
            ServerID = req.QueryRespons.ServiceServerID;
            Debug.Log($"Reg success get id of {ServerID}");
        }
        else
        {
            Debug.LogError("Connect server failure!");
            Server.Stop();
        }

    }

    private void StopAll()
    {
        Debug.Log("Exit");
        Server?.Stop();
        CenterServerClient?.Disconnect();
        Simulater?.Stop();
        Simulater = null;
        Server = null;
        CenterServerClient = null;
    }

    private void ExitApp()
    {
        StopAll();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void Update()
    {
        ProcessJoinClient();
        ProcessAction();
        SendNotify(Simulater.Tick());
    }

    private void OnDestroy()
    {
        StopAll();
    }

    private void ProcessJoinClient()
    {
        //send Init message.
        while (_addTemp.Count > 0)
        {
            if (_addTemp.TryDequeue(out BindPlayer client))
            {
                Debug.Log($"Add Client:{client.Account}");
                if (BattlePlayers.TryGetValue(client.Account, out BattlePlayer p))
                {
                    Server.DisConnectClient(p.Client);
                    BattlePlayers.Remove(client.Account);
                }

                var createNotify = Simulater.GetInitNotify();
                var c = Simulater.CreateUser(client.Player);
                if (c != null)
                {
                    client.Player.HeroCharacter = c;
                    BattlePlayers.Add(client.Account, client.Player);
                    var package = client.Player.GetNotifyPackage();
                    package.TimeNow =Simulater.TimeNow .Time;
                    var buffer = new MessageBufferPackage();
                    buffer.AddMessage(package.ToNotityMessage());
                    foreach (var i in createNotify)
                    {
                        buffer.AddMessage(i.ToNotityMessage());
                    }
                    client.Client.SendMessage(buffer.ToPackage());
                }
                else
                {
                    Debuger.LogError($"Create character failure!");
                    Server.DisConnectClient(p.Client);
                }
            }
        }
        while (_kickUsers.Count > 0)
        {
            if (_kickUsers.TryDequeue(out string u))
            {
                if (BattlePlayers.TryGetValue(u, out BattlePlayer p))
                {
                    ExitPlayer(p);
                }
            }
        }
    }

    private void ExitPlayer(BattlePlayer p, RequestClient<TaskHandler> gateClient =null)
    {
        if (p.HeroCharacter) GObject.Destroy(p.HeroCharacter);
        Server.DisConnectClient(p.Client);
        BattlePlayers.Remove(p.AccountId);
        if (!p.Dirty) return;
        Task.Factory.StartNew(() =>
        {
            if (gateClient == null)
            {
                gateClient = new RequestClient<TaskHandler>(p.GateServer.ServicesHost,
                   p.GateServer.ServicesPort);
                gateClient.ConnectAsync().Wait();
            }
            if (!gateClient.IsConnect)
            {
                Debug.LogError($"Gate Server {p.GateServer} nofound");
                return;
            }

            var req = new B2G_BattleReward
            {
                AccountUuid = p.AccountId,
                MapID = Simulater.LevelData.ID,
                Gold = p.Gold,
                Exp = p.GetHero().Exprices,
                Level = p.GetHero().Level
            };
            foreach (var c in p.Items)
            {
                req.Items.Add(c.Value);
            }
            var pack = BattleReward.CreateQuery()
            .GetResult(gateClient, req);
            gateClient.Disconnect();
        });
    }

    internal bool TryGetPlayer(string acccountUuid, out BattlePlayer player)
    {
        return BattlePlayers.TryGetValue(acccountUuid, out player);
    }

    private void SendNotify(IMessage[] notify)
    {
        if (notify.Length > 0)
        {
            var buffer = new MessageBufferPackage();
            foreach (var i in notify)
            {
                buffer.AddMessage(i.ToNotityMessage());
            }
            var syncTime = new Notify_SyncServerTime { ServerNow = Simulater.TimeNow.Time };
            buffer.AddMessage(syncTime.ToNotityMessage());
            var pack = buffer.ToPackage();
            foreach (var i in BattlePlayers)
            {
                if (!i.Value.Client.Enable)
                {
                    KickUser(i.Key);
                    continue;
                }
                i.Value.Client.SendMessage(pack);
            }
        }
    }

    private void ProcessAction()
    {
        foreach (var i in BattlePlayers)
        {
            if (i.Value.Client?.Enable != true)
            {
                KickUser(i.Key);
                continue;
            }
            bool needNotifyPackage = false;
            while (i.Value.Client.TryGetActionMessage(out Message msg))
            {
                IMessage action = msg.AsAction();
                if (action is Action_CollectItem collect)
                {

                    if (Simulater.TryGetElementByIndex(collect.Index, out BattleItem item))
                    {
                        if (item.IsAliveAble == true && item.CanBecollect(i.Value.HeroCharacter))
                        {
                            if (i.Value.AddDrop(item.DropItem))
                            {
                                needNotifyPackage = true;
                                GObject.Destroy(item);
                            }
                        }
                    }
                }
                else if (action is Action_UseItem useItem)
                {
                    if (i.Value.HeroCharacter.IsDeath) continue;
                    var config = CM.Current.GetConfigByID<ItemData>(useItem.ItemId);
                    if (config == null) continue;
                    if (i.Value.GetItemCount(useItem.ItemId) == 0) continue;
                    switch ((ItemType)config.ItemType)
                    {
                        case ItemType.ItHpitem:
                        case ItemType.ItMpitem:
                            {
                                var rTarget = new ReleaseAtTarget(i.Value.HeroCharacter, i.Value.HeroCharacter);
                                if (Simulater.CreateReleaser(config.Params[0], i.Value.HeroCharacter, rTarget, ReleaserType.Magic, -1))
                                {
                                    i.Value.ConsumeItem(useItem.ItemId);
                                    needNotifyPackage = true;
                                }
                                break;
                            }
                    }
                }
                else
                {
                    if (action is Action_ClickSkillIndex)
                    {
                        i.Value.HeroCharacter?.AiRoot.BreakTree();
                    }
                    i.Value.HeroCharacter?.AiRoot?.PushAction(action);
                }
            }
            if (needNotifyPackage)
            {
                var init = i.Value.GetNotifyPackage();
                init.TimeNow = Simulater.TimeNow.Time;
                i.Value.Client.SendMessage(init.ToNotityMessage());
            }
        }    
    }

    internal bool BindUser(string accountUuid, Client c, PlayerPackage package, DHero hero,int gold, GameServerInfo info)
    {
        var player = new BattlePlayer(accountUuid, package, hero, gold, c,info);
        _addTemp.Enqueue(new BindPlayer { Client = c, Player = player, Account = accountUuid, });
        return true;
    }

    internal void KickUser(string account_uuid)
    {
        _kickUsers.Enqueue(account_uuid);
    }
}
