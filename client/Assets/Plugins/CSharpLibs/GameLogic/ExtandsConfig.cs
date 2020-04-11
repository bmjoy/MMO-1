using System.Collections.Generic;
using EConfig;
using ExcelConfig;
using GameLogic.Game;
using GameLogic.Game.Elements;
using Proto;
using P = Proto.HeroPropertyType;

namespace GameLogic
{
    public static class ExtandsConfig
    {
        public static IList<int> SplitToInt(this string str, char sKey = '|')
        {
            var arrs = str.Split(sKey);
            var list = new List<int>();
            foreach (var i in arrs) list.Add(int.Parse(i));
            return list;
        }

        public static Dictionary<P, ComplexValue> GetProperties(this PlayerItem pItem)
        {
            var config = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(pItem.ItemID);
            var equip = ExcelToJSONConfigManager.Current.GetConfigByID<EquipmentData>(int.Parse(config.Params[0]));

            var level = ExcelToJSONConfigManager.Current
                          .FirstConfig<EquipmentLevelUpData>(t => t.Level == pItem.Level && t.Quality == config.Quality);
            var pro = equip.Properties.SplitToInt();
            var val = equip.PropertyValues.SplitToInt();

            var properties = new Dictionary<P, ComplexValue>();
            for (var ip = 0; ip < pro.Count; ip++)
            {
                var pr = (P)pro[ip];
                if (!properties.ContainsKey(pr)) properties.Add(pr, 0);
                if (properties.TryGetValue(pr, out ComplexValue value))
                {
                    value.SetBaseValue(value.BaseValue + val[ip]);
                }
            }
            if (pItem.Data != null)
            {
                foreach (var v in pItem.Data.Values)
                {
                    var k = (P)v.Key;
                    if (!properties.ContainsKey(k)) properties.Add(k, 0);
                    if (properties.TryGetValue(k, out ComplexValue value))
                    {
                        value.SetAppendValue(value.AppendValue + v.Value);
                    }
                }
            }
            foreach (var p in properties)
            {
                p.Value.SetRate(level?.AppendRate ?? 0);
            }
            return properties;
        }

        public static TargetTeamType GetTeamType(this CharacterMagicData att)
        {
            //var att = mc;
            var aiType = (MagicReleaseAITarget)att.AITargetType;
            TargetTeamType type = TargetTeamType.All;
            switch (aiType)
            {
                case MagicReleaseAITarget.MatEnemy:
                    type = TargetTeamType.Enemy;
                    break;
                case MagicReleaseAITarget.MatOwn:
                    type = TargetTeamType.Own;
                    break;
                case MagicReleaseAITarget.MatOwnTeam:
                    type = TargetTeamType.OwnTeam;
                    break;
                case MagicReleaseAITarget.MatOwnTeamWithOutSelf:
                    type = TargetTeamType.OwnTeamWithOutSelf;
                    break;
                case MagicReleaseAITarget.MatAll:
                    break;
                default:
                    type = TargetTeamType.All;
                    break;
            }
            return type;
        }

        public static IList<BattleCharacterMagic> CreateHeroMagic(this CharacterData data, DHero hero = null)
        {
            var cData = ExcelToJSONConfigManager.Current.FirstConfig<CharacterPlayerData>(t => t.CharacterID == data.ID);
            var magics = ExcelToJSONConfigManager.Current.GetConfigs<CharacterMagicData>(t =>
            {
                return t.CharacterID == data.ID && (MagicReleaseType)t.ReleaseType == MagicReleaseType.MrtMagic;
            });
            var list = new List<BattleCharacterMagic>();
            foreach (var i in magics)
            {
                var level = GetMagicLevel(hero, i.ID);
                if (level == null) continue;
                list.Add(new BattleCharacterMagic(MagicType.MtMagic, i, GetMagicLevel(hero, i.ID)));
            }
            if (cData != null)
            {
                if (cData.NormalAttack > 0)
                {
                    var config = ExcelToJSONConfigManager.Current.GetConfigByID<CharacterMagicData>(cData.NormalAttack);
                    list.Add(new BattleCharacterMagic(MagicType.MtNormal, config, GetMagicLevel(hero, cData.NormalAttack)));
                }

            }
            return list;
        }

        public static IList<BattleCharacterMagic> CreateHeroMagic(this DHero hero )
        {
            var data = ExcelToJSONConfigManager.Current.GetConfigByID<CharacterData>(hero.HeroID);
            return CreateHeroMagic(data, hero);
        }

        private static MagicLevelUpData GetMagicLevel(DHero hero, int magicID)
        {
            if (hero == null) return null;
            foreach (var i in hero.Magics)
            {
                if (i.MagicKey == magicID)
                {
                    return ExcelToJSONConfigManager
                        .Current
                        .FirstConfig<MagicLevelUpData>(t => t.MagicID == magicID && t.Level == i.Level);
                }
            }
            return null;
        }


    }
}
