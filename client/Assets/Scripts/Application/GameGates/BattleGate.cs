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

        Client = new RequestClient<TaskHandler>(ServerInfo.Host, ServerInfo.Port,false);
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
                        UUITipDrawer.Singleton.ShowNotify("BattleServer:" + r.Code);
                        UApplication.Singleton.GoBackToMainGate();
                    }
                    UUIManager.Singleton.ShowMask(false);
                });
            }
            else
            {
                UUITipDrawer.Singleton.ShowNotify("Can't login BattleServer!");
                UApplication.Singleton.GoBackToMainGate();
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
                FindObjectOfType<ThridPersionCameraContollor>()
                .SetLookAt(character.GetBoneByName("Bottom"));
                UUIManager.Singleton.ShowMask(false);
                var ui = UUIManager.Singleton.GetUIWindow<Windows.UUIBattle>();
                ui.InitCharacter(character);
            }
        };
        player.OnDeath = (view) =>
        {
            var character = view as UCharacterView;
            if (UApplication.S.AccountUuid == character.AccoundUuid)
            {
                UApplication.S.GoBackToMainGate();
            }
        };
        player.OnJoined = (initPack) =>
        {
            if (UApplication.Singleton.AccountUuid == initPack.AccountUuid)
            {
                startTime = Time.time;
                ServerStartTime = initPack.TimeNow;
            }
        };
        player.OnDrop = (drop) =>
        {

        };
    }

    private UCharacterView Owner;

    internal void MoveDir(Vector2 v)
    {
        if (!Owner) return;
        SendAction(new Action_MoveDir
        {
            Fast = true,
            Forward = new Proto.Vector3 { X = v.x, Z = v.y }
        });
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

    public void SendAction(IMessage action)
    {
        Client.SendMessage(action.ToAction());
    }

    public void Handle(Message message, SocketClient client)
    {
        var notify = message.AsNotify();
        Debug.Log($"{notify.GetType()}->{notify}");
        player.Process(notify);
    }

    internal void ReleaseSkill(CharacterMagicData magicData)
    {
        int target = -1;
        float dis = float.MaxValue;
        IList<UCharacterView> views = new List<UCharacterView>(); ;
        switch ((MagicReleaseAITarget)magicData.AITargetType)
        {
            case MagicReleaseAITarget.MatAll:
            case MagicReleaseAITarget.MatOwn:
            case MagicReleaseAITarget.MatOwnTeam:
                target = Owner.Index;
                break;
            case MagicReleaseAITarget.MatEnemy:
                PreView.Each<UCharacterView>(t =>
                {
                    if (t.TeamId == Owner.TeamId) return false;
                    var td = UnityEngine.Vector3.Distance(t.transform.position, Owner.transform.position);
                    if (td < dis) { dis = td; target = t.Index; }
                    return false;
                });
                break;
            case MagicReleaseAITarget.MatOwnTeamWithOutSelf:
                PreView.Each<UCharacterView>(t =>
                {
                    if (t.TeamId != Owner.TeamId) return false;
                    if (t.Index == Owner.Index) return false;
                    var td = UnityEngine.Vector3.Distance(t.transform.position, Owner.transform.position);
                    if (td < dis) { dis = td; target = t.Index; }
                    return false;
                });
                break;
        }

        SendAction(new Action_ClickSkillIndex { MagicId = magicData.ID, Target = target });

    }
    #endregion
}