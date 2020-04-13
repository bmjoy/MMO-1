using System;
using GameLogic.Utility;
using Layout.LayoutElements;
using Proto;

namespace GameLogic.Game.Elements
{
    public interface IMagicReleaser : IBattleElement
    {
        //for editor test 
        void ShowDamageRanger(DamageLayout layout, UnityEngine.Vector3 tar, UnityEngine.Quaternion rototion);
        void PlayTest(int pIndex, TimeLine line);
        //end

        [NeedNotify(typeof(Notify_PlayTimeLine),"PlayIndex", "PathIndex", "TargetIndex", "Type")]
        void PlayTimeLine(int pIndex,int pathIndex, int target, int type);
        [NeedNotify(typeof(Notify_CancelTimeLine), "PlayIndex")]
        void CancelTimeLine(int pIndex);
    }
}

