using System;
using GameLogic.Utility;
using Layout.LayoutElements;
using Proto;

namespace GameLogic.Game.Elements
{
    public interface IMagicReleaser : IBattleElement
    {
        void ShowDamageRanger(DamageLayout layout);

        [NeedNotify(typeof(Notify_ReleaserPlaySound), "TargetType", "ResourcesPath", "BoneName", "Value")]
        void PlaySound(int target, string resourcesPath, string fromBone, float value);
    }
}

