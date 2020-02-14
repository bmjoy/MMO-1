using System;
using Layout.LayoutElements;

namespace GameLogic.Game.Elements
{
    public interface IMagicReleaser : IBattleElement
    {
        void ShowDamageRanger(DamageLayout layout);
    }
}

