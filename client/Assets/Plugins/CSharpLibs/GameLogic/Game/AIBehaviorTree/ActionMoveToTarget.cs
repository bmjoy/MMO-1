﻿using System;
using System.Collections.Generic;
using BehaviorTree;
using EngineCore;
using GameLogic.Game.Elements;
using GameLogic.Game.Perceptions;
using Layout.AITree;
using UVector3 = UnityEngine.Vector3;

namespace GameLogic.Game.AIBehaviorTree
{
	[TreeNodeParse(typeof(TreeNodeMoveToTarget))]
	public class ActionMoveToTarget:ActionComposite<TreeNodeMoveToTarget>
	{

		public ActionMoveToTarget(TreeNodeMoveToTarget n) : base(n) { }

		public override IEnumerable<RunStatus> Execute(ITreeRoot context)
		{
			var root = context as AITreeRoot;
			var index = root[AITreeRoot.TRAGET_INDEX];
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
            if (!root.GetDistanceByValueType(Node.valueOf, Node.distance, out float stopDistance))
            {
                yield return RunStatus.Failure;
                yield break;
            }
            var per = root.Perception as BattlePerception;
            //var offset = new GVector3(r);
			//float lastTime = root.Time-2;
			var pos = target.View.Transform.position;
            per.CharacterMoveTo(root.Character, pos);
			view = root.Character.View;

			while (root.Perception.Distance(target, root.Character) > stopDistance)
			{
                if (UVector3.Distance(pos, target.View.Transform.position) > stopDistance)
                {
                    per.CharacterMoveTo(root.Character, target.View.Transform.position);
                    pos = target.View.Transform.position;
                }

				if(!target.Enable)
				{
                    per.CharacterStopMove(root.Character);
					yield return RunStatus.Failure;
					yield break;
				}
				yield return RunStatus.Running;
			}

            var time = root.Time;
            if (time + 0.2f < root.Time)
            {
                yield return RunStatus.Running;
            }
			per.CharacterStopMove(root.Character);

			yield return RunStatus.Success;

		}

		private IBattleCharacter view;

		public override void Stop(ITreeRoot context)
		{
            var root = context as AITreeRoot;
            var per = root.Perception as BattlePerception;
            if (LastStatus.HasValue && LastStatus.Value == RunStatus.Running && view != null)
            {
                per.CharacterStopMove(root.Character);
            }
			base.Stop(context);
		}
	}
}

