using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BehaviorTree;
using Layout.AITree;

namespace GameLogic.Game.AIBehaviorTree
{
	//TreeNodeWaitForSeconds
	[TreeNodeParse(typeof(TreeNodeWaitForSeconds))]
	public class BWaitForSeconds : ActionComposite<TreeNodeWaitForSeconds>
    {

		public BWaitForSeconds(TreeNodeWaitForSeconds n) : base(n) { }

        public override IEnumerable<BehaviorTree.RunStatus> Execute(ITreeRoot context)
        {
			float Seconds = Node.seconds;
			var root = context as AITreeRoot;
			switch (Node.valueOf)
			{
				case WaitTimeValueOf.AttackSpeed:
					{
                        Seconds = root.Character.AttackSpeed;
					}
					break;
			}
			float time = context.Time;
			//var lastTime = time;
			while (time + Seconds >= context.Time)
                yield return BehaviorTree.RunStatus.Running;
            yield return BehaviorTree.RunStatus.Success;
        }

		public override Composite FindGuid(string id)
		{
			if (Guid == id) return this;
			return null;
		}


    }
}
