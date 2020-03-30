using System;
using System.Collections;
using EngineCore.Simulater;
using Proto;
using Proto.LoginServerService;
using UnityEngine;
using UnityEngine.SceneManagement;
using Windows;
using XNet.Libs.Net;

public class LoginGate:UGate
{
    

    protected override void JoinGate()
    {
        SceneManager.LoadScene("null");
        UUIManager.Singleton.HideAll();
        UUIManager.S.CreateWindowAsync<UUILogin>((ui)=> {
            ui.ShowWindow();
        });
    }

    private RequestClient<TaskHandler> Client;

    public void GoLogin(string username, string password, Action<L2C_Login> callback)
    {
        StartCoroutine(DoLogin(username, password, callback));
    }

    public void GoReg(string username, string password, Action<L2C_Reg> callback)
    {
        StartCoroutine(DoReg(username, password, callback));
    }

    private IEnumerator DoReg(string username, string password, Action<L2C_Reg> callback)
    {
        yield return DoClient();
        if (Client == null)
        {
            callback?.Invoke(new L2C_Reg { Code = ErrorCode.Error });
            yield break;
        }

        var req = Reg.CreateQuery();
        yield return req.Send(Client, new C2L_Reg { Password = password, UserName = username, Version = MessageTypeIndexs.Version });
        callback?.Invoke(req.QueryRespons);
        Client?.Disconnect();
        Client = null;
    }

    private IEnumerator DoLogin(string name, string pwd, Action<L2C_Login> callback)
    {
        yield return DoClient();
        if (Client == null)
        {
            callback?.Invoke(new L2C_Login { Code = ErrorCode.LoginFailure });
            yield break;
        }
        var req = Login.CreateQuery();
        yield return req.Send(Client, new C2L_Login { Password = pwd , UserName = name, Version = MessageTypeIndexs.Version });
        callback?.Invoke(req.QueryRespons);
        Client?.Disconnect();
        Client = null;
    }

    private IEnumerator DoClient()
    {
        var ServerHost = UApplication.Singleton.ServerHost;
        var ServerPort = UApplication.Singleton.ServerPort;
        Client = new RequestClient<TaskHandler>(ServerHost, ServerPort, false);
        UApplication.Singleton.ConnectTime = Time.time;
        bool? re = null;
        Client.OnConnectCompleted = (su) =>
        {
            re = su;
        };
        Client.Connect();
        yield return new WaitUntil(() => re.HasValue);
        if (re != true)
        {
            Debug.LogError($"connect failure:{ServerHost}:{ServerPort}");
            Client.Disconnect();
            Client = null;
            yield break;
        }
    }

    protected override void ExitGate()
    {
        Debug.Log("Exit gate");
        Client?.Disconnect();
        Client = null;
    }

    protected override void Tick()
    {
        Client?.Update();
        UApplication.Singleton.PingDelay = (Client?.Delay??0) / (float)TimeSpan.TicksPerMillisecond;

    }

}