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

            var magic = root.Character.GetMagicById(message.MagicId);

            if (magic == null)
            {
                if (context.IsDebug)
                {
                    Attach("failure",$"nofound magic {message.MagicId}");
                }
                yield return RunStatus.Failure;
                yield break;
            }

            if (!root.Character.IsCoolDown(magic.ID, root.Time, false))
            {
                if (context.IsDebug)
                {
                    Attach("failure", $"{message.MagicId} cd not completed");
                }
                yield return RunStatus.Failure;
                yield break;
            }

            var target = root.Perception.FindTarget(message.Target);
            if (!target)
            {
                if (context.IsDebug)
                {
                    Attach("failure", $"target no found!");
                }
            }

            var lastTime = root.Time;
            while (BattlePerception.Distance(root.Character, target) > magic.RangeMax)
            {
                if (!target)
                {
                    yield break;
                }
                root.Character.MoveTo(target.Position);
                while (lastTime + .5 > root.Time)
                {
                    yield return RunStatus.Running;
                }
            }

            root.Character.StopMove();
            releaser = root
                .Perception.CreateReleaser(magic.MagicKey, new ReleaseAtTarget(root.Character, target), ReleaserType.Magic);
            while (!releaser.IsLayoutStartFinish) yield return RunStatus.Running;
            

            yield return RunStatus.Success;

        }

        public override void Stop(ITreeRoot context)
        {
            if (LastStatus == RunStatus.Running)
            {
                releaser?.Cancel();
            }
            base.Stop(context);
        }

    }
}

