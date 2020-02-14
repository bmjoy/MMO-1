using System;
using System.Collections.Generic;
using BehaviorTree;
using Layout.AITree;

namespace GameLogic.Game.AIBehaviorTree
{
    public class EventBattle:Decorator
    {
        public EventBattle(Composite child):base(child)
        {

        }

        private bool IsReceived = false;

        public BattleEventType eventType;

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
            while (true)
            {
                while (!IsReceived) yield return RunStatus.Running;
                IsReceived = false;
                DecoratedChild.Start(context);
                while (DecoratedChild.Tick(context) == RunStatus.Running)
                {
                    yield return RunStatus.Running;
                }
                DecoratedChild.Stop(context);
            }
        }


        public override void Start(ITreeRoot context)
        {
            base.Start(context);
            if (context is AITreeRoot r)
            {
                r.Character.OnBattleEvent += Character_OnBattleEvent;
            }
        }

        private void Character_OnBattleEvent(BattleEventType arg1, object arg2)
        {
            if (arg1 == eventType) IsReceived = true;
        }

        public override void Stop(ITreeRoot context)
        {
            base.Stop(context);
            if (context is AITreeRoot r)
            {
                r.Character.OnBattleEvent -= Character_OnBattleEvent;
            }
        }
    }
}
