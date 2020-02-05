﻿using System;
using System.Collections.Generic;
using BehaviorTree;
using EConfig;
using ExcelConfig;
using Layout.AITree;
using Layout.EditorAttributes;

namespace GameLogic.Game.AIBehaviorTree
{
	[TreeNodeParse(typeof(TreeNodeSelectCanReleaseMagic))]
	public class ActionSelectCanReleaseMagic:ActionComposite<TreeNodeSelectCanReleaseMagic>
	{
        public ActionSelectCanReleaseMagic(TreeNodeSelectCanReleaseMagic node) : base(node) { }

		private readonly HashSet<int> releaseHistorys = new HashSet<int>();

        [Label("当前魔法")]
        public string key;

        public override IEnumerable<RunStatus> Execute(ITreeRoot context)
        {
            var root = context as AITreeRoot;
            key = string.Empty;

            var magics = root.Character.Magics;
            if (magics == null || magics.Count == 0)
            {
                yield return RunStatus.Failure;
                yield break;
            }

            var list = new List<CharacterMagicData>();
            foreach (var i in magics)
            {
                if (i.ReleaseType == (int)Proto.MagicReleaseType.MrtNormalAttack)
                {
                    if (root.Character.IsCoolDown(i.ID, root.Time, false))
                    {
                        list.Add(i);
                    }
                }
            }

            if (list.Count == 0)
            {
                yield return RunStatus.Failure;
                yield break;
            }

            int result = -1;
            switch (Node.resultType)
            {
                case MagicResultType.Random:
                    result = GRandomer.RandomList(list).ID;
                    break;
                case MagicResultType.Frist:
                    result = list[0].ID;
                    break;
                case MagicResultType.Sequence:
                    foreach (var i in list)
                    {
                        if (releaseHistorys.Contains(i.ID)) continue;
                        result = i.ID;
                    }
                    if (result == -1)
					{
						releaseHistorys.Clear();
						result = list[0].ID;
					}
					releaseHistorys.Add(result);
					break;
			}
			if (result == -1)
			{
				yield return RunStatus.Failure;
				yield break;
			}
			root[AITreeRoot.SELECT_MAGIC_ID] = result;
            var config = ExcelToJSONConfigManager.Current.GetConfigByID<CharacterMagicData>(result);
            if (config != null)
                key = config.MagicKey;
			yield return RunStatus.Success;
        }


	}
}

