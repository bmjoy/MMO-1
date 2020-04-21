using System;
using System.Collections;
using System.Collections.Generic;
using EConfig;
using EngineCore.Simulater;
using GameLogic;
using GameLogic.Game.Elements;
using GameLogic.Game.LayoutLogics;
using GameLogic.Game.Perceptions;
using GameLogic.Game.States;
using Google.Protobuf;
using Proto;
using UGameTools;
using UnityEngine;
using UnityEngine.AddressableAssets;
using CM = ExcelConfig.ExcelToJSONConfigManager;
using P = Proto.HeroPropertyType;
using Vector3 = UnityEngine.Vector3;

public class LevelSimulatorGate : UGate, IStateLoader,IBattleGate
{
    private int LevelId;
    private DHero Hero;
    private PlayerPackage Package;

    public RenderTexture LookAtView { get; private set; }
    public BattleLevelData LevelData { get; private set; }
    public MapData MapConfig { get; private set; }
    public UPerceptionView PerView { get; private set; }

    private ITimeSimulater timeSimulater;

    public MonsterGroupPosition[] MonsterGroup { get; private set; }

    private PlayerBornPosition[] playerBornPositions;

    public BattleState State { private set; get; }

    public BattlePerception Per{get{ return State.Perception as BattlePerception; }}

    protected override void JoinGate()
    {
        base.JoinGate();
        UUIManager.S.ShowMask(true);
        StartCoroutine(InitLevel(LevelId));
    }

    protected override void ExitGate()
    {
        base.ExitGate();
        State?.Stop(timeSimulater.Now);
    }

    private IEnumerator InitLevel(int levelId)
    {

        LookAtView = new RenderTexture(128, 128, 32);
        LevelData = CM.Current.GetConfigByID<BattleLevelData>(levelId);
        MapConfig = CM.Current.GetConfigByID<MapData>(LevelData.MapID);
        yield return Addressables.LoadSceneAsync($"Assets/Levels/{MapConfig.LevelName}.unity");
        yield return new WaitForEndOfFrame();

        PerView = UPerceptionView.Create();
        timeSimulater = PerView as ITimeSimulater;
        MonsterGroup = GameObject.FindObjectsOfType<MonsterGroupPosition>();
        playerBornPositions = GameObject.FindObjectsOfType<PlayerBornPosition>();
        yield return new WaitForEndOfFrame();
        State = new BattleState(PerView, this, PerView);
        State.Start(this.GetTime());
        yield return null;
        MCreator = new Server.BattleMosterCreator(LevelData, MonsterGroup, Per);

        var data = CM.Current.GetConfigByID<CharacterData>(Hero.HeroID);
        var magic = Hero.CreateHeroMagic();
        var appendProperties = new Dictionary<P, int>();
        foreach (var i in Hero.Equips)
        {
            var equip = GetEquipByGuid(i.GUID);
            if (equip == null)
            {
                Debug.LogError($"No found equip {i.GUID}");
                continue;
            }
            var ps = equip.GetProperties();
            foreach (var p in ps)
            {
                if (appendProperties.ContainsKey(p.Key))
                {
                    appendProperties[p.Key] += p.Value.FinalValue;
                }
                else
                {
                    appendProperties.Add(p.Key, p.Value.FinalValue);
                }
            }
        }
        var pos = GRandomer.RandomArray(playerBornPositions).transform;//.position;        
        CharacterOwner = Per.CreateCharacter(
            Hero.Level,
            data,
            magic, appendProperties,
            0,
            pos.position,
            pos.rotation.eulerAngles,
            string.Empty,
            Hero.Name);
        Per.ChangeCharacterAI(data.AIResourcePath, CharacterOwner);
        Owner = CharacterOwner.CharacterView as UCharacterView;
        PerView.OwerTeamIndex = Owner.TeamId;
        PerView.OwnerIndex = Owner.Index;
        FindObjectOfType<ThridPersionCameraContollor>()
                .SetLookAt(Owner.GetBoneByName(UCharacterView.BottomBone));
        Owner.LookView(LookAtView);
        yield return UUIManager.S.CreateWindowAsync<Windows.UUIBattle>(ui => ui.ShowWindow(this));
        UUIManager.S.ShowMask(false);
    }

