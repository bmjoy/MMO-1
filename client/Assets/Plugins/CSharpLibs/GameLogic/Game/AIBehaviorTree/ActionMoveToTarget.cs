using System;
using System.Collections.Generic;
using BehaviorTree;
using EngineCore;
using GameLogic.Game.Elements;
using GameLogic.Game.Perceptions;
using Layout.AITree;
using UVector3 = UnityEngine.Vector3;

namespace GameLogic.Game.AIBehaviorTree
{
	[TreeNodeParse(typeof(TreeNodeMoveCloseTarget))]
	public class ActionMoveToTarget : ActionComposite<TreeNodeMoveCloseTarget>
	{

		public ActionMoveToTarget(TreeNodeMoveCloseTarget n) : base(n) { }

		public override IEnumerable<RunStatus> Execute(ITreeRoot context)
		{
			var root = context as AITreeRoot;
			if (!root.TryGetTarget(out BattleCharacter target))
			{
				if (context.IsDebug) Attach("failure", $"nofound target by target");
				yield return RunStatus.Failure;
				yield break;
			}

			if (!root.GetDistanceByValueType(Node.valueOf, Node.distance/100f, out float stopDistance))
			{
				if (context.IsDebug)
					Attach("failure", $"nofound stop distance");
				yield return RunStatus.Failure;
				yield break;
			}

			root.Character.MoveTo(target.Position);
			float last = root.Time-.3f;

			while (BattlePerception.Distance(target, root.Character) > stopDistance)
			{
				if (!target)
				{
					root.Character.StopMove();
					yield return RunStatus.Failure;
					yield break;
				}


				if (last + .2f > root.Time)
				{
					yield return RunStatus.Running;
					continue;
				}

				root.Character.MoveTo(target.Position);
				yield return RunStatus.Running;
			}

			root.Character.StopMove();

			yield return RunStatus.Success;

		}

		public override void Stop(ITreeRoot context)
		{
			var root = context as AITreeRoot;

			if (LastStatus == RunStatus.Running) if (root.Character) root.Character?.StopMove();

			base.Stop(context);
		}
	}
}

