using System.Collections.Generic;
using BehaviorTree;
using Layout.AITree;
using Proto;

namespace GameLogic.Game.AIBehaviorTree
{
    [TreeNodeParse(typeof(TreeNodeNetActionMove))]
    public class ActionNetMove:ActionComposite<TreeNodeNetActionMove>
    {
        public ActionNetMove(TreeNodeNetActionMove node):base(node)
        {
        }

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
            var root = context as AITreeRoot;

            if (!root.TryGetAction(out Action_MoveDir message))
            {
                if (context.IsDebug)
                    Attach("failure", $"{root[AITreeRoot.ACTION_MESSAGE]} is not move action");
                yield return RunStatus.Failure;
                yield break;
            }

            var target = message.Forward.ToUV3();
            if (target.magnitude > 0.001f)
                target = target.normalized * (message.Fast ? 1 : 0.5f);
            if (!root.Character.MoveForward(target, message.Position.ToUV3()))
            {
                if (root.IsDebug)
                {
                    Attach("failure", "nomove");
                }
                yield return RunStatus.Failure;
                yield break;
            }
            if (context.IsDebug)
            {
                Attach("move_dir", target);
            }
            yield return RunStatus.Success;
        }

        public override void Stop(ITreeRoot context)
        {
            base.Stop(context);
            if (LastStatus == RunStatus.Running)
            {
                var root = context as AITreeRoot;
                root.Character.StopMove();
            }
        }
    }
}

