using System;
using Proto;
using UGameTools;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ExcelConfig;
using GameLogic.Game.Perceptions;
using GameLogic.Game.Elements;
using Google.Protobuf;
using EConfig;
using System.Reflection;
using GameLogic.Utility;
using Windows;

/// <summary>
/// 游戏中的通知播放者
/// </summary>
public class NotifyPlayer
{
    private struct NotifyMapping
    {
        public NeedNotifyAttribute Attr { set; get; }
        public MethodInfo Method { set; get; }
    }
    private readonly Dictionary<Type, NotifyMapping> PerceptionInvokes = new Dictionary<Type, NotifyMapping>();
    private readonly Dictionary<Type, NotifyMapping> ElementInvokes = new Dictionary<Type, NotifyMapping>();

    public IBattlePerception PerView { set; get; }


    #region Events
    public Action<Notify_CharacterExp> OnAddExp;
    public Action<IBattleCharacter> OnCreateUser;
    public Action<Notify_PlayerJoinState> OnJoined;
    public Action<Notify_DropGold> OnDropGold;
    #endregion

    public NotifyPlayer(UPerceptionView view)
    {
        PerView = view;
        var invokes = typeof(IBattlePerception).GetMethods();
        foreach (var i in invokes)
        {
            var att = i.GetCustomAttribute<NeedNotifyAttribute>();
            if (att == null) continue;
            PerceptionInvokes.Add(att.NotifyType, new NotifyMapping { Method = i, Attr = att });
        }

        AddType<IBattleElement>();
        AddType<IBattleCharacter>();
        AddType<IBattleMissile>();
        AddType<IMagicReleaser>();
        AddType<IBattleItem>();
    }

    private void AddType<T>()
    {
        var invokes = typeof(T).GetMethods();
        foreach (var i in invokes)
        {
            var att = i.GetCustomAttribute<NeedNotifyAttribute>();
            if (att == null) continue;
            if (ElementInvokes.ContainsKey(att.NotifyType))
            {
                Debug.LogError($"{att.NotifyType} had add");
                continue;
            }
            ElementInvokes.Add(att.NotifyType, new NotifyMapping { Method = i, Attr = att });
            Debug.Log($"{ typeof(T)} handle notify {att.NotifyType}");
        }
    }

    private const string INDEX = "Index";

    /// <summary>
    /// 处理网络包的解析
    /// </summary>
    /// <param name="notify">Notify.</param>
    public void Process(IMessage notify)
    {

        Debug.Log($"{notify.GetType().Name}->{notify}");
        //优先处理 perception 创建元素
        if (PerceptionInvokes.TryGetValue(notify.GetType(), out NotifyMapping m))
        {
            var ps = new List<object>();
            foreach (var i in m.Attr.FieldNames)
            {
                ps.Add(notify.GetType().GetProperty(i).GetValue(notify));
            }
            var go = m.Method.Invoke(PerView, ps.ToArray());

            if (go is UElementView el)
            {
                el.SetPrecpetion(PerView as UPerceptionView);
                if((el is IBattleElement b)) b.JoinState((int)notify.GetType().GetProperty(INDEX).GetValue(notify));
            }

            if (go is UCharacterView c)
            {
                OnCreateUser?.Invoke(c);
            }

            if (go is UBattleItem item)
            {
                Debug.Log($"Drop: {item}");
            }
            return;
        }
        //查找元素消息
        //index
        var property = notify.GetType().GetProperty(INDEX);
        if (property != null)
        {
            var index = (int)property.GetValue(notify);
            var per = PerView as UPerceptionView;
            var v = per.GetViewByIndex(index);
            if (v == null)
            {
                Debug.LogError($"No found index {index} by {notify.GetType()} -> {notify}");
            }
            //Debug.Log($"{v.GetType()} -> {notify.GetType()}");
            if (ElementInvokes.TryGetValue(notify.GetType(), out NotifyMapping elI))
            {
                //Debug.Log($"invoke {elI.Method.Name}");
                var ps = new List<object>();
                foreach (var f in elI.Attr.FieldNames)
                {
                    ps.Add(notify.GetType().GetProperty(f).GetValue(notify));
                }
                elI.Method.Invoke(v, ps.ToArray());
                return;
            }
        }

        //处理特别消息
        if (notify is Notify_PlayerJoinState p)
        {
            OnJoined?.Invoke(p);
        }
        else if (notify is Notify_DropGold dropGold)
        {
            OnDropGold?.Invoke(dropGold);
        }
        else if (notify is Notify_CharacterExp exp)
        {
            OnAddExp?.Invoke(exp);
        }
        else
        {
            Debug.LogError($"NO Handle:{notify.GetType()}->{notify}");
        }
    }

}


