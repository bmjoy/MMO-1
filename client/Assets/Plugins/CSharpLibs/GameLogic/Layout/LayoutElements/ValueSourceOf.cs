using System;
using Proto;

namespace Layout.LayoutElements
{
    [Obsolete]
    public class ValueSourceOf
    {
        public GetValueFrom ValueForm = GetValueFrom.CurrentConfig;

        public int Value = 0;

        public override string ToString()
        {
            return $"{ValueForm} {Value}";
        }
    }
}
