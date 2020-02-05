﻿using System;
using Layout.EditorAttributes;
using System.Xml.Serialization;

namespace Layout.LayoutEffects
{
	[
		XmlInclude(typeof(NormalDamageEffect)),
		XmlInclude(typeof(AddBufEffect)),
        XmlInclude(typeof(CureEffect)),
        XmlInclude(typeof(AddPropertyEffect)),
        XmlInclude(typeof(BreakReleaserEffect)),
        XmlInclude(typeof(ModifyLockEffect))
	]
	public class EffectBase
	{
		public EffectBase ()
		{
			
		}
			
		public static T CreateInstance<T>() where T: EffectBase,new()
		{
			return new T ();
		}

		public static EffectBase CreateInstance(Type t)
		{
			var inst = Activator.CreateInstance (t) as EffectBase;
			return inst;
		}
	}
}

