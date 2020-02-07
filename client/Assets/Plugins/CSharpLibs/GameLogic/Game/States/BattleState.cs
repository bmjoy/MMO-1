﻿using System;
using GameLogic.Game.Perceptions;
using EngineCore.Simulater;
using GameLogic.Game.Elements;
using P = Proto.HeroPropertyType;
namespace GameLogic.Game.States
{
	
	public class BattleState:GState
	{
        public BattleState (IViewBase viewBase,IStateLoader loader, ITimeSimulater simulater)
		{
			ViewBase = viewBase;
			Perception = new BattlePerception (this, viewBase.Create (simulater));
			loader.Load (this);
		}

		public IViewBase ViewBase{ private set; get; }


		protected override void OnInit ()
		{
			base.OnInit ();

		}

        protected override void Tick(GTime time)
        {
            base.Tick(time);

        }

        float lastHpCure = 0;

        private void CureHPAndMp(float time)
        {
            //处理生命,魔法恢复
            if (lastHpCure + 3 < time)
            {
                lastHpCure = time;
                Each<BattleCharacter>((el) =>
                {
                    var hp = (int)(el[P.Force].FinalValue * BattleAlgorithm.FORCE_CURE_HP * 3);
                    if (hp > 0) el.AddHP(hp);
                    var mp = (int)(el[P.Knowledge].FinalValue * BattleAlgorithm.KNOWLEDGE_CURE_MP * 3);
                    if (mp > 0) el.AddMP(mp);
                    return false;
                });
            }
        }
    }
}

