using System;
using System.Collections.Generic;
using BehaviorTree;
using GameLogic.Game.Elements;
using Layout.AITree;

namespace GameLogic.Game.AIBehaviorTree
{
	[TreeNodeParse(typeof(TreeNodeDistancTarget))]
	public class ActionDistancTarget : ActionComposite<TreeNodeDistancTarget>
	{
		public ActionDistancTarget(TreeNodeDistancTarget node):base(node)
		{
		}

		public override IEnumerable<RunStatus> Execute(ITreeRoot context)
		{
			var root = context as AITreeRoot;
			var index = root["TargetIndex"];
			if (index == null)
			{
				yield return RunStatus.Failure;
				yield break;
			}

            if (!(root.Perception.State[(int)index] is BattleCharacter target))
            {
                yield return RunStatus.Failure;
                yield break;
            }

            if (!root.GetDistanceByValueType(Node.valueOf, Node.distance, out float distance))
            {
                yield return RunStatus.Failure;
                yield break;
            }
            switch (Node.compareType)
			{
				case CompareType.Less:
					if (root.Perception.Distance(target, root.Character) > distance)
						yield return RunStatus.Failure;
					else
						yield return RunStatus.Success;
					break;
				case CompareType.Greater:
					if (root.Perception.Distance(target, root.Character) > distance)
						yield return RunStatus.Success;
					else
						yield return RunStatus.Failure;
					break;
			}


		}

		//private float distance;
	}
}

