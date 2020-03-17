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
                while (DecoratedChild.Tick(context) == RunStatus.Running)
                {
                    yield return RunStatus.Running;
                }
                if (DecoratedChild.LastStatus == RunStatus.Failure)
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
