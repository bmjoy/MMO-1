using System;
using System.Collections.Generic;
using BehaviorTree;
using GameLogic.Game.Elements;
using GameLogic.Game.LayoutLogics;
using GameLogic.Game.Perceptions;
using Layout.AITree;
using Layout.LayoutElements;
using Proto;
using UVector3 = UnityEngine.Vector3;
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

            if (!root.TryGetAction(out Proto.Action_NormalAttack normal))
            {
                yield return RunStatus.Failure;
                yield break;
            }

            if (root.Character.TryGetNormalAtt(root.Time, out EConfig.CharacterMagicData att, out bool isAppend))
            {


                var aiType = (MagicReleaseAITarget)att.AITargetType;

                TargetTeamType type;

                switch (aiType)
                {

                    case MagicReleaseAITarget.MatEnemy:
                        type = TargetTeamType.Enemy;
                        break;
                    case MagicReleaseAITarget.MatOwn:
                        type = TargetTeamType.Own;
                        break;
                    case MagicReleaseAITarget.MatOwnTeam:
                        type = TargetTeamType.OwnTeam;
                        break;
                    case MagicReleaseAITarget.MatOwnTeamWithOutSelf:
                        type = TargetTeamType.OwnTeamWithOutSelf;
                        break;
                    default:
                        type = TargetTeamType.All;
                        break;
                }

                root.GetDistanceByValueType(DistanceValueOf.ViewDistance, 0, out float v);

                var target = root.Perception.FindTarget(root.Character, type, v, 360, TargetSelectType.Nearest);

                if (!target)
                {
                    yield return RunStatus.Failure;
                    yield break;
                }
                float last = 0;
                while (BattlePerception.Distance(target, root.Character) > att.RangeMax)
                {
                    if (last + 0.3f > root.Time)
                    {
                        yield return RunStatus.Running;
                        continue;
                    }

                    last = root.Time;
                    if (!root.Character.MoveTo(target.Position))
                    {
                        if (context.IsDebug)
                            Attach("failure", $"can move");
                        yield return RunStatus.Failure;
                        yield break;
                    }
                    yield return RunStatus.Running;
                }

                root.Character.StopMove();
                var rTarget = new ReleaseAtTarget(root.Character, target);
                releaser = root.Perception.CreateReleaser(att.MagicKey, rTarget, ReleaserType.Magic);

                if (!isAppend)
                {
                    root.Character.IncreaseNormalAttack(root.Time);
                }
                else
                {
                    root.Character.ResetNormalAttack(root.Time);
                }
                while (!releaser.IsLayoutStartFinish)
                {
                    yield return RunStatus.Running;
                }

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
