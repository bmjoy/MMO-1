﻿using System;
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
                    Attach("failure", $"nofound magic {message.MagicId}");
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

            var type = magic.GetTeamType();
            var target = root.Perception.FindTarget(root.Character, type, magic.RangeMax, 360, true, TargetSelectType.Nearest);
            if (!target)
            {
                yield return RunStatus.Failure;
                yield break;
            }
            bool hadMove = false;
            while (BattlePerception.Distance(root.Character, target) > magic.RangeMax)
            {
                if (!target)
                {
                    yield break;
                }
                if (!root.Character.MoveTo(target.Position, out _, magic.RangeMax))
                {
                    if (context.IsDebug) Attach("failure", $"can't move");
                    yield return RunStatus.Failure;
                    yield break;
                }
                float lastTime = root.Time;
                hadMove = true;
                while (lastTime + .5 > root.Time)
                {
                    yield return RunStatus.Running;
                }
            }

            if (hadMove) root.Character.StopMove();
            releaser = root.Perception.CreateReleaser(magic.MagicKey,
                new ReleaseAtTarget(root.Character, target), ReleaserType.Magic);
            root.Character.AttachMagicHistory(magic.ID, root.Time);
            if (releaser != null)
            {
                while (!releaser.IsLayoutStartFinish)
                    yield return RunStatus.Running;
                yield return RunStatus.Success;
                yield break ;
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
                releaser?.Cancel();
            }
            base.Stop(context);
        }

    }
}

