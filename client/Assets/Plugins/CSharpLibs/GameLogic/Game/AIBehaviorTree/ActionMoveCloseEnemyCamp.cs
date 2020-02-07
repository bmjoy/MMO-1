using System;
using System.Collections.Generic;
using BehaviorTree;
using GameLogic.Game.Perceptions;
using Layout.AITree;
using UVector3 = UnityEngine.Vector3;

namespace GameLogic.Game.AIBehaviorTree
{
    [TreeNodeParse(typeof(TreeNodeMoveCloseEnemyCamp))]
    [Obsolete]
	public class ActionMoveCloseEnemyCamp:ActionComposite<TreeNodeMoveCloseEnemyCamp>
	{
		public ActionMoveCloseEnemyCamp(TreeNodeMoveCloseEnemyCamp n):base(n)
		{
		}

		public override IEnumerable<RunStatus> Execute(ITreeRoot context)
		{
			yield return RunStatus.Success;
			yield break;
		}
	}
}

