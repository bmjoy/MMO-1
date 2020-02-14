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

    private class BindPlayer
    {
        public Client Client;
        public BattlePlayer Player;
        public string Account;
    }

    private MonsterGroupPosition[] monsterGroup;
    private PlayerBornPosition[] playerBornPositions;
    private DropGroupData drop;
    private int AliveCount = 0;
    private int CountKillCount = 0;
    private SocketServer Server;

    [Header("Server ID")]
    public int ServerID;


    public ConnectionManager Manager { get { return Server.CurrentConnectionManager; } }
    public RequestClient<LoginServerTaskServiceHandler> CenterServerClient { private set; get; }
    private GTime GetTime() { return (PerView as ITimeSimulater).Now; }
    public UPerceptionView PerView;
    public BattleLevelData LevelData { get; private set; }
    public MapData MapConfig { get; private set; }
    public BattleState State { private set; get; }

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

        monsterGroup = FindObjectsOfType<MonsterGroupPosition>();
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
        CenterServerClient.OnDisconnect = () => { Application.Quit(-2); };
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
    }

    private void ExitApp()
    {

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }

    internal bool BindUser(string accountUuid, Client c, PlayerPackage package, DHero hero)
    {
        var player = new BattlePlayer(accountUuid, package, hero, c);
        _addTemp.Enqueue(new BindPlayer { Client = c, Player = player, Account = accountUuid, });
        return true;
    }

    internal void KickUser(string account_uuid)
    {
        _kickUsers.Enqueue(account_uuid);
    }

    private void Update()
    {
        //CenterServerClient?.Update();
        if (State != null)
        {

            if (AliveCount == 0)
            {
                CreateMonster();
            }

            GState.Tick(State, GetTime());
            //CureHPAndMp();
            ProcessJoinClient();
            ProcessAction();
            SendNotify(PerView.GetAndClearNotify());
        }
    }



    private void OnDestroy()
    {
        Debug.Log("Exit");
        Server?.Stop();
        CenterServerClient?.Disconnect();
        State?.Stop(GetTime());
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
                        Debug.Log($"{i.GetType()}->{i}");
                        buffer.AddMessage(i.ToNotityMessage());
                    }
                    client.Client.SendMessage(buffer.ToPackage());
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
        Server.DisConnectClient(p.Client);
        BattlePlayers.Remove(p.AccountId);
    }

    private void SendNotify(IMessage[] notify)
    {
        if (notify.Length > 0)
        {
            var buffer = new MessageBufferPackage();
            foreach (var i in notify)
            {
                Debug.Log($"{i.GetType()}->{i}");
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
                Debug.Log($"client {i.Key} {action}");

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

    public void Load(GState state)
    {
        //to do
    }
    //处理掉落
    private void DoDrop()
    {
        if (drop == null) return;
        var items = drop.DropItem.SplitToInt();
        var pors = drop.Pro.SplitToInt();
        foreach (var i in BattlePlayers)
        {
            var notify = new Notify_Drop
            {
                AccountUuid = i.Value.AccountId
            };
            var gold = GRandomer.RandomMinAndMax(drop.GoldMin, drop.GoldMax);
            notify.Gold = gold;
            i.Value.AddGold(gold);
            if (items.Count > 0)
            {
                for (var index = 0; index < items.Count; index++)
                {
                    if (GRandomer.Probability10000(pors[index]))
                    {
                        i.Value.AddDrop(items[index], 1);
                        notify.Items.Add(new PlayerItem { ItemID = items[index], Num = 1 });
                    }
                }
            }
            var message = notify.ToNotityMessage();
            i.Value.Client.SendMessage(message);
        }
    }
    //处理怪物生成
    private void CreateMonster()
    {
        BattlePerception per = State.Perception as BattlePerception;
        //process Drop;
        if (drop != null)
        {
            DoDrop();
        }


        var groupPos = this.monsterGroup.Select(t => t.transform.position).ToArray();

        var pos = GRandomer.RandomArray(groupPos);

        IList<int> groups = null;

        if (CountKillCount < LevelData.BossNeedKilledNumber)
        {

            groups = LevelData.MonsterGroupID.SplitToInt();

        }
        else
        {
            groups = LevelData.BossGroupID.SplitToInt();
        }

        var monsterGroups = CM.Current.GetConfigs<MonsterGroupData>(t =>
        {
            return groups.Contains(t.ID);
        });

        var monsterGroup = GRandomer.RandomArray(monsterGroups);
        drop = CM.Current.GetConfigByID<DropGroupData>(monsterGroup.DropID);
        int maxCount = GRandomer.RandomMinAndMax(monsterGroup.MonsterNumberMin, monsterGroup.MonsterNumberMax);
        var standPos  = new List<BattleStandData>();
        switch ((StandType)monsterGroup.StandType)
        {
            case StandType.StAround:
                {
                    var r = monsterGroup.StandParams[0];
                    var ang = 360 / maxCount;
                    for (var i = 0; i < maxCount; i++)
                    {
                        var offset = Quaternion.Euler(0, ang * i, 0) * Vector3.forward * r;
                        var forword = Quaternion.LookRotation(Quaternion.Euler(0, ang * i, 0) * Vector3.forward);
                        standPos.Add(new BattleStandData { Pos = pos + offset, Forward = new Vector3(0, forword.eulerAngles.y, 0) });
                    }
                }
                break;
            case StandType.StRandom:
            default:
                {
                    var r = (int)monsterGroup.StandParams[0];
                    for (var i = 0; i < maxCount; i++)
                    {
                        var offset = new Vector3(GRandomer.RandomMinAndMax(-r, r), 0, GRandomer.RandomMinAndMax(-r, r));
                        standPos.Add(new BattleStandData
                        {
                            Pos = pos + offset,
                            Forward = new Vector3(0, GRandomer.RandomMinAndMax(0, 360), 0)
                        }); ;
                    }
                }

                break;
        }

        for (var i = 0; i < maxCount; i++)
        {
            var m = monsterGroup.MonsterID.SplitToInt();
            var p = monsterGroup.Pro.SplitToInt().ToArray();
            var monsterID = m[GRandomer.RandPro(p)];
            var monsterData = CM.Current.GetConfigByID<MonsterData>(monsterID);
            var data = CM.Current.GetConfigByID<CharacterData>(monsterData.CharacterID);
            var magic = CM.Current.GetConfigs<CharacterMagicData>(t => { return t.CharacterID == data.ID; });
            var Monster = per.CreateCharacter(monsterData.Level, data, magic.ToList(), 2, standPos[i].Pos,standPos[i].Forward, string.Empty,
                $"{monsterData.NamePrefix}.{ data.Name}");
            Monster[P.DamageMax].SetBaseValue(Monster[P.DamageMax].BaseValue + monsterData.DamageMax);
            Monster[P.DamageMin].SetBaseValue(Monster[P.DamageMin].BaseValue + monsterData.DamageMin);
            Monster[P.Force].SetBaseValue(Monster[P.Force].BaseValue + monsterData.Force);
            Monster[P.Agility].SetBaseValue(Monster[P.Agility].BaseValue + monsterData.Agility);
            Monster[P.Knowledge].SetBaseValue(Monster[P.Knowledge].BaseValue + monsterData.Knowledeg);
            Monster[P.MaxHp].SetBaseValue(Monster[P.MaxHp].BaseValue + monsterData.HPMax);
            //Monster[P.MaxMp].SetBaseValue(Monster[P.MaxMp].BaseValue+monsterData);
            Monster.Reset();
            per.ChangeCharacterAI(data.AIResourcePath, Monster);
            AliveCount++;
            Monster.OnDead = (el) =>
            {
                CountKillCount++;
                AliveCount--;
            };
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
        var cData = CM.Current.FirstConfig<CharacterPlayerData>(t=>t.CharacterID == data.ID);
        var magic = CM.Current.GetConfigs<CharacterMagicData>(t =>
        {
            return t.CharacterID == data.ID && (MagicReleaseType)t.ReleaseType == MagicReleaseType.MrtMagic;
        });

        var pos = GRandomer.RandomArray(playerBornPositions).transform.position;
        //处理装备加成
        character = per.CreateCharacter(user.GetHero().Level,
            data, magic.ToList(), 1, pos, new Vector3(0, 0, 0), user.AccountId, user.GetHero().Name);
        if (cData != null)
            character.AddNormalAttack(cData.NormalAttack, cData.NormalAttackAppend);

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
        character.ModifyValue(P.ViewDistance, AddType.Append, 1000 * 100);
        per.ChangeCharacterAI(data.AIResourcePath, character);
        character.Reset();
        return character;
    }

}
