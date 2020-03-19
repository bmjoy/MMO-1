using System;
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

public class BattleGate : UGate, IServerMessageHandler
{

    public void SetServer(GameServerInfo serverInfo, int mapID)
    {
       
        ServerInfo = serverInfo;
        MapID = mapID;
        MapConfig = ExcelToJSONConfigManager.Current.GetConfigByID<MapData>(MapID);
        
    }

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

    private MapData MapConfig;

    private  NotifyPlayer player;

    private GameServerInfo ServerInfo;
    private int MapID;
    public RequestClient<TaskHandler> Client { set; get; }

    public UPerceptionView PreView { get; internal set; }

    #region implemented abstract members of UGate

    protected override void JoinGate()
    {
        UUIManager.Singleton.HideAll();
        UUIManager.Singleton.ShowMask(true);
        var ui = UUIManager.Singleton.CreateWindow<Windows.UUIBattle>();
        ui.ShowWindow();
        StartCoroutine(Init());
    }

    private IEnumerator Init()
    {
        yield return SceneManager.LoadSceneAsync(MapConfig.LevelName);

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
                    MapID = MapID,
                    Version = 1
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
            if (UApplication.S.AccountUuid == character.AccoundUuid)
            {
                Owner = character;
                Owner.ShowName = true;

                PreView.OwerTeamIndex = character.TeamId;
                PreView.OwnerIndex = character.Index;

                FindObjectOfType<ThridPersionCameraContollor>()
                .SetLookAt(character.GetBoneByName("Bottom"));
                UUIManager.Singleton.ShowMask(false);
                var ui = UUIManager.Singleton.GetUIWindow<Windows.UUIBattle>();
                ui.InitCharacter(character);
                UUIManager.S.ShowMask(false);
                character.OnItemTrigger = TriggerItem;

            }
        };
        
        player.OnJoined = (initPack) =>
        {
            if (UApplication.S.AccountUuid == initPack.AccountUuid)
            {
                startTime = Time.time;
                ServerStartTime = initPack.TimeNow;
                Package = initPack.Package;
                UUIManager.S.GetUIWindow<Windows.UUIBattle>()?.SetPackage(Package);
            }
        };
        
    }



    private void TriggerItem(UBattleItem item)
    {
        if (item.IsOwner(Owner.Index))
        {
            SendAction(new Action_CollectItem { Index = item.Index });
        }
        else
        {
            UApplication.S.ShowNotify($"{item.config.Name} 不属于你，无法拾取!");
        }
    }

    private UCharacterView Owner;

    internal void MoveDir(Vector3 dir)
    {
        if (!Owner) return;
        if (Owner.IsDeath) return;
       
        var fast = dir.magnitude > 0.8f;
        var pos = Owner.transform.position;
        var dn = dir.normalized;

        var ch = Owner as IBattleCharacter;
        var move = new Action_MoveDir
        {
            Fast = fast,
            Position = pos.ToPV3(),
            Forward = new Proto.Vector3 { X = dn.x, Z = dn.z }
        };
        SendAction(move);

        if (Owner.IsLock(ActionLockType.NoMove)) return;

        if (Owner.IsCanForwardMoving) 
        {
            if (dir.magnitude < 0.001f)
            {
              ch.StopMove(pos.ToPV3());
            }
            else
            {
                var f = dn * (fast ? 1f : 0.5f);
                ch.SetMoveDir(pos.ToPV3(), new Proto.Vector3 { X = f.x, Z = f.z });
            }
        }
    }


    public bool IsMpFull()
    {
        if (!this.Owner) return true;
        return Owner.IsFullMp;
    }

    public bool IsHpFull()
    {
        if (!this.Owner) return true;
        return Owner.IsFullHp;
    }
    internal bool SendUserItem(ItemType type)
    {
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

    internal void DoNormalAttack()
    {
        if (!Owner) return;

        if (Owner.TryGetMagicByType(MagicType.MtNormal, out HeroMagicData data))
        {
            var config = ExcelToJSONConfigManager.Current.GetConfigByID<CharacterMagicData>(data.MagicID);
            if (config != null) Owner.ShowRange(config.RangeMax);
        }

        SendAction(new Action_NormalAttack());
    }

    protected override void ExitGate()
    {
        Client?.Disconnect();
        UUIManager.Singleton.ShowMask(false);
    }

    private void OnDisconnect()
    {
        //UUITipDrawer.Singleton.ShowNotify("Can't login BattleServer!");
        UApplication.S.GoBackToMainGate();  
    }

    protected override void Tick()
    {
        if (Client != null)
        {
            PreView.GetAndClearNotify();
            Client.Update();
            UApplication.Singleton.ReceiveTotal = Client.ReceiveSize;
            UApplication.Singleton.SendTotal = Client.SendSize;
            UApplication.Singleton.PingDelay = (float)Client.Delay / (float)TimeSpan.TicksPerMillisecond;
        }
    }

    private void SendAction(IMessage action)
    {
        Client.SendMessage(action.ToAction());
    }

    public void Handle(Message message, SocketClient client)
    {
        var notify = message.AsNotify();
        //Debug.Log($"{notify.GetType()}->{notify}");
        player.Process(notify);
    }

    internal void ReleaseSkill(CharacterMagicData magicData)
    {
        if (!Owner) return;
        if (Owner.TryGetMagicData(magicData.ID, out HeroMagicData data))
        {
            var config = ExcelToJSONConfigManager.Current.GetConfigByID<CharacterMagicData>(data.MagicID);
            if (config != null) Owner.ShowRange(config.RangeMax);
            if (config.MPCost <= Owner.MP)
                SendAction(new Action_ClickSkillIndex { MagicId = magicData.ID });
            else
                UApplication.S.ShowNotify($"MP不足无法释放{config.Name}");
        }

       
    }

    #endregion
}