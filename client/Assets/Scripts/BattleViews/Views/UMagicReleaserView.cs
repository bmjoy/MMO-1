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
    public void SetCharacter(IBattleCharacter releaser, IBattleCharacter target)
    {
        CharacterTarget = target;
        CharacterReleaser = releaser;
    }

    public IBattleCharacter CharacterTarget { private set; get; }
    public IBattleCharacter CharacterReleaser { private set; get; }

    public string Key { get; internal set; }

    private readonly LinkedList<TimeLineViewPlayer> _players = new LinkedList<TimeLineViewPlayer>();

    void IMagicReleaser.PlayTimeLine(string layoutPath)
    {
        var timeLine =( PerView as IBattlePerception)?.GetTimeLineByPath(layoutPath);
        if (timeLine == null) return;
        _players.AddLast(new TimeLineViewPlayer(timeLine, this));
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

    public void PlaySound(Layout.TargetType target, string resourcesPath, string fromBone, float value)
    {
        var tar = target;
        if ((tar == Layout.TargetType.Releaser ? CharacterReleaser : CharacterTarget) is UCharacterView orgin)
        {
            var pos = orgin.GetBoneByName(fromBone).position;
            var clip = ResourcesManager.S.LoadResourcesWithExName<AudioClip>(resourcesPath);
            AudioSource.PlayClipAtPoint(clip, pos, value);
        }
    }

    public override IMessage ToInitNotify()
    {
        var createNotify = new Notify_CreateReleaser
        {
            Index = Index,
            ReleaserIndex = CharacterReleaser.Index,
            TargetIndex = CharacterTarget.Index,
            MagicKey = Key
        };
        return createNotify;
    }

    void IMagicReleaser.ShowDamageRanger(DamageLayout layout)
    {
#if UNITY_EDITOR
        if (layout.damageType == Layout.LayoutElements.DamageType.Rangle)
        {
            var target = layout.target == Layout.TargetType.Releaser ? CharacterReleaser : CharacterTarget;
            var pos = target.Transform.position + target.Rotation * layout.offsetPosition.ToUV3();
            _Rangges.Add(new DebugOfRange
            {
                Angle = layout.angle,
                EType = layout.effectType,
                forward = target.Transform.rotation,
                Pos = pos,
                Radius = layout.radius,
                targetsNums = 0,
                time = Time.time + .3f
            });
        }
#endif
    }

    private void Update()
    {
        TickTimeLine(PerView.GetTime());
    }

#if UNITY_EDITOR

    private class DebugOfRange
    {
        public EffectType EType;
        public UVector3 Pos;
        public Quaternion forward;
        public float Radius;
        public float Angle;
        public float targetsNums;
        public float time;
    }

    private readonly List<DebugOfRange> _Rangges = new List<DebugOfRange>();

    public void OnDrawGizmos()
    {
        foreach (var i in _Rangges)
        {
            if (i.time < Time.time) continue;
            DrawClire(i.Pos, i.forward, i.Radius, i.Angle);
            UnityEditor.Handles.Label(i.Pos, string.Format("A{1:0.0}_R{2:0.0}_{0}", i.targetsNums, i.Angle, i.Radius));
        }
    }

    private void DrawClire(UVector3 pos, Quaternion forward, float r, float a)
    {
        if (a > 360) a = 360;

        var c = Gizmos.color;
        Gizmos.color = Color.red;

        var qu2 = forward * Quaternion.Euler(0, a / 2, 0);
        var qu1 = forward * Quaternion.Euler(0, -a / 2, 0);
        var pos1 = qu1 * UVector3.forward * r + pos;
        var pos2 = qu2 * UVector3.forward * r + pos;
        Gizmos.DrawLine(pos, pos1);
        Gizmos.DrawLine(pos, pos2);
        UVector3 start = pos1;
        for (float i = -a / 2; i < a / 2 - 5;)
        {
            i += 5;
            var diffQu = forward * Quaternion.Euler(0, i, 0);
            var temp = diffQu * UVector3.forward * r + pos;
            Gizmos.DrawLine(start, temp);
            start = temp;
        }
        Gizmos.DrawLine(start, pos2);
        Gizmos.color = c;
    }

#endif



}
