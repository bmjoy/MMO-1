using System;
using GameLogic.Game.Elements;
using Proto;
using UnityEngine;
using P = Proto.HeroPropertyType;

namespace GameLogic.Game
{
    public struct DamageResult
    {
        public DamageResult(DamageType t, bool isMissed, int da,int crtm)
        {
            DType = t;
            IsMissed = isMissed;
            Damage = da;
            CrtMult = crtm;
        }
        public DamageType DType;
        public bool IsMissed;
        public int Damage;
        public int CrtMult;
    }
	/// <summary>
	/// 战斗中的算法
	/// </summary>
	public sealed class BattleAlgorithm
	{
        /// <summary>
        /// 力量增加血量
        /// </summary>
        public static float FORCE_HP = 5;
        /// <summary>
        /// 智力增加Mp
        /// </summary>
        public static float KNOWLEGDE_MP = 1.5f;
        /// <summary>
        /// 敏捷增加防御
        /// </summary>
        public static float AGILITY_DEFANCE = 1;
        /// <summary>
        /// 敏捷减少普攻间隔时间 ms
        /// </summary>
        public static float AGILITY_SUBWAITTIME = 8;//每点敏捷降低攻击间隔时间毫秒
        /// <summary>
        /// 敏捷增加移动速度
        /// </summary>
        public static float AGILITY_ADDSPEED = 0.02f;//0.02米
        /// <summary>
        /// 攻击最小间隔
        /// </summary>
        public static float ATTACK_MIN_WAIT = 300;//攻击最低间隔300毫秒
        /// <summary>
        /// 力量增加回血速度
        /// </summary>
        public static float FORCE_CURE_HP = 0.01f; //每点力量每秒增加血量
        /// <summary>
        /// 智力增加恢复MP
        /// </summary>
        public static float KNOWLEDGE_CURE_MP = 0.01f;//每点智力增加魔法
        /// <summary>
        /// 最快的移动速度
        /// </summary>
        public static float MAX_SPEED = 6.5f;//最大速度

        /// <summary>
        /// hurt r
        /// </summary>
        public static float HURT_NOTIFY_R = 10f;

        /// <summary>
        /// 计算普通攻击
        /// </summary>
        /// <param name="attack"></param>
        /// <returns></returns>
        public static int CalNormalDamage(BattleCharacter attack)
        {
            float damage = Randomer.RandomMinAndMax(attack[P.DamageMin].FinalValue,attack[P.DamageMax].FinalValue);
            
            switch (attack.Category)
            {
                case HeroCategory.HcAgility:
                    damage += attack[P.Agility].FinalValue;
                    break;
                case HeroCategory.HcForce:
                    damage += attack[P.Force].FinalValue;
                    break;
                case HeroCategory.HcKnowledge:
                    damage += attack[P.Knowledge].FinalValue;
                    break;
            }
            return (int)damage;
        }

		public static float[][] DamageRate = new float[][]
		{
			new float[]{0f,0f,0f},//混乱
			new float[]{0f,0.5f,0f},
			new float[]{.5f,-0.5f,0f}
		};

        //处理伤害类型加成
        public static int CalFinalDamage(int damage, DamageType dType, DefanceType dfType)
        {
            float rate = 1 + DamageRate[(int)dType][(int)dfType];
            float result = damage * rate;
            return (int)result;
        }


        public static DamageResult GetDamageResult(BattleCharacter sources, int damage,DamageType dType, BattleCharacter defencer)
        {
            bool isMissed = false;
            int crtmult = 1;
            var crt = sources[P.Crt].FinalValue;

            if (GRandomer.Probability10000(crt))
            {
                crtmult = 2;
            }
            switch (dType)
            {
                case DamageType.Physical:
                    {
                        var d = defencer[P.Defance].FinalValue + defencer[P.Agility].FinalValue*AGILITY_DEFANCE;
                        damage = damage * crtmult;
                        //乘法
                        var result = (damage *  damage) /(damage + d );
                        isMissed = GRandomer.Probability10000(defencer[P.Jouk].FinalValue);
                        damage = (int)result;
                    }
                    break;
                case DamageType.Magic:
                    {
                        damage =(int)( damage *(1 - defencer[P.Resistibility].FinalValue / 10000f));
                    }
                    break;
                default:
                    break;
            }

            return new DamageResult(dType, isMissed, damage, crtmult);
        }
	}
}

