using UnityEngine;
using GameLogic.Game.Elements;
using Google.Protobuf;
using Proto;
using Layout.LayoutElements;
using GameLogic;
using UVector3 = UnityEngine.Vector3;
using System.Collections.Generic;
using GameLogic.Game.LayoutLogics;
using System.Reflection;
using System;
using GameLogic.Game.Perceptions;
using EngineCore.Simulater;

public class UMagicReleaserView : UElementView, IMagicReleaser
{
    public void SetCharacter(int releaser, int target, UVector3 pos)
    {
        CharacterTarget = PerView.GetViewByIndex<UCharacterView>(target);
        CharacterReleaser = PerView.GetViewByIndex<UCharacterView>(releaser);
        RIndex = releaser;
        TIndex = target;
    }

    public UVector3 TargetPos;
    private int RIndex;
    private int TIndex;

    public IBattleCharacter CharacterTarget { private set; get; }
    public IBattleCharacter CharacterReleaser { private set; get; }

    public string Key { get; internal set; }

    private readonly LinkedList<TimeLineViewPlayer> _players = new LinkedList<TimeLineViewPlayer>();

    void IMagicReleaser.PlayTimeLine(string layoutPath,int targetIndex, int type)
    {
#if UNITY_SERVER || UNITY_EDITOR
        CreateNotify(new Notify_PlayTimeLine
        {
            Path = layoutPath,
            Index = Index,
            TargetIndex = targetIndex,
            Type =type
        });
#endif
#if !UNITY_SERVER

        var eType = (Layout.EventType)type;

        var tar = PerView.GetViewByIndex<UCharacterView>(targetIndex);
        PlayLine((PerView as IBattlePerception)?.GetTimeLineByPath(layoutPath),tar, eType);
#endif
    }

    private void PlayLine(TimeLine timeLine, IBattleCharacter eventTarget, Layout.EventType type)
    {
        if (timeLine == null) return;
        _players.AddLast(new TimeLineViewPlayer(timeLine, this, eventTarget, type));
    }

    void IMagicReleaser.PlayTest(TimeLine line)
    {
        PlayLine(line, this.CharacterTarget, Layout.EventType.EVENT_START);
    }

    private void TickTimeLine(GTime time)
    {
        var current = _players.First;
        while (current != null)
        {
            if (current.Value.Tick(time))
            {
                current.Value.Destory();
                _players.Remove(current);
            }
            current = current.Next;
        }
    }


    private readonly List<IParticlePlayer> pPlayers  = new List<IParticlePlayer>();

    internal void AttachParticle(IParticlePlayer particle)
    {
        pPlayers.Add(particle);
    }

    public override IMessage ToInitNotify()
    {
        var createNotify = new Notify_CreateReleaser
        {
            Index = Index,
            ReleaserIndex = RIndex,
            TargetIndex = TIndex,
            MagicKey = Key,
            Position = TargetPos.ToPV3()
        };
        return createNotify;
    }

    private void OnDestroy()
    {
        foreach (var i in pPlayers)
            i.DestoryParticle();
        pPlayers.Clear();
    }

    private void Update()
    {
        TickTimeLine(PerView.GetTime());
    }


    void IMagicReleaser.ShowDamageRanger(DamageLayout layout)
    {
#if UNITY_EDITOR
        if (layout.RangeType.damageType == Layout.LayoutElements.DamageType.Rangle)
        {
            var target = layout.target == Layout.TargetType.Releaser ? CharacterReleaser : CharacterTarget;
            var pos = target.Transform.position + target.Rotation * layout.RangeType.offsetPosition.ToUV3();
            DamageRangeDebuger.TryGet(this.gameObject)
                .AddDebug(layout, pos, target.Transform.rotation);
        }
#endif
    }


}
