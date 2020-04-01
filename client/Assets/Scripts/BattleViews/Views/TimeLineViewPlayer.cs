using System;
using System.Collections.Generic;
using System.Reflection;
using GameLogic.Game.LayoutLogics;
using Layout.LayoutElements;
using UnityEngine;

public class TimeLineViewPlayer : TimeLinePlayerBase
{

    #region  EnableLayout
    static TimeLineViewPlayer()
    {
        var type = typeof(TimeLineViewPlayer);
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
        foreach (var i in methods)
        {
            if (!(i.GetCustomAttributes(typeof(HandleLayoutAttribute), false) is HandleLayoutAttribute[] atts) || atts.Length == 0)
                continue;
            _handler.Add(atts[0].HandleType, i);
        }
    }

    private static readonly Dictionary<Type, MethodInfo> _handler = new Dictionary<Type, MethodInfo>();

    private static void ActiveLayout(LayoutBase layout, TimeLineViewPlayer player)
    {
        if (_handler.TryGetValue(layout.GetType(), out MethodInfo m))
        {
            m.Invoke(null, new object[] { player, layout });
        }
        else
        {
            throw new Exception("No Found handle Type :" + layout.GetType());
        }
    }

    #endregion

    #region RepeatTimeLine
    [HandleLayout(typeof(RepeatTimeLine))]
    public static void RepeatTimeLineActive(TimeLineViewPlayer player, LayoutBase layoutBase)
    {
        if (layoutBase is RepeatTimeLine r) player.Repeat(r.RepeatCount);
    }
    #endregion

    #region ParticleLayout
    [HandleLayout(typeof(ParticleLayout))]
    public static void ParticleActive(TimeLineViewPlayer player, LayoutBase layoutBase)
    {
        var layout = layoutBase as ParticleLayout;
        var particle = player.RView.PerView.CreateParticlePlayer(player.RView, layout);
        if (particle == null) return;
        switch (layout.destoryType)
        {
            case ParticleDestoryType.LayoutTimeOut:
                player.AttachParticle(particle);
                break;
            case ParticleDestoryType.Time:
                particle.AutoDestory(layout.destoryTime);
                break;
            case ParticleDestoryType.Normal:
                player.RView.AttachParticle(particle);
                break;
                
        }
    }


    #endregion
    #region LookAtTarget
    //LookAtTarget
    [HandleLayout(typeof(LookAtTarget))]
    public static void LookAtTargetActive(TimeLineViewPlayer linePlayer, LayoutBase layoutBase)
    {
        if (layoutBase is LookAtTarget)
            linePlayer.RView.CharacterReleaser.LookAtTarget(linePlayer.RView.CharacterTarget.Index);
    }
    #endregion

    #region MotionLayout
    [HandleLayout(typeof(MotionLayout))]
    public static void MotionActive(TimeLineViewPlayer player, LayoutBase layoutBase)
    {
        var layout = layoutBase as MotionLayout;
        if (layout.targetType == Layout.TargetType.Releaser)
        {
            player.RView.CharacterReleaser?.PlayMotion(layout.motionName);
        }
        else if (layout.targetType == Layout.TargetType.Target)
        {
            player.RView.CharacterTarget?.PlayMotion(layout.motionName);
        }
    }
    #endregion

    #region PlaySoundLayout
    [HandleLayout(typeof(PlaySoundLayout))]
    public static void PlaySoundLayout(TimeLineViewPlayer player, LayoutBase layoutBase)
    {
        var sound = layoutBase as PlaySoundLayout;
        var tar = sound.target;
        if ((tar == Layout.TargetType.Releaser ? player.RView.CharacterReleaser : player.RView.CharacterTarget)
            is UCharacterView orgin)
        {
            if (orgin)
            {
                var pos = orgin.GetBoneByName(sound.fromBone).position;
                ResourcesManager.S.LoadResourcesWithExName<AudioClip>(sound.resourcesPath, (clip) =>
                {
                    AudioSource.PlayClipAtPoint(clip, pos, sound.value);
                });
            }
        }
    }
    #endregion
 


    public TimeLineViewPlayer(TimeLine line, UMagicReleaserView view)
        : base(line)
    {
        this.RView = view;
    }

    public UMagicReleaserView RView { get; }

    protected override void EnableLayout(LayoutBase layout)
    {
        if (LayoutBase.IsViewLayout(layout)) ActiveLayout(layout, this);
    }

    private readonly List<IParticlePlayer> _players = new List<IParticlePlayer>();

    protected override void OnDestory()
    {
        base.OnDestory();
        foreach (var i in _players)
        {
            i.DestoryParticle();
        }
    }

    public void AttachParticle(IParticlePlayer particle)
    {
        _players.Add(particle);
    }
}
