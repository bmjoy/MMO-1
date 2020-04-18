using UnityEngine;
using org.vxwo.csharp.json;
using System.Collections.Generic;
using ExcelConfig;
using Proto;
using XNet.Libs.Utility;
using System.Collections;
using EConfig;

/// <summary>
/// 处理 App
/// </summary>
public class UApplication : XSingleton<UApplication>
{


    public int ReceiveTotal;
    public int SendTotal;
    public float ConnectTime;

    public string ServerHost;
    public int ServerPort;
    public string ServerName;

    public string GateServerHost;
    public int GateServerPort;
    public int index = 0;
    public GameServerInfo GameServer;
    public string AccountUuid;
    public string SesssionKey;

    public float PingDelay = 0f;

    #region Gate

    public void GetServer()
    {
        var config = ResourcesManager.S.ReadStreamingFile("client.json");
        var clientConfig = ClientConfig.Parser.ParseJson(config);

        ServerHost = clientConfig.LoginServerHost;
        ServerPort = clientConfig.LoginServerPort;
        ServerName = clientConfig.LoginServerHost;
        Debug.Log(string.Format("{2} {0}:{1}", ServerHost, ServerPort, ServerName));
    }

    public void GoBackToMainGate()=> GoToMainGate(GameServer);
  
    public void GoToMainGate(GameServerInfo info)
    {
        ChangeGate<GMainGate>().Init(info);
        GateServerHost = info.Host;
        GateServerPort = info.Port;
    }

    public void GoServerMainGate(GameServerInfo server, string userID, string session)
    {
        GameServer = server;
        AccountUuid = userID;
        SesssionKey = session;
        GoToMainGate(server);
    }

    public void StartLocalLevel(DHero hero, PlayerPackage package, int levelID)
    {
        ChangeGate<LevelSimulatorGate>().Init(hero, package, levelID) ;
    }

    public void GotoLoginGate() => ChangeGate<LoginGate>();
   
    public void GotoBattleGate(GameServerInfo serverInfo, int mapID) => ChangeGate<BattleGate>().SetServer(serverInfo, mapID);
   

    public T ChangeGate<T>() where T : UGate
    {
        if (gate) Destroy(gate);
        return (T)(gate = gameObject.AddComponent<T>());
    }

    private UGate gate;


    #endregion

    #region mono behavior

    protected override void Awake()
    {
        base.Awake();
        _ = new ExcelToJSONConfigManager(ResourcesManager.S);
        GetServer();
        StartCoroutine(RunReader());
        Constant = ExcelToJSONConfigManager.Current.GetConfigByID<ConstantValue>(1);
    }

    public ConstantValue Constant { get;private set; }

    void Start() => GotoLoginGate();
  

    public void OnApplicationQuit()
    {
        if (gate) Destroy(gate);
    }

    #endregion

    #region Reader

    private IEnumerator RunReader()
    {
        while (true)
        {
            yield return null;

            if (NotifyMessages.Count > 0)
            {
                var message = NotifyMessages.Dequeue();
                UUITipDrawer.Singleton.ShowNotify(message);
                yield return null;
            }
        }
    }

    public void ShowError(ErrorCode code)
    {
        ShowNotify("ErrorCode:" + code);
    }

    public void ShowNotify(string msg)
    {
        NotifyMessages.Enqueue(new AppNotify { Message =msg, endTime = Time.time +3.2f });
    }

    private Queue<AppNotify> NotifyMessages { get; } = new Queue<AppNotify>();

    #endregion


    public static T G<T>() where T : UGate { return S.gate as T; }

   
}


