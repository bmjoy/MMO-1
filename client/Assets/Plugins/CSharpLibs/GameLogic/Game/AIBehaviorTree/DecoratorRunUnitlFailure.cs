﻿using BehaviorTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogic.Game.AIBehaviorTree
{
    public class DecoratorRunUnitlFailure : Decorator
    {
        public DecoratorRunUnitlFailure(Composite child) : base(child) { }

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {

            if (DecoratedChild == null)
            {
                yield return RunStatus.Failure;
                yield break;
            }

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

                if (DecoratedChild.LastStatus.HasValue && DecoratedChild.LastStatus.Value == RunStatus.Failure)
                {
                    yield return RunStatus.Failure;
                    yield break;
                }
                DecoratedChild.Start(context);
                yield return RunStatus.Running;
            }
        }
    }
}
