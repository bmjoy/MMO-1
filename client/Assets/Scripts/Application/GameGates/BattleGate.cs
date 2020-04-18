﻿using System;
using Proto;
using UnityEngine;
using XNet.Libs.Net;
using UnityEngine.SceneManagement;
using EConfig;
using Google.Protobuf;
using Proto.BattleServerService;
using XNet.Libs.Utility;
using ExcelConfig;
using System.Collections;
using System.Collections.Generic;
using GameLogic;
using GameLogic.Game.Elements;
using Vector3 = UnityEngine.Vector3;
using Windows;
using UnityEngine.AddressableAssets;
using UGameTools;

public class BattleGate : UGate, IServerMessageHandler,IBattleGate
{

    float IBattleGate.TimeServerNow { get { return TimeServerNow; } }

    UPerceptionView IBattleGate.PreView { get { return PreView; } }

    Texture IBattleGate.LookAtView { get { return LookAtView; } }

    UCharacterView IBattleGate.Owner { get { return Owner; } }

    void IBattleGate.Exit()
    {
        ExitBattle.CreateQuery()
                       .SendRequest(Client,
                       new C2B_ExitBattle
                       {
                           AccountUuid = UApplication.S.AccountUuid
                       },
                       (r) =>
                       {
                           UApplication.S.GoBackToMainGate();
                           if (!r.Code.IsOk())
                               UApplication.S.ShowError(r.Code);
                       }, UUIManager.S);
    }

    PlayerPackage IBattleGate.Package { get { return Package; } }

    DHero IBattleGate.Hero { get { return Hero; } }


    public void SetServer(GameServerInfo serverInfo, int levelID)
    {
       
        ServerInfo = serverInfo;
        Level = ExcelToJSONConfigManager.Current.GetConfigByID<BattleLevelData>(levelID);
        MapConfig = ExcelToJSONConfigManager.Current.GetConfigByID<MapData>(Level.MapID);
        
    }


    public BattleLevelData Level { private set; get; }

    public float TimeServerNow
    {
        get
        {
            if (startTime < 0)  return 0f;
            return Time.time - startTime + ServerStartTime;
        }
    }
    private float startTime = -1f;
    private float ServerStartTime = 0;

    public PlayerPackage Package { get; private set; }
    public DHero Hero { private set; get; }

    private MapData MapConfig;

    private  NotifyPlayer player;

    private GameServerInfo ServerInfo;
    public RequestClient<TaskHandler> Client { set; get; }

    public UPerceptionView PreView { get; internal set; }

    #region implemented abstract members of UGate

    private GameGMTools gm;

    protected override void JoinGate()
    {
        UUIManager.Singleton.HideAll();
        UUIManager.Singleton.ShowMask(true);
       
        StartCoroutine(Init());
        gm= this.gameObject.AddComponent<GameGMTools>();
    }

    private IEnumerator Init()
    {
        LookAtView = new RenderTexture(128, 128, 32);
        yield return Addressables.LoadSceneAsync($"Assets/Levels/{MapConfig.LevelName}.unity");
        yield return new WaitForEndOfFrame();
        PreView = UPerceptionView.Create();
        player = new NotifyPlayer(PreView);

        Client = new RequestClient<TaskHandler>(ServerInfo.Host, ServerInfo.Port, false);
        Client.RegisterHandler(MessageClass.Notify, this);
        Client.OnConnectCompleted += (success) =>
        {
            UApplication.Singleton.ConnectTime = Time.time;
            if (success)
            {
                _ = JoinBattle.CreateQuery()
                .SendRequest(Client, new C2B_JoinBattle
                {
                    Session = UApplication.S.SesssionKey,
                    AccountUuid = UApplication.S.AccountUuid,
                    MapID = Level.ID,
                    Version = MessageTypeIndexs.Version
                },
                (r) =>
                {
                    if (!r.Code.IsOk())
                    {
                        UApplication.Singleton.GoBackToMainGate();
                        UApplication.S.ShowError(r.Code);
                    }
                    
                });
            }
            else
            {
                UUITipDrawer.S.ShowNotify("Can't login BattleServer!");
                UApplication.S.GoBackToMainGate();
            }
        };
        Client.OnDisconnect += OnDisconnect;
        Client.Connect();
        player.OnCreateUser = (view) =>
        {
            var character = view as UCharacterView;
            if (character.OwnerIndex>0) return;
            if (UApplication.S.AccountUuid == character.AccoundUuid)
            {
                Owner = character;
                //Owner.transform.SetLayer( LayerMask.NameToLayer("Player"));
                Owner.ShowName = true;
                PreView.OwerTeamIndex = character.TeamId;
                PreView.OwnerIndex = character.Index;
                FindObjectOfType<ThridPersionCameraContollor>()
                .SetLookAt(character.GetBoneByName(UCharacterView.BottomBone));
                character.OnItemTrigger = TriggerItem;
                character.LookView(LookAtView);
                UUIManager.Singleton.CreateWindowAsync<UUIBattle>((ui) =>
                {
                    ui.ShowWindow(this);
                    UUIManager.S.ShowMask(false);
                });

            }
        };
        
        player.OnJoined = (initPack) =>
        {
            if (UApplication.S.AccountUuid == initPack.AccountUuid)
            {
                startTime = Time.time;
                ServerStartTime = initPack.TimeNow;
                Package = initPack.Package;
                Hero = initPack.Hero;
                UUIManager.S.UpdateUIData();
            }
        };

        player.OnAddExp = (exp) =>
        {
            Hero.Exprices = exp.Exp;
            Hero.Level = exp.Level;

            if (exp.Level != exp.OldLeve)
            {
                UUIManager.S.CreateWindowAsync<UUILevelUp>((ui) =>
                {
                    ui.ShowWindow(exp.Level);
                });
            }
            UUIManager.S.UpdateUIData();
            //UUIManager.S.GetUIWindow<UUIBattle>()?.InitHero(Hero);
        };

        player.OnDropGold = (gold) =>
        {
            //UApplication.S.ShowNotify($"获得金币{gold.Gold}");
        };

        player.OnSyncServerTime = (sTime) =>
        {
            startTime = Time.time;
            ServerStartTime = sTime.ServerNow;
        };
        
    }


