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
			
			var distance = Node.distance.Value/100f;


			if (!root.GetDistanceByValueType(Node.valueOf, distance, out distance))
			{
				yield return RunStatus.Failure;
				yield break;
			}

			if (!root.TryGetTarget(out BattleCharacter targetCharacter))
			{
				if (root.IsDebug) Attach("failure", "notarget");
				yield return RunStatus.Failure;
				yield break;
			}

			var noraml = (root.Character.Position - targetCharacter.Position).normalized;
			var target = noraml * distance + root.Character.Position;



			while ((root.Character.Position - target).magnitude > 0.2f)
			{
				if (!root.Character.MoveTo(target, out target))
				{
					if (root.IsDebug) Attach("failure", "move failure");
					yield return RunStatus.Failure;
					yield break;
				}
				var start = root.Time;
				while (start + 1f > root.Time)
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
                root.Character.StopMove();
            }
        }
	}
}

