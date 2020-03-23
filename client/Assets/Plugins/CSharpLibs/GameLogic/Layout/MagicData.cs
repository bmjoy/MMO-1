using System;
using Layout.EditorAttributes;
using System.Collections.Generic;

namespace Layout
{
	public class MagicData
	{
		public MagicData ()
		{
			Containers = new List<EventContainer> ();
			triggerDurationTime = -1;
			triggerTicksTime = -1;
		}

		[Label("名称")]
		public string name;

		//[Label("事件")]
		public List<EventContainer> Containers;

		[Label("触发间隔时间")]
		public float triggerTicksTime;

		[Label("触发持续时间")]
		public float triggerDurationTime;

		[Label("唯一(唯一不允许多个释放实例)")]
		public bool unique = false;
	}
}

