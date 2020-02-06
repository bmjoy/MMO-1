using UnityEngine;
using org.vxwo.csharp.json;
using System.Collections.Generic;
using ExcelConfig;
using Proto;
using XNet.Libs.Utility;
using System.Collections;

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

    public void GoBackToMainGate()
    {
        //GameServer = new GameServerInfo{ ServerID =  , Host = host, Port =port };
        GoToMainGate(GameServer);
    }

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
        PlayerPrefs.SetString("_PlayerSession", session);
        PlayerPrefs.SetString("_UserID", AccountUuid);
        PlayerPrefs.SetInt("_GateServerPort", server.Port);
        PlayerPrefs.SetString("_GateServerHost", server.Host);
        PlayerPrefs.SetInt("_GateServerID", server.ServerId);
    }

    public void GotoLoginGate()
    {
        ChangeGate<LoginGate>();
    }

    public void GotoBattleGate(GameServerInfo serverInfo, int mapID)
    {
        ChangeGate<BattleGate>().SetServer(serverInfo, mapID);
    }

    public T ChangeGate<T>() where T : UGate
    {
        if (gate) Destroy(gate);
        return (T)(gate = gameObject.AddComponent<T>());
    }

    private UGate gate;


    #endregion

    #region mono behavior

    public void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        _ = new ExcelToJSONConfigManager(ResourcesManager.S);
        GetServer();

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        StartCoroutine(RunReader());
    }

    void Start()
    {
        GotoLoginGate();
    }

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
                UUITipDrawer.Singleton.ShowNotify(NotifyMessages.Dequeue());
                yield return new WaitForSeconds(.8f);
            }
        }
    }

    public void ShowError(ErrorCode code)
    {
        ShowNotify("ErrorCode:" + code);
    }

    public void ShowNotify(string notify)
    {
        NotifyMessages.Enqueue(notify);
    }

    private Queue<string> NotifyMessages { get; } = new Queue<string>();

    #endregion


    public static T G<T>() where T : UGate { return S.gate as T; }
}