    public RenderTexture LookAtView {private set; get; }

    private void TriggerItem(UBattleItem item)
    {
        if (item.IsOwner(Owner.Index))
        {
            SendAction(new Action_CollectItem { Index = item.Index });
        }
        else
        {
            UApplication.S.ShowNotify($"{item.config.Name} Can't collect!");
        }
    }

    public UCharacterView Owner { private set; get; }

  
    private float lastSyncTime = 0;
    private float releaseLockTime = -1;

    void IBattleGate.MoveDir(Vector3 dir)
    {
        if (!CanNetAction()) return;
        if (releaseLockTime > Time.time) return;
        if (Owner.IsLock(ActionLockType.NoMove)) return;
        var pos = Owner.transform.position;
        if (dir.magnitude > 0.01f)
        {
            var dn = new Vector3(dir.x, 0, dir.z);
            dn = dn.normalized;
            Vector3 willPos = Owner.MoveJoystick(dn);
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

    void IBattleGate.TrySendLookForward(bool force)
    {
        if (!force)
        {
            if (!Owner) return;

            if (Owner is IBattleCharacter view)
            {
                if (view.IsMoving) return;
            }
            if (Owner.InStartLayout) return;
        }

        var act = new Action_LookRotation { LookRotationY = ThridPersionCameraContollor.Current.RotationY };
        SendAction(act);
    }

    bool IBattleGate.IsMpFull()
    {
        if (!this.Owner) return true;
        return Owner.IsFullMp;
    }

    bool IBattleGate.IsHpFull()
    {
        if (!this.Owner) return true;
        return Owner.IsFullHp;
    }

    private void ReleaseLock()
    {
        releaseLockTime = Time.time + .3f;
        if (Owner) Owner.DoStopMove();
    }

    protected override void ExitGate()
    {
        if (gm) Destroy(gm);
        Client?.Disconnect();
        UUIManager.S.ShowMask(false);
        PrintReceived();
    }

    private void OnDisconnect()
    {
        UUITipDrawer.S.ShowNotify("Disconnect from BattleServer!");
        UApplication.S.GoBackToMainGate();
    }

    protected override void Tick()
    {
        if (Client != null)
        {
            PreView.GetAndClearNotify();
            Client.Update();
            UApplication.S.ReceiveTotal = Client.ReceiveSize;
            UApplication.S.SendTotal = Client.SendSize;
            UApplication.S.PingDelay = (float)Client.Delay / (float)TimeSpan.TicksPerMillisecond;
        }

        if (Print)
        {
            Print = false;
            PrintReceived();
        }
    }
    private void SendAction(IMessage action)
    {
        Debug.Log($"{action.GetType()}{action}");
        Client.SendMessage(action.ToAction());
    }

    private readonly Dictionary<Type, int> _messageTotal = new Dictionary<Type, int>();

    public void Handle(Message message, SocketClient client)
    {
        var notify = message.AsNotify();
       
        player.Process(notify);

        if (_messageTotal.TryGetValue(notify.GetType(), out _))
        {
            _messageTotal[notify.GetType()] += message.Size;
        }
        else
        {
            _messageTotal.Add(notify.GetType(), message.Size);
        }
    }
    public bool Print = false;
    private void PrintReceived()
    {
        foreach (var i in _messageTotal)
        {
            Debug.Log($"Total:{i.Key}->{i.Value}");
        }
    }

    private bool CanNetAction()
    {
        if (!Owner) return false;
        if (Owner.IsDeath) return false;
        if (Owner.IsLock(ActionLockType.NoAi)) return false;
        return true;
    }

    void IBattleGate.ReleaseSkill(HeroMagicData magicData)
    {
        if (!CanNetAction()) return;
        if (Owner.TryGetMagicData(magicData.MagicID, out HeroMagicData data))
        {
            var character = Owner as IBattleCharacter;
            var config = ExcelToJSONConfigManager.Current.GetConfigByID<CharacterMagicData>(data.MagicID);
            if (config != null) Owner.ShowRange(config.RangeMax);
            if (data.MPCost <= Owner.MP)
            {
                ReleaseLock();
                SendAction(new Action_ClickSkillIndex
                {
                    MagicId = data.MagicID,
                    Position = character.Transform.position.ToPV3(),
                    Rotation = character.Rotation.eulerAngles.ToPV3()
                });
            }
            else
            {
                UApplication.S.ShowNotify(LanguageManager.S.Format("BATTLE_NO_MP_TO_CAST", config.Name));
            }
        }
       
    }

    void IBattleGate.DoNormalAttack()
    {
        SendAction(new Action_NormalAttack());
    }

    bool IBattleGate.SendUseItem(ItemType type)
    {
        if (!Owner) return false;
        if (Owner.IsDeath) return false;
        //if (!CanNetAction()) return false;
        foreach (var i in Package.Items)
        {
            var config = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(i.Value.ItemID);
            if ((ItemType)config.ItemType == type)
            {
                SendAction(new Action_UseItem { ItemId = i.Value.ItemID });
                return true;
            }
        }
        return false;
    }


    #endregion
}