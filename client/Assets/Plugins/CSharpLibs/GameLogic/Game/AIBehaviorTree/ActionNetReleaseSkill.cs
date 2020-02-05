using System;
using System.Collections.Generic;
using BehaviorTree;
using Layout.AITree;

namespace GameLogic.Game.AIBehaviorTree
{
    [TreeNodeParse(typeof(TreeNodeNetActionSkill))]
    public class ActionNetReleaseSkill:ActionComposite<TreeNodeNetActionSkill>
    {
        public ActionNetReleaseSkill(TreeNodeNetActionSkill n):base(n)
        {
        }

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
            var root = context as AITreeRoot;
            var message = root[AITreeRoot.ACTION_MESSAGE] as Proto.Action_ClickSkillIndex;
            if (message == null)
            {
                yield return RunStatus.Failure;
                yield break;
            }

            root[AITreeRoot.ACTION_MESSAGE] = null;
            if (!root.Character.HasMagicKey(message.MagicKey))
            {
                yield return RunStatus.Failure;
                yield break;
            }

            var magic = root.Character.GetMagicByKey(message.MagicKey);
            if (!root.Character.IsCoolDown(magic.ID, root.Time, false))
            {
                yield return RunStatus.Failure;
                yield break;
            }

            root[AITreeRoot.SELECT_MAGIC_ID] = magic.ID;
            yield return RunStatus.Success;

        }

        public void SetTreeNode(TreeNode node)
        {
            //throw new NotImplementedException();
        }
    }
}

