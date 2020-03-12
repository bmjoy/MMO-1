using System;
using EngineCore.Simulater;

namespace GameLogic.Game.Elements
{
    public interface IBattleItem : IBattleElement
    {

    }

    public class BattleItem:BattleElement<IBattleItem>
    {
        public BattleItem(GControllor controllor,IBattleItem view, Proto.PlayerItem item):base(controllor, view)
        {
            DropItem = item;
        }

        public Proto.PlayerItem DropItem { private set; get; }
    }
}
