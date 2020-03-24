using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    [LevelSimulater(MType = Proto.MapType.Boss)]
    public class BossLevelSimulater:BattleLevelSimulater
    {
        private BattleMosterCreator MonsterCreator { set; get; }

        protected BossLevelSimulater(EConfig.BattleLevelData data) : base(data)
        {
            MonsterCreator = new BattleMosterCreator(this);
        }
        protected override void OnTick()
        {
            base.OnTick();
            MonsterCreator?.TryCreateMonster(this.TimeNow.Time);
        }
    }
}
