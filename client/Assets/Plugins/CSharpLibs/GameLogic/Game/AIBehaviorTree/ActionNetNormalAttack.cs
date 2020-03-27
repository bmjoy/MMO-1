using System;
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

            while (true)
            {
                BattleCharacterMagic mc = null;
                while (mc == null)
                {
                    root.Character.EachActiveMagicByType(MagicType.MtNormal, root.Time, (item) =>
                        {
                            mc = item;
                            return true;
                        });
                    yield return RunStatus.Running;
                }
                var att = mc.Config;
                TargetTeamType type = mc.Config.GetTeamType();
                root.GetDistanceByValueType(DistanceValueOf.ViewDistance, 0, out float v);
                var target = root.Perception.FindTarget(root.Character, type, v, 360, true, TargetSelectType.Nearest);
                while (true)
                {
                    if (!target)
                    {
                        yield return RunStatus.Failure;
                        yield break;
                    }
                    var last = root.Time;
                    if (!root.Character.MoveTo(target.Position, out _, att.RangeMax))
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
                var rTarget = new ReleaseAtTarget(root.Character, target);
                releaser = root.Perception.CreateReleaser(att.MagicKey, root.Character, rTarget, ReleaserType.Magic, -1);
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
            if (LastStatus == RunStatus.Running)
            {
                if (!releaser.IsLayoutStartFinish)
                {
                    releaser.StopAllPlayer();
                    releaser.SetState(ReleaserStates.ToComplete);
                }
                if (context is AITreeRoot root)
                {
                    if (root?.Character?.IsMoving == true) root.Character.StopMove();
                }
            }
            base.Stop(context);
        }
    }
}
