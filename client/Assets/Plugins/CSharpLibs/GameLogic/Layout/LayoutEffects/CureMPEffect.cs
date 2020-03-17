using Layout.EditorAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Layout.LayoutEffects
{


    [EditorEffect("恢复魔法")]
    public class CureMPEffect : EffectBase
    {
        public CureMPEffect()
        {
            valueType = ValueOf.NormalAttack;
        }

        [Label("取值来源")]
        public ValueOf valueType;

        [Label("值")]
        public int value;
    }
}
