using System;
using Layout.EditorAttributes;

namespace Layout.LayoutEffects
{
	public enum DurationTimeValueOf
	{
		MagicConfig =0 ,
		DurationTime
	}

	[EditorEffect("添加持续效果(释放技能buf)")]
	public class AddBufEffect:EffectBase
	{
		[Label("配置KEY")]
		public string buffMagicKey;

		[Label("持续时间")]
		public float durationTime;

		public override string ToString()
		{
			return string.Format("效果 {0} 持续 {1}s", buffMagicKey, durationTime);
		}
	}
}

