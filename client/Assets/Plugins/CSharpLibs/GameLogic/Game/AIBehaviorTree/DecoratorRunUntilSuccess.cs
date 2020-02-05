﻿using BehaviorTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogic.Game.AIBehaviorTree
{
    public class DecoratorRunUntilSuccess :Decorator
    {
        public DecoratorRunUntilSuccess(Composite child) : base(child) { }

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
            DecoratedChild.Start(context);
            while (true)
            {
                DecoratedChild.Tick(context);
                if (DecoratedChild.LastStatus.HasValue && DecoratedChild.LastStatus.Value == RunStatus.Running)
                {
                    yield return RunStatus.Running;
                    continue;
                }
                DecoratedChild.Stop(context);
                if (DecoratedChild.LastStatus.HasValue && DecoratedChild.LastStatus.Value == RunStatus.Success)
                {
                    yield return RunStatus.Success;
                    yield break;
                }
                DecoratedChild.Start(context);
                yield return RunStatus.Running;
            }
        }
    }
}
