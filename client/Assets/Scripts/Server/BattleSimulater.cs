﻿using System.Collections;
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
using Layout.LayoutEffects;
using System.Linq;
using EConfig;
using UGameTools;
using UnityEngine.SceneManagement;
using Vector3 = UnityEngine.Vector3;
using P = Proto.HeroPropertyType;
using CM = ExcelConfig.ExcelToJSONConfigManager;
using Layout.AITree;
using Layout;
using System.Threading.Tasks;
using Proto.GateBattleServerService;

public class BattleSimulater : XSingleton<BattleSimulater>, IStateLoader, IAIRunner
{

    #region AI RUN
    private BattleCharacter aiAttach;

    AITreeRoot IAIRunner.RunAI(TreeNode ai)
    {
        if (aiAttach == null)
        {
            Debug.LogError($"Need attach a battlecharacter");
            return null;
        }

        if (this.State.Perception is BattlePerception p)
        {
            var root = p.ChangeCharacterAI(ai, this.aiAttach);
            root.IsDebug = true;
            return root;
        }

        return null;
    }

    bool IAIRunner.IsRuning(Layout.EventType eventType)
    {
        return false;
    }

    bool IAIRunner.ReleaseMagic(MagicData data)
    {
        return false;
    }

    void IAIRunner.Attach(BattleCharacter character)
    {
        aiAttach = character;
        if (character.AIRoot != null)
            character.AIRoot.IsDebug = true;
        //throw new System.NotImplementedException();
    }
    #endregion

    void IStateLoader.Load(GState state)
    {

    }
    private class BindPlayer
    {
        public Client Client;
        public BattlePlayer Player;
        public string Account;
    }
    private PlayerBornPosition[] playerBornPositions;
    private SocketServer Server;
    private Server.BattleMosterCreator MonsterCreator { set; get; }

    [Header("Server ID")]
    public int ServerID;

    public GTime GetTime() { return (PerView as ITimeSimulater).Now; }
    public ConnectionManager Manager { get { return Server.CurrentConnectionManager; } }
    public RequestClient<LoginServerTaskServiceHandler> CenterServerClient { private set; get; }
    public UPerceptionView PerView {private set; get; }
    public BattleLevelData LevelData { get; private set; }
    public MapData MapConfig { get; private set; }
    public BattleState State { private set; get; }
    public MonsterGroupPosition[] MonsterGroup { private set; get; }

    private readonly ConcurrentQueue<BindPlayer> _addTemp = new ConcurrentQueue<BindPlayer>();
    private readonly ConcurrentQueue<string> _kickUsers = new ConcurrentQueue<string>();
    private readonly Dictionary<string, BattlePlayer> BattlePlayers = new Dictionary<string, BattlePlayer>();

    private void Start()
    {
        StartCoroutine(Begin());
    }

    private IEnumerator Begin()
    {
        var config = ResourcesManager.S.ReadStreamingFile("server.json");
        var battleServer = BattleServerConfig.Parser.ParseJson(config); ;
        new CM(ResourcesManager.S);

        LevelData = CM.Current.GetConfigByID<BattleLevelData>(battleServer.Level);
        MapConfig = CM.Current.GetConfigByID<MapData>(LevelData.MapID);
        yield return SceneManager.LoadSceneAsync(MapConfig.LevelName, LoadSceneMode.Single);

        yield return new WaitForEndOfFrame();
        PerView = UPerceptionView.Create();

        MonsterGroup = FindObjectsOfType<MonsterGroupPosition>();
        playerBornPositions = FindObjectsOfType<PlayerBornPosition>();

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
                Version = 1
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

        State = new BattleState(PerView, this, PerView);
        State.Start(this.GetTime());
        MonsterCreator = new Server.BattleMosterCreator(this);
    }

    private void StopAll()
    {
        Debug.Log("Exit");
        Server?.Stop();
        CenterServerClient?.Disconnect();
        State?.Stop(GetTime());
        Server = null;
        State = null;
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
        if (State == null) return;
        GState.Tick(State, GetTime());
        MonsterCreator?.TryCreateMonster(GetTime().Time);
        ProcessJoinClient();
        ProcessAction();
        SendNotify(PerView.GetAndClearNotify());
    }

    private void OnDestroy()
    {
        StopAll();
    }

