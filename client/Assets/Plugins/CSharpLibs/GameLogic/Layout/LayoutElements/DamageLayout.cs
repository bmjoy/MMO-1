﻿using System;
using Layout.EditorAttributes;
//using System.Xml;

namespace Layout.LayoutElements
{
	public enum DamageType
	{
		Single=0,//单体
		Rangle 
	}

	public enum FilterType
	{
		ALL=0,
		OwnerTeam, //自己队友
		EmenyTeam, //敌人队伍
		Alliance   //联盟队伍 
	}

	public enum EffectType
	{
		EffectGroupAll=0,
		EffectGroup, //event group
		EffectConfig //config
	}

	public class DamageRange
	{
		[Label("伤害筛选类型")]
		public DamageType damageType= DamageType.Rangle;
		[Label("过滤方式")]
		public FilterType fiterType = FilterType.EmenyTeam;

		[Label("半径")]
		public float radius = 1;
		[Label("范围角度方向")]
		public float angle = 360;
		[Label("方向偏移角")]
		public float offsetAngle =0;
		[Label("偏移向量")]
		public Vector3 offsetPosition = Vector3.zero;

        public override string ToString()
        {
			if (damageType == DamageType.Single)
				return $"{damageType}";
			return $"{damageType} R:{radius} offsetAngle:{ offsetAngle} of Angle:{angle}";
        }
    }

	[EditorLayout("目标判定")]
	public class DamageLayout:LayoutBase
	{
		public DamageLayout()
		{
			target = TargetType.Releaser;
			effectType = EffectType.EffectGroup;
		}

		[Label("目标")]
		public TargetType target;

        [Label("范围")]
		public DamageRange RangeType = new DamageRange();

		[Label("效果取值来源")]
		public EffectType effectType;
		[Label("执行的效果组Key")]
		public string effectKey;

		public override string ToString ()
		{
			return string.Format ("目标{0} 范围{1} 效果 {2}",target , RangeType, effectKey);
		}
	}
}

