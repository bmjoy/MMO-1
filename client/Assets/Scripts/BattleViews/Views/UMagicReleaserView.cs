﻿using UnityEngine;
using System.Collections;
using GameLogic.Game.Elements;
using Google.Protobuf;
using Proto;
using EngineCore.Simulater;
using Layout.LayoutElements;
using GameLogic;
using UVector3 = UnityEngine.Vector3;
using System.Collections.Generic;

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
        for (float i = -a/2; i < a/2 -5; )
        {
            i += 5;
            var diffQu = forward * Quaternion.Euler(0,i,0);
            var temp = diffQu * UVector3.forward * r + pos;
            Gizmos.DrawLine(start, temp);
            start = temp;
        }
       Gizmos.DrawLine(start, pos2);
        Gizmos.color = c;
    }
#endif


}
