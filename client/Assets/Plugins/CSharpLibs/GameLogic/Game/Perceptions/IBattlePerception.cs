using System;
using GameLogic.Game.Elements;
using EngineCore;
using Layout.LayoutElements;
using Layout;
using GameLogic.Game.LayoutLogics;
using EngineCore.Simulater;
using Layout.AITree;
using UVector3 = UnityEngine.Vector3;
using GameLogic.Utility;
using Proto;
using System.Collections.Generic;
using GameLogic.Game.AIBehaviorTree;

namespace GameLogic.Game.Perceptions
{
    /// <summary>
    /// I battle perception.
    /// </summary>
    public interface IBattlePerception:ITreeLoader
    {
        /// <summary>
        /// 当前的时间仿真
        /// </summary>
        /// <returns>The time simulater.</returns>
        ITimeSimulater GetTimeSimulater();

        /// <summary>
        /// Gets the AIT ree.
        /// </summary>
        /// <returns>The AIT ree.</returns>
        /// <param name="pathTree">Path tree.</param>
        TreeNode GetAITree(string pathTree);

        /// <summary>
        /// 获取当前的layout
        /// </summary>
        /// <returns>The time line by path.</returns>
        /// <param name="path">Path.</param>
        TimeLine GetTimeLineByPath(string path);

        /// <summary>
        /// Gets the magic by key.
        /// </summary>
        /// <returns>The magic by key.</returns>
        /// <param name="key">Key.</param>
        MagicData GetMagicByKey(string key);

        /// <summary>
        /// Exists the magic key.
        /// </summary>
        /// <returns>The magic key.</returns>
        /// <param name="key">Key.</param>
        bool ExistMagicKey(string key);

 
        [NeedNotify(typeof(Notify_CreateBattleCharacter),
            "AccountUuid", "ConfigID", "TeamIndex",
            "Position", "Forward", "Level", "Name", "Speed","Hp","MaxHp", "Mp", "MpMax", "Cds")]
        IBattleCharacter CreateBattleCharacterView
            (string account_id,int config, int teamId,
            Proto.Vector3 pos, Proto.Vector3 forward,int level,string name, float speed, int hp, int hpMax, int mp, int mpMax, IList<HeroMagicData> cds);

       
        [NeedNotify(typeof(Notify_CreateReleaser), "ReleaserIndex", "TargetIndex", "MagicKey", "Position")]
        IMagicReleaser CreateReleaserView(int releaser, int target, string magicKey, Proto.Vector3 targetPos);

        /// <summary>
        /// Creates the missile.
        /// </summary>
        /// <param name="releaseIndex"></param>
        /// <param name="res"></param>
        /// <param name="offset"></param>
        /// <param name="fromBone"></param>
        /// <param name="toBone"></param>
        /// <param name="speed"></param>
        /// <returns></returns>
        [NeedNotify(typeof(Notify_CreateMissile), "ReleaserIndex",
            "ResourcesPath","Offset", "FromBone", "ToBone", "Speed")]
        IBattleMissile CreateMissile(int releaseIndex,
            string res,  Proto.Vector3 offset, string fromBone, string toBone, float speed);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="itemID"></param>
        /// <param name="num"></param>
        /// <param name="teamIndex"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [NeedNotify(typeof(Notify_Drop),"Pos","Item","TeamIndex","GroupIndex")]
        IBattleItem CreateDropItem(Proto.Vector3 pos, PlayerItem item, int teamIndex, int groupId);


    
        /// <summary>
        /// Process damage
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="target"></param>
        /// <param name="damage"></param>
        /// <param name="isMissed"></param>
        /// <param name="crtMult"></param>
        /// <returns></returns>
        [NeedNotify(typeof(Notify_DamageResult), "Index", "TargetIndex", "Damage", "IsMissed", "CrtMult")]
        bool ProcessDamage(int owner, int target, int damage, bool isMissed,int crtMult);

       
    }
}