    internal void Init(DHero hero, PlayerPackage package, int levelID)
    {
        this.Hero = hero;
        this.Package = package;
        this.LevelId = levelID;
    }

    private Server.BattleMosterCreator MCreator;

    private PlayerItem GetEquipByGuid(string uuid)
    {
        if (Package.Items.TryGetValue(uuid, out PlayerItem ite))
            return ite;
        return null;
    }

    private BattleCharacter CharacterOwner;
    private float lastSyncTime;

    public UCharacterView Owner { private set; get; }

    private GTime GetTime()
    {
        return timeSimulater.Now;
    }

    void IStateLoader.Load(GState state)
    {
        //throw new System.NotImplementedException();
    }

    protected override void Tick()
    {
        base.Tick();
        if (State == null) return;
        GState.Tick(State, timeSimulater.Now);
        MCreator?.TryCreateMonster(this.GetTime().Time);
        PerView.GetAndClearNotify();
    }

    float IBattleGate.TimeServerNow => timeSimulater.Now.Time;

    UPerceptionView IBattleGate.PreView => PerView;

    Texture IBattleGate.LookAtView => LookAtView;

    UCharacterView IBattleGate.Owner => Owner;

    PlayerPackage IBattleGate.Package => Package;

    DHero IBattleGate.Hero => Hero;

    void IBattleGate.ReleaseSkill(HeroMagicData data)
    {
        var character = Owner as IBattleCharacter;
        SendAction(new Action_ClickSkillIndex
        {
            MagicId = data.MagicID,
            Position = character.Transform.position.ToPV3(),
            Rotation = character.Rotation.eulerAngles.ToPV3()
        });
    }

    void IBattleGate.Exit()
    {
        UApplication.S.GoBackToMainGate();
    }

    void IBattleGate.MoveDir(UnityEngine.Vector3 dir)
    {
        if (Owner.IsLock(ActionLockType.NoMove)) return;
        var pos = Owner.transform.position;
        if (dir.magnitude > 0.01f)
        {
            var dn = new Vector3(dir.x, 0, dir.z);
            dn = dn.normalized;
            var willPos = Owner.MoveJoystick(dn);
            if (lastSyncTime + 0.2f < Time.time)
            {
                var joystickMove = new Action_MoveJoystick
                {
                    Position = pos.ToPV3(),
                    WillPos = willPos.ToPV3()
                };
                SendAction(joystickMove);
                lastSyncTime = Time.time;
            }
        }
        else
        {
            var stopMove = new Action_StopMove { StopPos = pos.ToPV3() };
            if (Owner.DoStopMove())
            {
                SendAction(stopMove);
                if (this is IBattleGate b)
                    b.TrySendLookForward(true);
            }
        }
    }

    private void SendAction(IMessage action)
    {
        CharacterOwner?.AiRoot.PushAction(action);
    }

    void IBattleGate.TrySendLookForward(bool force)
    {
       // CharacterOwner?.AiRoot.PushAction(new Action_LookRotation { Position = dir.ToPV3() });
    }

    void IBattleGate.DoNormalAttack()
    {
        SendAction(  new Action_NormalAttack());
    }

    bool IBattleGate.SendUseItem(ItemType type)
    {
        foreach (var i in Package.Items)
        {
            var config = CM.Current.GetConfigByID<ItemData>(i.Value.ItemID);
            if ((ItemType)config.ItemType == type)
            {
                var rTarget = new ReleaseAtTarget(CharacterOwner, CharacterOwner);
                Per.CreateReleaser(config.Params[0], CharacterOwner, rTarget, ReleaserType.Magic, ReleaserModeType.RmtNone, -1);
                return true;
            }
        }
        return false;
    }

    bool IBattleGate.IsHpFull()
    {
        return false;
    }

    bool IBattleGate.IsMpFull()
    {
        return false;
    }


}
