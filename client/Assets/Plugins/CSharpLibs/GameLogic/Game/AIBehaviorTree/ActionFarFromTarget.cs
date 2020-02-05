using System.Collections.Generic;
using BehaviorTree;
using GameLogic.Game.Elements;
using GameLogic.Game.Perceptions;
using Layout.AITree;
using UnityEngine;

namespace GameLogic.Game.AIBehaviorTree
{
    [TreeNodeParse(typeof(TreeNodeFarFromTarget))]
	public class ActionFarFromTarget:ActionComposite<TreeNodeFarFromTarget>
	{
		public ActionFarFromTarget(TreeNodeFarFromTarget node):base(node)
		{
		}

		public override IEnumerable<RunStatus> Execute(ITreeRoot context)
		{
			var root = context as AITreeRoot;
			var per = root.Perception as BattlePerception;
			var distance = Node.distance;
			if (!root.GetDistanceByValueType(Node.valueOf, distance, out distance))
			{
				yield return RunStatus.Failure;
				yield break;
			}

			Vector3 target;

			var targetIndex = root[AITreeRoot.TRAGET_INDEX];
			if (targetIndex == null)
			{
				yield return RunStatus.Failure;
				yield break;
			}
            if (!(root.Perception.State[(int)targetIndex] is BattleCharacter targetCharacter))
            {
                yield return RunStatus.Failure;
                yield break;
            }

            var noraml =(root.Character.View.Transform.position - targetCharacter.View.Transform.position).normalized;
			target = noraml * distance + root.Character.View.Transform.position;

            per.CharacterMoveTo(root.Character, target);

            while ((root.Character.View.Transform.position-target).magnitude > 0.2f)
			{
				yield return RunStatus.Running;
			}

			root.Character.StopMove();
			yield return RunStatus.Success;


		}

        public override void Stop(ITreeRoot context)
        {
            base.Stop(context);
            if (LastStatus == RunStatus.Running)
            {
                var root = context as AITreeRoot;
                var per = root.Perception as BattlePerception;
                per.CharacterStopMove(root.Character);}
        }
	}
}

