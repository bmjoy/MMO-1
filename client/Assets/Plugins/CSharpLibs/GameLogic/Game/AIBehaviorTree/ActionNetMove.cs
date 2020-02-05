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

            if (!(root[AITreeRoot.ACTION_MESSAGE] is Action_MoveDir message))
            {
                yield return RunStatus.Failure;
                yield break;
            }

            root[AITreeRoot.ACTION_MESSAGE] = null;
            var target = message.Forward.ToUV3();
            root.Character.MoveForward(target);
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

