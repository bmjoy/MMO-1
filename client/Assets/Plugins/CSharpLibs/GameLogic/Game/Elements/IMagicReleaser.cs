using System;
using GameLogic.Utility;
using Layout.LayoutElements;
using Proto;

namespace GameLogic.Game.Elements
{
    public interface IMagicReleaser : IBattleElement
    {
        void ShowDamageRanger(DamageLayout layout);

        [NeedNotify(typeof(Notify_PlayTimeLine), "Path", "TargetIndex", "Type")]
        void PlayTimeLine(string layoutPath, int target, int type);

        void PlayTest(TimeLine line);
    }
}

