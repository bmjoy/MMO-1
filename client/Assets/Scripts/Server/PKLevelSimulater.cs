using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    [LevelSimulater(MType = Proto.MapType.Pk)]
    internal class PKLevelSimulater : BattleLevelSimulater
    {
        protected PKLevelSimulater(EConfig.BattleLevelData data) : base(data)
        {
            
        }

        private int TeamIndex = 0;

        protected override int PlayerTeamIndex { get { return TeamIndex++; } }
    }
}
