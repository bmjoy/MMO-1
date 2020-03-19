using System;
using System.Collections.Generic;
using System.Reflection;
using GameLogic.Game.LayoutLogics;
using Layout.LayoutElements;

public class TimeLineViewPlayer : TimeLinePlayerBase
{

    #region  EnableLayout
    static TimeLineViewPlayer()
    {
        var type = typeof(LayoutBaseLogic);
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
                //自动销亡
                break;
        }
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
        player.RView.PlaySound(sound.target, sound.resourcesPath, sound.fromBone, sound.value);
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
        if (LayoutBase.IsViewLayout(layout))
            ActiveLayout(layout, this);
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
