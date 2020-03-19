using System;
using GameLogic.Utility;
using Layout.LayoutElements;
using Proto;

namespace GameLogic.Game.Elements
{
    public interface IMagicReleaser : IBattleElement
    {
        void ShowDamageRanger(DamageLayout layout);

        [NeedNotify(typeof(Notify_PlayTimeLine),"Path")]
        void PlayTimeLine(string layoutPath);
    }
}

