using System;
using Layout.EditorAttributes;

namespace Layout.LayoutEffects
{
	public enum ValueOf
	{
		NormalAttack = 0,//普攻
		FixedValue,// 固定值
		MPMaxPro,//魔法最大值万分比
		HPMaxPro,//生命最大值万分比
		HPPro,//生命万分比
		MPPro,//魔法万分比
	}

	[EditorEffect("攻击伤害")]
	public class NormalDamageEffect:EffectBase
	{
		[Label("取值来源")]
		public ValueOf valueOf;

		[Label("固定伤害值")]
		public int DamageValue;

		public override string ToString()
		{
			return $"取值方式:{valueOf} -参数 {DamageValue}";
		}
	}
}

