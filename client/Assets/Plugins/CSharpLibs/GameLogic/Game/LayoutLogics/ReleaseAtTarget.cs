using GameLogic.Game.Elements;
using UVector3 = UnityEngine.Vector3;

namespace GameLogic.Game.LayoutLogics
{
	public class ReleaseAtTarget : IReleaserTarget
	{
		public ReleaseAtTarget(BattleCharacter releaser, BattleCharacter target)
		{
			Releaser = releaser;
			ReleaserTarget = target;
		}

		public BattleCharacter Releaser { get; private set; }

		public BattleCharacter ReleaserTarget { get; private set; }

		public UVector3 TargetPosition
		{
			get { return ReleaserTarget.Position; }
		}

	}
}

