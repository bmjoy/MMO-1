﻿using System.Collections.Generic;
using BehaviorTree;
using EConfig;
using ExcelConfig;
using GameLogic.Game.Elements;
using GameLogic.Game.LayoutLogics;
using Layout.AITree;

namespace GameLogic.Game.AIBehaviorTree
{
    [TreeNodeParse(typeof(TreeNodeReleaseMagic))]
	public class ActionReleaseMagic : ActionComposite<TreeNodeReleaseMagic>
	{

        public ActionReleaseMagic(TreeNodeReleaseMagic node) : base(node) { }

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
            var root = context as AITreeRoot;

            if (!root.TryGetTarget(out BattleCharacter target))
            {
                yield return RunStatus.Failure;
                yield break;
            }

            string key = Node.magicKey;
            switch (Node.valueOf)
            {
                case MagicValueOf.BlackBoard:
                    {

                        if (!root.TryGetMagic(out CharacterMagicData magicData))

                        {
                            yield return RunStatus.Failure;
                            yield break;
                        }
                        key = magicData.MagicKey;
                        root.Character.AttachMagicHistory(magicData.ID, root.Time);
                    }
                    break;
                case MagicValueOf.MagicKey:
					{
						key = Node.magicKey;
					}
					break;
			}

			if (!root.Perception.View.ExistMagicKey(key))
			{
                if (context.IsDebug)
                {
                    Attach("failure", $"nofound key {key}");
                }
				yield return RunStatus.Failure;
				yield break;
			}


            root.Character.StopMove();
            releaser = root.Perception
                .CreateReleaser(key, root.Character, new ReleaseAtTarget(root.Character, target), ReleaserType.Magic ,-1);

            while (!releaser.IsLayoutStartFinish)
            {
                yield return RunStatus.Running;
            }

            yield return RunStatus.Success;
            yield break;
           
        }

        private MagicReleaser releaser;

        public override void Stop(ITreeRoot context)
        {
            if (LastStatus == RunStatus.Running)
            {
                releaser?.Cancel();
            }
            base.Stop(context);
        }
	}
}

