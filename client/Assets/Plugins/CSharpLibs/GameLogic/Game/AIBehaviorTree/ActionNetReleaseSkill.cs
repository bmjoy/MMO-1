using System;
using System.Collections.Generic;
using BehaviorTree;
using GameLogic.Game.Elements;
using GameLogic.Game.LayoutLogics;
using GameLogic.Game.Perceptions;
using Layout.AITree;
using Proto;

namespace GameLogic.Game.AIBehaviorTree
{
    [TreeNodeParse(typeof(TreeNodeNetActionSkill))]
    public class ActionNetReleaseSkill:ActionComposite<TreeNodeNetActionSkill>
    {
        private MagicReleaser releaser;

        public ActionNetReleaseSkill(TreeNodeNetActionSkill n):base(n)
        {
        }

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
            var root = context as AITreeRoot;

            if (!root.TryGetAction(out Action_ClickSkillIndex message))
            {
                if (context.IsDebug)
                {
                    Attach("failure", $"not release skill action");
                }
                yield return RunStatus.Failure;
                yield break;
            }

            if (!root.Character.TryGetActiveMagicById(message.MagicId, root.Time, out BattleCharacterMagic magic))
            {
                if (context.IsDebug)
                {
                    Attach("failure", $"get skill faiure is cd");
                }
                yield return RunStatus.Failure;
                yield break;
            }

            var type = magic.Config.GetTeamType();
            var dis = magic.Config.RangeMax;

            root.GetDistanceByValueType(DistanceValueOf.ViewDistance, dis, out dis);

            var target = root.Perception.FindTarget(root.Character, type, dis, 360, true, TargetSelectType.ForwardNearest);
            if (!target)
            {
                yield return RunStatus.Failure;
                yield break;
            }

            if (BattlePerception.Distance(target, root.Character) > magic.Config.RangeMax)
            {
                while (true)
                {
                    if (!target)
                    {
                        yield return RunStatus.Failure;
                        yield break;
                    }
                    var last = root.Time;
                    if (!root.Character.MoveTo(target.Position, out _, magic.Config.RangeMax))
                    {
                        if (context.IsDebug) Attach("failure", $"can move");
                        yield return RunStatus.Failure;
                        yield break;
                    }
                    while (last + 0.4f > root.Time && root.Character.IsMoving)
                    {
                        yield return RunStatus.Running;
                    }
                    if (!root.Character.IsMoving) break;
                }
            }
            else
            {
                root.Character.TryToSetPosition(message.Position.ToUV3(), message.Rotation.Y);
            }

            if (!root.Character.SubMP(magic.Config.MPCost))
            {
                yield return RunStatus.Failure;
                yield break;
            }

            var rt = new ReleaseAtTarget(root.Character, target);
            releaser = root.Perception.CreateReleaser(magic.Config.MagicKey, root.Character,rt, ReleaserType.Magic, ReleaserModeType.RmtMagic, 0);
            if (releaser)
            {
                releaser.SetParam(magic.Params);
                root.Character.AttachMagicHistory(magic.ConfigId, root.Time);
                while (!releaser.IsLayoutStartFinish)  yield return RunStatus.Running;
                yield return RunStatus.Success;
                yield break;
            }
            else
            {
                yield return RunStatus.Failure;
                yield break;
            }
        }

        public override void Stop(ITreeRoot context)
        {
            if (LastStatus == RunStatus.Running)
            {
                if(releaser!=null)
                releaser.Cancel();
                releaser = null;
            }
            base.Stop(context);
        }

    }
}

