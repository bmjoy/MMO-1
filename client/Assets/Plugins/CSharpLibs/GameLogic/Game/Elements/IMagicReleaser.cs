using System;
using GameLogic.Utility;
using Layout.LayoutElements;
using Proto;

namespace GameLogic.Game.Elements
{
    public interface IMagicReleaser : IBattleElement
    {
        void ShowDamageRanger(DamageLayout layout, UnityEngine.Vector3 tar, UnityEngine.Quaternion rototion);

        [NeedNotify(typeof(Notify_PlayTimeLine),"PlayIndex", "Path", "TargetIndex", "Type")]
        void PlayTimeLine(int pIndex,string layoutPath, int target, int type);
        [NeedNotify(typeof(Notify_CancelTimeLine), "PlayIndex")]
        void CancelTimeLine(int pIndex);

        void PlayTest(int pIndex,TimeLine line);
    }
}

