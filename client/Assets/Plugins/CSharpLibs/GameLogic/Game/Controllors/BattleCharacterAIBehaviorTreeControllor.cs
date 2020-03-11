using System;
using EngineCore.Simulater;
using GameLogic.Game.Elements;

namespace GameLogic.Game.Controllors
{
	public class BattleCharacterAIBehaviorTreeControllor:GControllor
	{
		public BattleCharacterAIBehaviorTreeControllor(GPerception per) : base(per) { }

		public override GAction GetAction(GTime time, GObject current)
		{
			var character = current as BattleCharacter;
			if (!character.IsDeath) character?.TickAi();
			return GAction.Empty;
		}
	}
}

