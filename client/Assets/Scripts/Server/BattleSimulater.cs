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
using Layout.LayoutEffects;
using System.Linq;
using GameLogic.Game;
using ExcelConfig;
using EConfig;
using UGameTools;
using UnityEngine.SceneManagement;
using org.vxwo.csharp.json;
using Vector3 = UnityEngine.Vector3;

public class BattleSimulater : XSingleton<BattleSimulater>, IStateLoader, IConfigLoader
{

    List<T> IConfigLoader.Deserialize<T>()
    {
        var name = ExcelToJSONConfigManager.GetFileName<T>();
        var json = ResourcesManager.S.LoadText("Json/" + name);
        if (json == null) return null;
        return JsonTool.Deserialize<List<T>>(json);
    }

    private class BindPlayer
    {
        public Client Client;
        public BattlePlayer Player;
        public string Account;
    }

    private float lastHpCure = 0;
    private MonsterGroupPosition[] monsterGroup;
    private PlayerBornPosition[] playerBornPositions;
    private DropGroupData drop;
    private int AliveCount = 0;
    private int CountKillCount = 0;
    private SocketServer Server;

    [Space]
    [Header("Service Info")]
    public int ListenPort = 2100;
    public string ListenHost = "127.0.0.1";
    public int MaxPlayer = 1000;
    [Space]
    [Header("Center Server")]
    public string LoginHost = "127.0.0.1";
    public int Port = 1800;//center server port
    public int ServerID = -1;
    [Space]
    [Header("Handle Level")]
    private int LevelID = 1;

    public ConnectionManager Manager { get { return Server.CurrentConnectionManager; } }
    public RequestClient<LoginServerTaskServiceHandler> CenterServerClient { private set; get; }
    private GTime GetTime() { return (PerView as ITimeSimulater).Now; }
    public ServerPerceptionView PerView;
    public BattleLevelData LevelData { get; private set; }
    public MapData MapConfig { get; private set; }
    public BattleState State { private set; get; }

    private readonly ConcurrentQueue<BindPlayer> _addTemp = new ConcurrentQueue<BindPlayer>();
    private readonly ConcurrentQueue<string> _kickUsers = new ConcurrentQueue<string>();
    private readonly Dictionary<string, BattlePlayer> BattlePlayers = new Dictionary<string, BattlePlayer>();
    [System.Obsolete("需要考虑下")]
    private readonly Dictionary<string, BattleCharacter> Characters = new Dictionary<string, BattleCharacter>();
    private void Start()
    {
        StartCoroutine(Begin());
    }

