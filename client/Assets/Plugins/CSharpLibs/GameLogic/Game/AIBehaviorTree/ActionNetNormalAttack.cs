using System;
using System.Collections.Generic;
using BehaviorTree;
using Layout.AITree;

namespace GameLogic.Game.AIBehaviorTree
{

    [TreeNodeParse(typeof(TreedNodeNetNomarlAttack))]
    public class ActionNetNormalAttack:ActionComposite<TreedNodeNetNomarlAttack>
    {
        public ActionNetNormalAttack(TreedNodeNetNomarlAttack node):base(node)
        {
        }

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
            yield return RunStatus.Failure;
            yield break;
        }
    }
}
