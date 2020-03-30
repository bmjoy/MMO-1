﻿using System;
using UnityEngine;
using Tips;
using System.Collections.Generic;

public struct AppNotify
{
    public string Message;
    public float endTime;
}

public class UUITipDrawer:XSingleton<UUITipDrawer>
{

    private class NotifyMessage
    {
        public float time;
        public string message;
        public int ID = -1;

        public static implicit operator NotifyMessage(AppNotify notify)
        {
            return new NotifyMessage() { message = notify.Message, time = notify.endTime };
        }
    }


    public int DrawUUITipNameBar(int instanceId, string name,
        int level, int hp, int hpMax, int mp, int mpMax, bool owner, Vector3 offset, Camera c)
    {
        UUIManager.S.TryToGetTip(instanceId, out UUITipNameBar tip);
        if (tip != null)
        {
            tip.SetInfo(name, level, hp, hpMax, mp, mpMax, owner);
            tip.LookAt(c);
            UUITip.Update(tip, offset);
            return tip.InstanceID;
        }
        return instanceId;
    }

    public int DrawItemName(int instanceId, string name, bool owner, Vector3 offset, Camera c)
    {
        UUIManager.S.TryToGetTip(instanceId, out UUIName tip);
        if (tip != null)
        {
            tip.ShowName(name, owner);
            tip.LookAt(c);
            UUITip.Update(tip, offset);
            return tip.InstanceID;
        }
        return instanceId;
    }

    #region Notify
    private int DrawUUINotify(int instanceId, string notify)
    {
        UUIManager.Singleton.TryToGetTip(instanceId, out UUINotify tip);
        if (tip != null)
        {
            //tip = UUIManager.Singleton.CreateTip<UUINotify>();
            tip.SetNotify(notify);
            UUITip.Update(tip);
            return tip.InstanceID;
        }
        return instanceId;
    }
        
    private readonly List<NotifyMessage> notifys= new List<NotifyMessage>();
    private readonly Queue<NotifyMessage> _dels = new Queue<NotifyMessage>();


    public void ShowNotify(AppNotify notify)
    {
        notifys.Add(notify);
    }
    public void ShowNotify(string notify, float dur = 4.5f)
    {
        notifys.Add(new NotifyMessage{ message = notify, time = Time.time +dur });
        Debug.Log(notify);
    }
    #endregion

    public void Update()
    {
        foreach (var i in notifys)
        {
            i.ID = DrawUUINotify(i.ID, i.message);
            if (i.time < Time.time)
            {
                _dels.Enqueue(i);
            }
        }

        while (_dels.Count > 0)
        {
            notifys.Remove(_dels.Dequeue());
        }

    }
}