    private IEnumerator Begin()
    {
        //Debuger.Loger = new UnityLoger();
        new ExcelToJSONConfigManager(this);

        MapConfig = ExcelToJSONConfigManager.Current.GetConfigByID<MapData>(1);
        yield return SceneManager.LoadSceneAsync(MapConfig.LevelName, LoadSceneMode.Single);

        yield return new WaitForEndOfFrame();
        PerView = gameObject.AddComponent<ServerPerceptionView>();

        monsterGroup = FindObjectsOfType<MonsterGroupPosition>();
        playerBornPositions = FindObjectsOfType<PlayerBornPosition>();

        var handler = new RequestHandle<BattleServerService>();
        Server = new SocketServer(new ConnectionManager(), ListenPort)
        {
            HandlerManager = handler
        };

        Server.Start();

        yield return new WaitForEndOfFrame();

        CenterServerClient = new RequestClient<LoginServerTaskServiceHandler>(LoginHost, Port);
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
                Maxplayer = MaxPlayer,
                Host = ListenHost,
                Port = ListenPort,
                LevelId = LevelID,
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


        LevelData = ExcelToJSONConfigManager.Current.GetConfigByID<BattleLevelData>(LevelID);
        MapConfig = ExcelToJSONConfigManager.Current.GetConfigByID<MapData>(LevelData.MapID);

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
        var player = new BattlePlayer(accountUuid, package, hero,c) ;
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
        var view = per.View as ServerPerceptionView;
        //send Init message.
        while (_addTemp.Count > 0)
        {
            if (_addTemp.TryDequeue(out BindPlayer client))
            {
                if (BattlePlayers.TryGetValue(client.Account, out BattlePlayer p))
                {
                    Server.DisConnectClient(p.Client);
                    BattlePlayers.Remove(client.Account);
                }

                var createNotify = view.GetInitNotify();

                if (CreateUser(client.Player))
                {
                    BattlePlayers.Add(client.Account, client.Player);
                    //package
                    var package = client.Player.GetNotifyPackage();
                    package.TimeNow = (GetTime().Time);
                    var buffer = new MessageBufferPackage();
                    buffer.AddMessage(package.ToNotityMessage());
                    foreach (var i in createNotify)
                    {
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
                    Server.DisConnectClient(p.Client);
                    BattlePlayers.Remove(u);
                    if (Characters.TryGetValue(u, out BattleCharacter c))
                    {
                        Characters.Remove(u);
                        GObject.Destory(c);
                    }
                }
                 
            }
        }
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
            if (i.Value.Client?.Enable!=true)
            {
                KickUser(i.Key);
                continue;
            }

            if (i.Value.Client.TryGetActionMessage(out Message msg))
            {
                IMessage action = msg.AsAction();
                Debug.Log($"client {i.Key} {action}");
                if (Characters.TryGetValue(i.Key, out BattleCharacter userCharacter))
                {
                    if (action is Action_AutoFindTarget)
                    {
                        var auto = action as Action_AutoFindTarget;
                        userCharacter.ModifyValue(HeroPropertyType.ViewDistance,
                            AddType.Append, !auto.Auto ? 0 : 1000 * 100); //修改玩家AI视野
                        userCharacter.AIRoot.BreakTree();
                    }
                    else if (userCharacter.AIRoot != null)
                    {
                        //保存到AI
                        Debug.Log($"[{userCharacter.Index}]{userCharacter.Name} {action}");
                        userCharacter.AIRoot[AITreeRoot.ACTION_MESSAGE] = action;
                        userCharacter.AIRoot.BreakTree();//处理输入 重新启动行为树
                    }
                }
            }
        }
        //处理生命,魔法恢复
        if (lastHpCure + 1 < GetTime().Time)
        {
            lastHpCure = GetTime().Time;
            State.Each<BattleCharacter>((el) =>
            {
                var hp = (int)(el[HeroPropertyType.Force].FinalValue * BattleAlgorithm.FORCE_CURE_HP);
                if (hp > 0) el.AddHP(hp);
                var mp = (int)(el[HeroPropertyType.Knowledge].FinalValue * BattleAlgorithm.KNOWLEDGE_CURE_MP);
                if (mp > 0) el.AddMP(mp);
                return false;
            });
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

        var groups = LevelData.MonsterGroupID.SplitToInt();

        var monsterGroups = ExcelToJSONConfigManager.Current.GetConfigs<MonsterGroupData>(t =>
        {
            return groups.Contains(t.ID);
        });


        var monsterGroup = GRandomer.RandomArray(monsterGroups);
        drop = ExcelToJSONConfigManager.Current.GetConfigByID<DropGroupData>(monsterGroup.DropID);

        int maxCount = GRandomer.RandomMinAndMax(monsterGroup.MonsterNumberMin, monsterGroup.MonsterNumberMax);
        for (var i = 0; i < maxCount; i++)
        {
            var m = monsterGroup.MonsterID.SplitToInt();
            var p = monsterGroup.Pro.SplitToInt().ToArray();
            var monsterID = m[GRandomer.RandPro(p)];
            var monsterData = ExcelToJSONConfigManager.Current.GetConfigByID<MonsterData>(monsterID);
            var data = ExcelToJSONConfigManager.Current.GetConfigByID<CharacterData>(monsterData.CharacterID);
            var magic = ExcelToJSONConfigManager.Current.GetConfigs<CharacterMagicData>(t => { return t.CharacterID == data.ID; });
            var Monster = per.CreateCharacter(monsterData.Level,
                data,  magic.ToList(),  2,
                pos + new Vector3(GRandomer.RandomMinAndMax(-1, 1), 0, GRandomer.RandomMinAndMax(-1, 1)) * i,
                new Vector3(0, 0, 0), string.Empty,
                $"{monsterData.NamePrefix}.{ data.Name}");
            //data
            Monster[HeroPropertyType.DamageMax]
                .SetBaseValue(Monster[HeroPropertyType.DamageMax].BaseValue + monsterData.DamageMax);
            Monster[HeroPropertyType.DamageMin]
                .SetBaseValue(Monster[HeroPropertyType.DamageMin].BaseValue + monsterData.DamageMax);
            Monster[HeroPropertyType.Force]
                .SetBaseValue(Monster[HeroPropertyType.Force].BaseValue + monsterData.Force);
            Monster[HeroPropertyType.Agility]
                .SetBaseValue(Monster[HeroPropertyType.Agility].BaseValue + monsterData.Agility);
            Monster[HeroPropertyType.Knowledge]
                .SetBaseValue(Monster[HeroPropertyType.Knowledge].BaseValue + monsterData.Knowledeg);
            Monster[HeroPropertyType.MaxHp]
                .SetBaseValue(Monster[HeroPropertyType.MaxHp].BaseValue + monsterData.HPMax);
            Monster[HeroPropertyType.MaxMp]
                .SetBaseValue(Monster[HeroPropertyType.MaxMp].BaseValue);
            Monster.Name = string.Format("{0}.{1}", monsterData.NamePrefix, data.Name);

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

    private bool CreateUser(BattlePlayer user)
    {
        if (Characters.TryGetValue(user.AccountId, out BattleCharacter battleCharacte))
        {
            Debug.Log($"hero is exists!{user.AccountId}"); ;
            return true;
        }

        var per = State.Perception as BattlePerception;
        var data = ExcelToJSONConfigManager.Current.GetConfigByID<CharacterData>(user.GetHero().HeroID);
        var magic = ExcelToJSONConfigManager.Current
            .GetConfigs<CharacterMagicData>(t => { return t.CharacterID == data.ID; });
        var pos = GRandomer.RandomArray(playerBornPositions).transform.position;
        //处理装备加成
        battleCharacte = per.CreateCharacter(user.GetHero().Level,
            data, magic.ToList(), 1, pos, new Vector3(0, 0, 0), user.AccountId,
            user.GetHero().Name);
        foreach (var i in user.GetHero().Equips)
        {
            var itemsConfig = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(i.ItemID);
            var equipId = int.Parse(itemsConfig.Params1);
            var equipconfig = ExcelToJSONConfigManager.Current.GetConfigByID<EquipmentData>(equipId);
            if (equipconfig == null) continue;
            var equip = user.GetEquipByGuid(i.GUID);
            float addRate = 0f;
            if (equip != null)
            {
                var equipLevelUp = ExcelToJSONConfigManager
                    .Current
                    .FirstConfig<EquipmentLevelUpData>(t => t.Level == equip.Level && t.Quility == equipconfig.Quility);
                if (equipLevelUp != null)
                {
                    addRate = (float)equipLevelUp.AppendRate / 10000f;
                }
            }
            //基础加成
            var properties = equipconfig.Properties.SplitToInt();
            var values = equipconfig.PropertyValues.SplitToInt();
            for (var index = 0; index < properties.Count; index++)
            {
                var p = (HeroPropertyType)properties[index];
                var v = battleCharacte[p].BaseValue + (float)values[index] * (1 + addRate);
                battleCharacte[p].SetBaseValue((int)v);
            }
        }
        battleCharacte.ModifyValue(HeroPropertyType.ViewDistance, AddType.Append, 1000 * 100); //修改玩家AI视野
        Characters.Add(user.AccountId, battleCharacte);
        per.ChangeCharacterAI(data.AIResourcePath, battleCharacte);
        return true;
    }
}
