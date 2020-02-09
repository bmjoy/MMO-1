using System.Collections.Generic;
using BehaviorTree;
using EConfig;
using ExcelConfig;
using GameLogic.Game.Elements;
using GameLogic.Game.Perceptions;
using Layout.AITree;
using Layout.EditorAttributes;
using Proto;
using UVector3 = UnityEngine.Vector3;

namespace GameLogic.Game.AIBehaviorTree
{
    [TreeNodeParse(typeof(TreeNodeFindTarget))]
	public class ActionFindTarget:ActionComposite<TreeNodeFindTarget>
    {
		public ActionFindTarget(TreeNodeFindTarget n) : base(n) { }

		public override IEnumerable<RunStatus> Execute(ITreeRoot context)
		{
			var root = context as AITreeRoot;
			var character =  root.Character;
			var per = root.Perception;
			var list = new List<BattleCharacter>();
			var distance = Node.Distance/100f;
			float getDistanceValue = 0f;
			int view = Node.View;
			if (!root.GetDistanceByValueType(Node.valueOf, distance, out distance))
			{
                getDistanceValue = -1;
				yield return RunStatus.Failure;
                yield break;
			}
            getDistanceValue = distance;

			//是否保留之前目标
			if (!Node.findNew)
			{
				root.TryGetTarget(out BattleCharacter targetCharacter);
				if (targetCharacter)
				{
					if (BattlePerception.InviewSide(targetCharacter, root.Character, distance,view))
					{
						yield return RunStatus.Success;
						yield break;
					}
				}
			}
            //清除
            root[AITreeRoot.TRAGET_INDEX] = null;
            var type = Node.teamType;
            //处理使用魔法目标
            if (Node.useMagicConfig)
            {
				if (!root.TryGetMagic(out CharacterMagicData data))
				{ 
                    yield return RunStatus.Failure;
                    yield break;
                }
                type = (TargetTeamType)data.AITargetType;
            }

			per.State.Each<BattleCharacter>(t => 
            {
                //隐身的不进入目标查找
                if (t.Lock.IsLock(ActionLockType.Inhiden))   return false;
                switch (type)
				{
                    case TargetTeamType.Enemy:
						if (character.TeamIndex == t.TeamIndex)
							return false;
						break;
					case TargetTeamType.OwnTeam:
						if (character.TeamIndex != t.TeamIndex)
							return false;
						break;
                    case TargetTeamType.OwnTeamWithOutSelf:
                        if (character.Index == t.Index) return false;
                        if (character.TeamIndex != t.TeamIndex)
                            return false;
                        break;
                    case TargetTeamType.Own:
                        {
                            if (character.Index != t.Index) return false;
                        }
                        break;
                    case TargetTeamType.All: 
                        break;
                    default:
                        return false;
				}

				if (!BattlePerception.InviewSide(t, root.Character, distance,view) )return false;
				switch (Node.filter)
				{
					case TargetFilterType.Hurt:
                        if (t.HP == t.MaxHP) return false;
						break;
				}
				list.Add(t);
				return false;
			});

            //getTargets = list.Count;
			if (list.Count == 0)
			{
				if (context.IsDebug) Attach("failure", $"{list.Count} nofound targets");
				yield return RunStatus.Failure;
				yield break;
			}


			BattleCharacter target =null;

			switch (Node.selectType)
			{
				case TargetSelectType.Nearest:
					{
						target = list[0];
						var d = BattlePerception.Distance(target, character);
						foreach (var i in list)
						{
							var temp = BattlePerception.Distance(i, character);
							if (temp < d)
							{
								d = temp;
								target = i;
							}
						}
					}
					break;
				case TargetSelectType.Random:
					target = GRandomer.RandomList(list);
					break;
				case TargetSelectType.HPMax:
					{
						target = list[0];
						var d = target.HP;
						foreach (var i in list)
						{
							var temp = i.HP;
							if (temp > d)
							{
								d = temp;
								target = i;
							}
						}
					}
					break;
				case TargetSelectType.HPMin:
					{
						target = list[0];
						var d = target.HP;
						foreach (var i in list)
						{
							var temp = i.HP;
							if (temp < d)
							{
								d = temp;
								target = i;
							}
						}
					}
					break;
				case TargetSelectType.HPRateMax:
					{
						target = list[0];
                        var d = (float)target.HP/ target.MaxHP;
						foreach (var i in list)
						{
                            var temp = (float)i.HP / i.MaxHP; ;
							if (temp > d)
							{
								d = temp;
								target = i;
							}
						}
					}
				break;
				case TargetSelectType.HPRateMin:
					{
						target = list[0];
                        var d = (float)target.HP / target.MaxHP;
						foreach (var i in list)
						{
                            var temp = (float)i.HP / i.MaxHP; 
							if (temp < d)
							{
								d = temp;
								target = i;
							}
						}
					}
					break;
			}

			if (context.IsDebug) Attach("Tagert", target);
			root[AITreeRoot.TRAGET_INDEX] = target.Index;
			yield return RunStatus.Success;
		}

	}
}

