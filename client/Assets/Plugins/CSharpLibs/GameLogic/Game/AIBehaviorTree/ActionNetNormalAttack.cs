﻿using System;
using System.Collections.Generic;
using BehaviorTree;
using EConfig;
using GameLogic.Game.Elements;
using GameLogic.Game.LayoutLogics;
using GameLogic.Game.Perceptions;
using Layout.AITree;
using Layout.LayoutElements;
using Proto;
using UVector3 = UnityEngine.Vector3;
using GameLogic;
namespace GameLogic.Game.AIBehaviorTree
{

    [TreeNodeParse(typeof(TreedNodeNetNomarlAttack))]
    public class ActionNetNormalAttack:ActionComposite<TreedNodeNetNomarlAttack>
    {
        private MagicReleaser releaser;

        public ActionNetNormalAttack(TreedNodeNetNomarlAttack node):base(node)
        {
        }

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
            var root = context as AITreeRoot;
            if (!root.TryGetAction(out Action_NormalAttack normal))
            {
                yield return RunStatus.Failure;
                yield break;
            }
            int count = 0;
            while (true)
            {
                MagicType mtype = count < 3 ? MagicType.MtNormal : MagicType.MtNormalAppend;
                if (count >= 3) count = 0;
                BattleCharacterMagic mc = null;
                if (!root.Character.HaveMagicByType(mtype))
                {
                    mtype = MagicType.MtNormal;
                }
                while (mc == null)
                {
                    root.Character.EachActiveMagicByType(mtype, root.Time,
                        (item) =>
                        {
                            mc = item;
                            return true;
                        });

                    yield return RunStatus.Running;
                }

                count++;
                var att = mc.Config;
                TargetTeamType type = mc.Config.GetTeamType();
                root.GetDistanceByValueType(DistanceValueOf.ViewDistance, 0, out float v);
                var target = root.Perception.FindTarget(root.Character, type, v, 360, true, TargetSelectType.Nearest);
                if (!target)
                {
                    yield return RunStatus.Failure;
                    yield break;
                }
                float last = 0;
                bool moving = false;

                while (BattlePerception.Distance(root.Character, target) > att.RangeMax)
                {
                    if (last + 0.5f > root.Time)
                    {
                        yield return RunStatus.Running;
                        continue;
                    }
                    last = root.Time;

                    if (!root.Character.MoveTo(target.Position,out _, att.RangeMax))
                    {
                        if (context.IsDebug) Attach("failure", $"can move");
                        yield return RunStatus.Failure;
                        yield break;
                    }
                    moving = true;
                    yield return RunStatus.Running;
                }             
                if (moving) root.Character.StopMove();
                var rTarget = new ReleaseAtTarget(root.Character, target);
                releaser = root.Perception.CreateReleaser(att.MagicKey, rTarget, ReleaserType.Magic, -1);
                if (!releaser)
                {
                    yield return RunStatus.Failure;
                    yield break;
                }
                root.Character.AttachMagicHistory(att.ID, root.Time, root.Character.AttackSpeed);
                while (!releaser.IsLayoutStartFinish)
                {
                    yield return RunStatus.Running;
                }
                yield return RunStatus.Running;

            }
        }

        public override void Stop(ITreeRoot context)
        {
            base.Stop(context);
            if (LastStatus == RunStatus.Running)
            {
                if (!releaser.IsLayoutStartFinish)
                {
                    releaser.StopAllPlayer();
                    releaser.SetState(ReleaserStates.ToComplete);
                }
            }
        }
    }
}
