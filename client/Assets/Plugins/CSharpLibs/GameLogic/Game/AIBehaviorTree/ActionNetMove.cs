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
            if (root.TryGetAction(out Action_StopMove stop))
            {
                if (root.Character.IsMoving)
                {
                    root.Character.StopMove(stop.StopPos.ToUV3());
                    yield return RunStatus.Success;
                    yield break;
                }
            }

            if (root.TryGetAction(out Action_MoveJoystick move))
            {
                if (root.Character.MoveTo(move.WillPos.ToUV3(), out _))
                {
                    if (root.IsDebug)
                    {
                        Attach("target", move.WillPos);
                        Attach("position", root.Character.Position);
                    }
                    yield return RunStatus.Success;
                    yield break;
                }
               
            }
            yield return RunStatus.Failure;
        }
    }
}

