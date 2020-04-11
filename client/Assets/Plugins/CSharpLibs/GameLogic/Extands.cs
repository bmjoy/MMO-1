using System.Collections.Generic;
using EConfig;
using ExcelConfig;
using GameLogic.Game;
using Proto;
using UVector3 = UnityEngine.Vector3;
using P = Proto.HeroPropertyType;
namespace GameLogic
{
    public static class Extands
    {
        public static Vector3 ToV3(this Proto.Vector3 v3)
        {
            return new Vector3 { X = v3.X, Y = v3.Y, Z = v3.Z };
        }

        public static UVector3 ToUV3(this Proto.Vector3 v3)
        {
            return new UVector3(v3.X, v3.Y, v3.Z);
        }

        public static Vector3 ToV3(this Layout.Vector3 v3)
        {
            return new Vector3 { X = v3.x, Y = v3.y, Z = v3.z };
        }

        public static UVector3 ToUV3(this Layout.Vector3 v3)
        {
            return new UVector3(v3.x, v3.y, v3.z);
        }
        public static Proto.Vector3 ToPV3(this UVector3 v3)
        {
            return new Vector3 { X = v3.x, Y = v3.y, Z = v3.z };
        }

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

        public static TargetTeamType GetTeamType(this EConfig.CharacterMagicData att)
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
    }

}

