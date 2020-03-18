using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Proto;
using ExcelConfig;
using System.Collections.Generic;
using GameLogic.Game.Perceptions;
using UGameTools;
using EConfig;
using Proto.GateServerService;
using System.Threading.Tasks;
using Windows;
using System.Collections;
using XNet.Libs.Net;
using Vector3 = UnityEngine.Vector3;
using GameLogic.Game.Elements;

public class GMainGate:UGate
{
    public void Init(GameServerInfo gateServer)
    {
        ServerInfo = gateServer;
    }

    public UPerceptionView view;
    public MainData Data;
    public int Gold;
    public int Coin;
    public PlayerPackage package;
    public DHero hero;
    private UCharacterView characterView;
    private GameServerInfo ServerInfo;
    public RequestClient<GateServerTaskHandler> Client{ private set; get; }

    public void UpdateItem(IList<PlayerItem> diff)
    {
        foreach (var i in diff)
        {
            if (package.Items.TryGetValue(i.GUID, out PlayerItem p))
            {
                p.Num += i.Num;
                if (p.Num <= 0)
                {
                    package.Items.Remove(i.GUID);
                }

            }
        }
        UUIManager.S.UpdateUIData();
    }

    public UCharacterView ReCreateHero(int heroID,string heroname)
    {

        if (characterView)
        {
            if (characterView.ConfigID == heroID) return characterView;
            characterView.DestorySelf(0);
        }

        var character = ExcelToJSONConfigManager.Current.GetConfigByID<CharacterData>(heroID);
        
        var perView = view as IBattlePerception;
        characterView = perView.CreateBattleCharacterView(string.Empty,
            character.ID,0,
            Data.pos[3].position.ToPVer3(),
            Vector3.zero.ToPVer3(),1,heroname,
            character.MoveSpeed, character.HPMax, character.HPMax, character.MPMax, character.MPMax, null) as UCharacterView;
        var thridCamear = FindObjectOfType<ThridPersionCameraContollor>();
        thridCamear.SetLookAt(characterView.GetBoneByName("Bottom"));
        characterView.ShowName = false;
        return characterView;
    }

    internal void RotationHero(float x)
    {
        characterView.targetLookQuaternion = characterView.targetLookQuaternion * Quaternion.Euler(0,x, 0);
        timeTO = Time.time + 2;
    }

    private float timeTO = -1f;

    #region implemented abstract members of UGate

    protected override void JoinGate()
    {
        UUIManager.Singleton.HideAll();
        UUIManager.Singleton.ShowMask(true);

        StartCoroutine(StartInit());
    }

    private IEnumerator StartInit()
    {

        yield return SceneManager.LoadSceneAsync("Main");

        Data = FindObjectOfType<MainData>();
        view = UPerceptionView.Create();
        Client = new RequestClient<GateServerTaskHandler>(ServerInfo.Host, ServerInfo.Port, false)
        {
            OnConnectCompleted = (success) =>
            {
                UApplication.S.ConnectTime = Time.time;
                if (success)
                {
                    _ = RequestPlayer();
                }
                else
                {
                    Invoke(() => { UApplication.S.GotoLoginGate(); });
                }
            },
            OnDisconnect = OnDisconnect
        };
        Client.Connect();
    }

    private async Task RequestPlayer()
    {

        var r = await Login.CreateQuery()
             .SendAsync(Client, new C2G_Login
             {
                 Session = UApplication.S.SesssionKey,
                 UserID = UApplication.S.AccountUuid,
                 Version = 1
             });

        Invoke(() =>
        {
            if (r.Code.IsOk())
            {
                ShowPlayer(r);
            }
            else
            {
                UUITipDrawer.S.ShowNotify("GateServer Response:" + r.Code);
                UApplication.S.GotoLoginGate();
            }
        });

    }


    private void ShowPlayer(G2C_Login result)
    {

        UUIManager.Singleton.ShowMask(false);

        if (result.HavePlayer)
        {
            ShowMain();
        }
        else
        {
            UUIManager.S.CreateWindow<UUIHeroCreate>().ShowWindow();
        }
    }

    public void ShowMain()
    {
        UUIManager.Singleton.CreateWindow<UUIMain>().ShowWindow() ;
    }

    protected override void ExitGate()
    {
        Client?.Disconnect();
        UUIManager.Singleton.ShowMask(false);
        UUIManager.Singleton.HideAll();
       
    }

    protected override void Tick()
    {
        if (Client == null) return;
        Client.Update();
        UApplication.Singleton.ReceiveTotal = Client.ReceiveSize;
        UApplication.Singleton.SendTotal = Client.SendSize;
        UApplication.Singleton.PingDelay = (float)Client.Delay / (float)TimeSpan.TicksPerMillisecond;
        if (timeTO > 0 && timeTO < Time.time)
        {
            timeTO = -1;
            if (!characterView) return;
            var character = ExcelToJSONConfigManager.Current.FirstConfig<CharacterPlayerData>(t => t.CharacterID == hero.HeroID);
            if (characterView is IBattleCharacter c && !string.IsNullOrEmpty( character?.Motion))
            {
                c.PlayMotion(character.Motion);
            }
            characterView.targetLookQuaternion = Quaternion.identity;
        }
    }

    private void OnDisconnect()
    {
        UApplication.S.GotoLoginGate();
    }

    

    public async Task TryToJoinLastBattle()
    {
        var r = await GetLastBattle.CreateQuery()
             .SendAsync(Client,
             new C2G_GetLastBattle
             {
                 AccountUuid = UApplication.S.AccountUuid
             });

        if (r.Code == ErrorCode.Ok)
        {
            UApplication.S.GotoBattleGate(r.BattleServer, r.MapID);
        }
        else
        {
            UApplication.S.ShowError(r.Code);
        }
    }
    #endregion

   
}