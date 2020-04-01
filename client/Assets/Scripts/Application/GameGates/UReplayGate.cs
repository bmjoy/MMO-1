using System;
using GameLogic.Utility;
using UnityEngine.SceneManagement;
using UnityEngine;
using Proto;
using UGameTools;
using Google.Protobuf;
using EConfig;
using EngineCore.Simulater;
using UnityEngine.AddressableAssets;

public class UReplayGate : UGate
{
    public void Init(byte[] replayerData, int mapID)
    {
        var replayer = new NotifyMessagePool();
        replayer.LoadFormBytes(replayerData);
        Replayer = replayer;
        var data = ExcelConfig.ExcelToJSONConfigManager.Current.GetConfigByID<MapData>(mapID);
        Map = data;
    }

    private MapData Map;

    private NotifyMessagePool Replayer;
    private float startTime = -1f;
    private NotifyMessagePool.Frame frame;
    private NotifyPlayer player;
    private UPerceptionView PerView;

    protected override void JoinGate()
    {
        StartCoroutine(Load());
    }

    private GTime GetTime() => (PerView as ITimeSimulater).Now;

    private System.Collections.IEnumerator Load()
    {
        UUIManager.S.ShowMask(true);
        UUIManager.S.HideAll();
        UUIManager.S.ShowLoading(0);
        var operation = ResourcesManager.S.LoadLevelAsync(Map);
        PerView = UPerceptionView.Create();
        var time = Time.time;
        while (!operation.IsDone)
        {
            UUIManager.S.ShowLoading( Mathf.Clamp01((Time.time - time) /3));
            yield return null;
        }
        UUIManager.S.ShowMask(false);
        startTime = GetTime().Time;
        player = new NotifyPlayer(PerView);
    }

    protected override void Tick()
    {
        if (startTime < 0) return;
        if (frame == null) Replayer.NextFrame(out frame);

        if (frame == null) return;

        if (frame.time > GetTime().Time - startTime) return;
        foreach (var i in frame.GetNotify()) Process(i);
        frame = null;

    }

    private void Process(IMessage notify)
    {
        player.Process(notify);
    }
}