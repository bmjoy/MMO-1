using System;
using Layout.EditorAttributes;

namespace Layout.LayoutElements
{
	[EditorLayout("看向目标")]
	public class LookAtTarget:LayoutBase
	{
		public LookAtTarget()
		{
			
		}

		public override string ToString()
		{
			return $"看向目标";
		}
	}
}

