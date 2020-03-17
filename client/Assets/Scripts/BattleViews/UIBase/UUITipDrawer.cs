using System;
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


    public int DrawUUITipNameBar(int instanceId, string name,int level, int hp,int hpMax , bool owner, Vector3 offset, Camera c)
    {
        // UUITipHpBar tip;
        if (!UUIManager.Singleton.TryToGetTip(instanceId, out UUITipNameBar tip))
        {
            tip = UUIManager.S.CreateTip<UUITipNameBar>(true);
        }
        tip.SetInfo(name,level, hp, hpMax, owner);
        tip.LookAt(c);
        UUITip.Update(tip, offset);
       
        return tip.InstanceID;
    }

    public int DrawItemName(int instanceId, string name, bool owner, Vector3 offset, Camera c)
    { 
      // UUITipHpBar tip;
        if (!UUIManager.Singleton.TryToGetTip(instanceId, out UUIName tip))
        {
            tip = UUIManager.S.CreateTip<UUIName>(true);
        }
        tip.ShowName(name,owner);
        tip.LookAt(c);
        UUITip.Update(tip, offset);
        return tip.InstanceID;
    }

    #region Notify
    private  int DrawUUINotify(int instanceId, string notify)
    {
        if (!UUIManager.Singleton.TryToGetTip(instanceId, out UUINotify tip))
        {
            tip = UUIManager.Singleton.CreateTip<UUINotify>();
            tip.SetNotify(notify);
        }
        UUITip.Update(tip);
        return tip.InstanceID;
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


