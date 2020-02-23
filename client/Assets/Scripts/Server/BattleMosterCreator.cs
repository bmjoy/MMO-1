using System;
using System.Collections.Generic;
using System.Linq;
using EConfig;
using GameLogic.Game.Perceptions;
using Proto;
using UGameTools;
using UnityEngine;
using CM = ExcelConfig.ExcelToJSONConfigManager;
using Vector3 = UnityEngine.Vector3;
using P = Proto.HeroPropertyType;
using EngineCore.Simulater;

namespace Server
{
    public class BattleMosterCreator
    {
        public BattleSimulater Simulater { get; private set; }

        public BattlePerception Per { get { return Simulater.State.Perception as BattlePerception; } }

        public BattleLevelData LevelData { get { return Simulater.LevelData; } }

        private MonsterGroupPosition[] MonsterGroups { get { return Simulater.MonsterGroup; } }

        public int CountKillCount;

        public int AliveCount;

        private float LastTime = 0;

        public BattleMosterCreator(BattleSimulater sim)
        {
            Simulater = sim;
        }

        private void CreateMonster()
        {
           
            BattlePerception per = Per;
            //process Drop;
            var groupPos = this.MonsterGroups.Select(t => t.transform.position).ToArray();

            var pos = GRandomer.RandomArray(groupPos);

            IList<int> groups = null;

            if (CountKillCount < LevelData.BossNeedKilledNumber)
            {

                groups = LevelData.MonsterGroupID.SplitToInt();
            }
            else
            {
                CountKillCount = 0;
                groups = LevelData.BossGroupID.SplitToInt();
            }

            var monsterGroups = CM.Current.GetConfigs<MonsterGroupData>(t =>
            {
                return groups.Contains(t.ID);
            });

            var monsterGroup = GRandomer.RandomArray(monsterGroups);
            var drop = CM.Current.GetConfigByID<DropGroupData>(monsterGroup.DropID);
            int maxCount = GRandomer.RandomMinAndMax(monsterGroup.MonsterNumberMin, monsterGroup.MonsterNumberMax);
            var standPos = new List<BattleStandData>();
            switch ((StandType)monsterGroup.StandType)
            {
                case StandType.StAround:
                    {
                        var r = monsterGroup.StandParams[0];
                        var ang = 360 / maxCount;
                        for (var i = 0; i < maxCount; i++)
                        {
                            var offset = Quaternion.Euler(0, ang * i, 0) * Vector3.forward * r;
                            var forword = Quaternion.LookRotation(Quaternion.Euler(0, ang * i, 0) * Vector3.forward);
                            standPos.Add(new BattleStandData { Pos = pos + offset, Forward = new Vector3(0, forword.eulerAngles.y, 0) });
                        }
                    }
                    break;
                case StandType.StRandom:
                default:
                    {
                        var r = (int)monsterGroup.StandParams[0];
                        for (var i = 0; i < maxCount; i++)
                        {
                            var offset = new Vector3(GRandomer.RandomMinAndMax(-r, r), 0, GRandomer.RandomMinAndMax(-r, r));
                            standPos.Add(new BattleStandData
                            {
                                Pos = pos + offset,
                                Forward = new Vector3(0, GRandomer.RandomMinAndMax(0, 360), 0)
                            }); ;
                        }
                    }

                    break;
            }

            for (var i = 0; i < maxCount; i++)
            {
                var m = monsterGroup.MonsterID.SplitToInt();
                var p = monsterGroup.Pro.SplitToInt().ToArray();
                var monsterID = m[GRandomer.RandPro(p)];
                var monsterData = CM.Current.GetConfigByID<MonsterData>(monsterID);
                var data = CM.Current.GetConfigByID<CharacterData>(monsterData.CharacterID);
                var magic = CM.Current.GetConfigs<CharacterMagicData>(t => { return t.CharacterID == data.ID; });
                var mName = $"{data.Name}";

                if (!string.IsNullOrEmpty(monsterData.NamePrefix))
                {
                    mName = $"{monsterData.NamePrefix}.{data.Name}";
                }

                var Monster = per.CreateCharacter(monsterData.Level, data, magic.ToList(), 2,
                    standPos[i].Pos, standPos[i].Forward, string.Empty, mName);

                Monster[P.DamageMax].SetBaseValue(Monster[P.DamageMax].BaseValue + monsterData.DamageMax);
                Monster[P.DamageMin].SetBaseValue(Monster[P.DamageMin].BaseValue + monsterData.DamageMin);
                Monster[P.Force].SetBaseValue(Monster[P.Force].BaseValue + monsterData.Force);
                Monster[P.Agility].SetBaseValue(Monster[P.Agility].BaseValue + monsterData.Agility);
                Monster[P.Knowledge].SetBaseValue(Monster[P.Knowledge].BaseValue + monsterData.Knowledeg);
                Monster[P.MaxHp].SetBaseValue(Monster[P.MaxHp].BaseValue + monsterData.HPMax);

                Monster.Reset();
                per.ChangeCharacterAI(data.AIResourcePath, Monster);
                AliveCount++;
                Monster.OnDead = (el) =>
                {
                    CountKillCount++;
                    AliveCount--;
                    GObject.Destroy(el, 3f);
                };
            }
        }

        internal void TryCreateMonster(float time)
        {
            if (LastTime + LevelData.MaxRefrshTime < time)
            {
                if (AliveCount <= LevelData.MaxMonster)
                    CreateMonster();
                LastTime = time;
            }

            if (AliveCount == 0)
            {
                CreateMonster();
                LastTime = time;
            }
        }

        /*
        private void DoDrop()
        {
            if (drop == null) return;
            var items = drop.DropItem.SplitToInt();
            var pors = drop.Pro.SplitToInt();
            foreach (var i in BattlePlayers)
            {
                var notify = new Notify_Drop
                {
                    AccountUuid = i.Value.AccountId
                };
                var gold = GRandomer.RandomMinAndMax(drop.GoldMin, drop.GoldMax);
                notify.Gold = gold;
                i.Value.AddGold(gold);
                if (items.Count > 0)
                {
                    for (var index = 0; index < items.Count; index++)
                    {
                        if (GRandomer.Probability10000(pors[index]))
                        {
                            i.Value.AddDrop(items[index], 1);
                            notify.Items.Add(new PlayerItem { ItemID = items[index], Num = 1 });
                        }
                    }
                }
                var message = notify.ToNotityMessage();
                i.Value.Client.SendMessage(message);
            }
        }*/
    }
}
