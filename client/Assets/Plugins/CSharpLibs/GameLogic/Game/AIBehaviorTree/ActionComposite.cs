using System;
using BehaviorTree;
using Layout.AITree;

namespace GameLogic.Game.AIBehaviorTree
{
	public abstract class ActionComposite<T>:Composite where T:TreeNode
	{
		public ActionComposite(T Node)
		{
			this.Node = Node;
		}

        public T Node {  private set; get; }

		public override Composite FindGuid(string id)
		{
			if (this.Guid == id) return this;
			return null;
		}


	}
}

