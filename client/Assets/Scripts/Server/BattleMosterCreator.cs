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
using GameLogic.Game.Elements;
using XNet.Libs.Utility;

namespace Server
{
    public struct BattleStandData
    {
        public Vector3 Pos;
        public Vector3 Forward;
    }


    public class BattleMosterCreator
    {
        public BattleLevelSimulater Simulater { get; private set; }

        public BattlePerception Per { get { return Simulater.State.Perception as BattlePerception; } }

        public BattleLevelData LevelData { get { return Simulater.LevelData; } }

        private MonsterGroupPosition[] MonsterGroups { get { return Simulater.MonsterGroup; } }

        public int CountKillCount;

        public int AliveCount;

        private float LastTime = 0;

        public BattleMosterCreator(BattleLevelSimulater sim)
        {
            Simulater = sim;
        }

        private void CreateMonster()
        {
            BattlePerception per = Per;
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
                var id = m[GRandomer.RandPro(p)];
                var monsterData = CM.Current.GetConfigByID<MonsterData>(id);
                var data = CM.Current.GetConfigByID<CharacterData>(monsterData.CharacterID);
                var magic = per.CreateHeroMagic(data.ID);
                var mName = $"{data.Name}";

                if (!string.IsNullOrEmpty(monsterData.NamePrefix))
                {
                    mName = $"{monsterData.NamePrefix}.{data.Name}";
                }


                var append = new Dictionary<P, int>
                {
                    { P.DamageMax, monsterData.DamageMax },
                    { P.DamageMin, monsterData.DamageMin },
                    { P.Force, monsterData.Force },
                    { P.Agility, monsterData.Agility },
                    { P.Knowledge, monsterData.Knowledeg },
                    { P.MaxHp, monsterData.HPMax }
                };


                var Monster = per.CreateCharacter(monsterData.Level, data, magic, append, 2,
                    standPos[i].Pos, standPos[i].Forward, string.Empty, mName);
                per.ChangeCharacterAI(data.AIResourcePath, Monster);
                AliveCount++;

                Monster["__Drop"] = drop;
                Monster["__Monster"] = monsterData;

                Monster.OnDead = (el) =>
                {
                    GObject.Destroy(el, 3f);
                    CountKillCount++;
                    AliveCount--;

                    if (el["__Drop"] is DropGroupData d
                    && el["__Monster"] is MonsterData mdata)
                    {
                        var os = el.Watch.Values.OrderBy(t => t.FristTime).ToList();
                        foreach (var i in os)
                        {
                            var owner = per.FindTarget(i.Index);
                            if (!owner) continue;
                            //召唤物掉落归属问题
                            if (owner.OwnerIndex > 0) owner = per.FindTarget(owner.OwnerIndex);
                            DoDrop(el.Position, mdata, d, owner?.Index ?? -1, owner?.TeamIndex ?? -1, owner);
                            break;
                        }
                    }
                };
            }
        }

        internal void TryCreateMonster(float time)
        {
            if (AliveCount == 0)
            {
                CreateMonster();
                LastTime = time;
            }
            else if (LastTime + LevelData.MaxRefrshTime < time)
            {
                if (AliveCount <= LevelData.MaxMonster) CreateMonster();
                LastTime = time;
            }
        }

        private void DoDrop(Vector3 pos, MonsterData monster, DropGroupData drop, int groupIndex, int teamIndex, BattleCharacter owner)
        {
            BattlePlayer player = null;
            if (owner && BattleSimulater.S.TryGetPlayer(owner.AcccountUuid, out  player))
            {
                var exp = player.GetHero().Exprices;
                int expNew = player.AddExp(monster.Exp, out int old, out int newLevel);
                if (newLevel != old) player.HeroCharacter.SetLevel(newLevel);
                var expNotify = new Notify_CharacterExp { Exp = expNew, Level = newLevel, OldExp = exp, OldLeve = old };
                player.Client.SendMessage(expNotify.ToNotityMessage());
            }

            if (drop == null) return;
            if (!GRandomer.Probability10000(drop.DropPro)) return;
            var items = drop.DropItem.SplitToInt();
            var pors = drop.Pro.SplitToInt();
            if (owner)
            {
                var gold = GRandomer.RandomMinAndMax(drop.GoldMin, drop.GoldMax);
                if (gold > 0)
                {
                    if (player != null)
                    {
                        player.AddGold(gold);
                        var notify = new Notify_DropGold { Gold = gold, TotalGold = player.Gold };
                        player.Client.SendMessage(notify.ToNotityMessage());
                    }
                }
            }

            var count = GRandomer.RandomMinAndMax(drop.DropMinNum, drop.DropMaxNum);
            while (count > 0)
            {
                count--;
                var offset = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f));
                var index = GRandomer.RandPro(pors.ToArray());
                var item = new PlayerItem { ItemID = items[index], Num = 1 };
                Per.CreateItem(pos + offset, item, groupIndex, teamIndex);
            }
        }
    }
}
