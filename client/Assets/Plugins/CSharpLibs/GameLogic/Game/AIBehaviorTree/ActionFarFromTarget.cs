﻿using System.Collections.Generic;
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

			if (root.TryGetTarget(out BattleCharacter targetCharacter))
			{
				yield return RunStatus.Failure;
				yield break;
			}

			var noraml = (root.Character.Position - targetCharacter.Position).normalized;
			var target = noraml * distance + root.Character.Position;

			root.Character.MoveTo(target);

			while ((root.Character.Position - target).magnitude > 0.2f)
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
                root.Character.StopMove();
            }
        }
	}
}