    private void ProcessJoinClient()
    {
        var per = State.Perception as BattlePerception;
        var view = per.View as UPerceptionView;
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

                var createNotify = view.GetInitNotify();

                var c = CreateUser(client.Player);
                if (c != null)
                {
                    client.Player.HeroCharacter = c;
                    BattlePlayers.Add(client.Account, client.Player);
                    var package = client.Player.GetNotifyPackage();
                    package.TimeNow = GetTime().Time;
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

    private void ExitPlayer(BattlePlayer p)
    {
        if (p.HeroCharacter)
        {
            GObject.Destory(p.HeroCharacter);
        }

        Server.DisConnectClient(p.Client);
        BattlePlayers.Remove(p.AccountId);
        if (!p.Dirty) return;
        Task.Factory.StartNew(() =>
        {
            var gateClient = new RequestClient<TaskHandler>(p.GateServer.ServicesHost,
                p.GateServer.ServicesPort);
            gateClient.ConnectAsync().Wait();
            if (!gateClient.IsConnect)
            {
                Debug.LogError($"Gate Server {p.GateServer} nofound");
                return;
            }

            var req = new B2G_BattleReward
            {
                AccountUuid = p.AccountId,
                MapID = LevelData.ID,
                Gold = p.Gold
            };

            foreach (var c in p.ConsumeItems)
            {
                req.ConsumeItems.Add(c);
            }
            foreach (var r in p.DropItems)
            {
                req.DropItems.Add(r);
            }
            var pack = BattleReward.CreateQuery()
            .GetResult(gateClient, req);
            gateClient.Disconnect();
        });
    }

    private void SendNotify(IMessage[] notify)
    {
        if (notify.Length > 0)
        {
            var buffer = new MessageBufferPackage();
            foreach (var i in notify)
            {
                //Debug.Log($"{i.GetType()}->{i}");
                buffer.AddMessage(i.ToNotityMessage());
            }
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
            if (i.Value.Client.TryGetActionMessage(out Message msg))
            {
                IMessage action = msg.AsAction();
                //Debug.Log($"client {i.Key} {action}");
                if (BattlePlayers.TryGetValue(i.Key, out BattlePlayer p))
                {
                    if (p.HeroCharacter?.AIRoot != null)
                    {
                        //保存到AI
                        Debug.Log($"[{p.HeroCharacter.Index}]{p.HeroCharacter.Name} {action}");
                        p.HeroCharacter.AIRoot[AITreeRoot.ACTION_MESSAGE] = action;
                        p.HeroCharacter.AIRoot.BreakTree();//处理输入 重新启动行为树
                    }
                }
            }
        }
    }

    private BattleCharacter CreateUser(BattlePlayer user)
    {
        BattleCharacter character = null;
        State.Each<BattleCharacter>(t =>
        {
            if (!t.Enable) return false;
            if (t.AcccountUuid == user.AccountId)
            {
                character = t;
                return true;
            }
            return false;
        });

        if (character != null) return character;

        var per = State.Perception as BattlePerception;
        var data = CM.Current.GetConfigByID<CharacterData>(user.GetHero().HeroID);
        var cData = CM.Current.FirstConfig<CharacterPlayerData>(t => t.CharacterID == data.ID);
        var magic = CM.Current.GetConfigs<CharacterMagicData>(t =>
        {
            return t.CharacterID == data.ID && (MagicReleaseType)t.ReleaseType == MagicReleaseType.MrtMagic;
        });

        var pos = GRandomer.RandomArray(playerBornPositions).transform.position;
        
        character = per.CreateCharacter(user.GetHero().Level,
            data, magic.ToList(), 1, pos, new Vector3(0, 0, 0), user.AccountId, user.GetHero().Name);
        if (cData != null) character.AddNormalAttack(cData.NormalAttack, cData.NormalAttackAppend);
        //处理装备加成
        foreach (var i in user.GetHero().Equips)
        {
            var itemsConfig = CM.Current.GetConfigByID<ItemData>(i.ItemID);
            var equipId = int.Parse(itemsConfig.Params[0]);
            var equipconfig = CM.Current.GetConfigByID<EquipmentData>(equipId);
            if (equipconfig == null) continue;
            var equip = user.GetEquipByGuid(i.GUID);
            float addRate = 0f;
            if (equip != null)
            {
                var equipLevelUp = CM
                    .Current
                    .FirstConfig<EquipmentLevelUpData>(t => t.Level == equip.Level && t.Quility == equipconfig.Quility);
                if (equipLevelUp != null)
                {
                    addRate = equipLevelUp.AppendRate / 10000f;
                }
            }
            //基础加成
            var properties = equipconfig.Properties.SplitToInt();
            var values = equipconfig.PropertyValues.SplitToInt();
            for (var index = 0; index < properties.Count; index++)
            {
                var p = (P)properties[index];
                var v = character[p].BaseValue + values[index] * (1 + addRate);
                character[p].SetBaseValue((int)v);
            }
        }
        //character.ModifyValue(P.ViewDistance, AddType.Append, 1000 * 100);
        per.ChangeCharacterAI(data.AIResourcePath, character);
        character.Reset();
        return character;
    }

    internal bool BindUser(string accountUuid, Client c, PlayerPackage package, DHero hero, GameServerInfo info)
    {
        var player = new BattlePlayer(accountUuid, package, hero, c,info);
        _addTemp.Enqueue(new BindPlayer { Client = c, Player = player, Account = accountUuid, });
        return true;
    }

    internal void KickUser(string account_uuid)
    {
        _kickUsers.Enqueue(account_uuid);
    }
}
