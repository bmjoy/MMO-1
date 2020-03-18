using System;
using System.Collections.Generic;

namespace Proto
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple =true)]
    public class IndexAttribute:Attribute
    {
         public IndexAttribute(int index,Type tOm) 
         {
            this.Index = index;
            this.TypeOfMessage = tOm;
         }
        public int Index { set; get; }
        public Type TypeOfMessage { set; get; }
    }

    [AttributeUsage(AttributeTargets.Class,AllowMultiple =false)]
    public class ApiVersionAttribute : Attribute
    {
        public ApiVersionAttribute(int m, int dev, int bate)
        {
            if (m > 99 || dev > 99 || bate > 99) throw new Exception("must less then 100");
            v = m* 10000 + dev* 100 + bate;
        }
        private readonly int v = 0;
        public int Version { get { return v; } }
    }


    [Index(1000001,typeof(Action_ClickSkillIndex))]
    [Index(1000002,typeof(Void))]
    [Index(1000003,typeof(Action_AutoFindTarget))]
    [Index(1000005,typeof(Action_MoveDir))]
    [Index(1000007,typeof(Action_NormalAttack))]
    [Index(1000009,typeof(Action_CollectItem))]
    [Index(1000011,typeof(Action_UseItem))]
    [Index(1001001,typeof(C2B_ExitBattle))]
    [Index(1001002,typeof(B2C_ExitBattle))]
    [Index(1001003,typeof(C2B_JoinBattle))]
    [Index(1001004,typeof(B2C_JoinBattle))]
    [Index(1002001,typeof(B2G_GetPlayerInfo))]
    [Index(1002002,typeof(G2B_GetPlayerInfo))]
    [Index(1002003,typeof(B2G_BattleReward))]
    [Index(1002004,typeof(G2B_BattleReward))]
    [Index(1003001,typeof(C2G_Login))]
    [Index(1003002,typeof(G2C_Login))]
    [Index(1003003,typeof(C2G_CreateHero))]
    [Index(1003004,typeof(G2C_CreateHero))]
    [Index(1003005,typeof(C2G_BeginGame))]
    [Index(1003006,typeof(G2C_BeginGame))]
    [Index(1003007,typeof(C2G_GetLastBattle))]
    [Index(1003008,typeof(G2C_GetLastBattle))]
    [Index(1003009,typeof(C2G_OperatorEquip))]
    [Index(1003010,typeof(G2C_OperatorEquip))]
    [Index(1003011,typeof(C2G_SaleItem))]
    [Index(1003012,typeof(G2C_SaleItem))]
    [Index(1003013,typeof(C2G_EquipmentLevelUp))]
    [Index(1003014,typeof(G2C_EquipmentLevelUp))]
    [Index(1003015,typeof(C2G_GMTool))]
    [Index(1003016,typeof(G2C_GMTool))]
    [Index(1003017,typeof(C2G_BuyPackageSize))]
    [Index(1003018,typeof(G2C_BuyPackageSize))]
    [Index(1003019,typeof(C2G_MagicLevelUp))]
    [Index(1003020,typeof(G2C_MagicLevelUp))]
    [Index(1003021,typeof(C2G_Shop))]
    [Index(1003022,typeof(G2C_Shop))]
    [Index(1003023,typeof(C2G_BuyItem))]
    [Index(1003024,typeof(G2C_BuyItem))]
    [Index(1004001,typeof(Task_G2C_SyncPackage))]
    [Index(1004003,typeof(Task_G2C_SyncHero))]
    [Index(1004005,typeof(Task_G2C_JoinBattle))]
    [Index(1005001,typeof(B2L_RegBattleServer))]
    [Index(1005002,typeof(L2B_RegBattleServer))]
    [Index(1005003,typeof(B2L_EndBattle))]
    [Index(1005004,typeof(L2B_EndBattle))]
    [Index(1005005,typeof(B2L_CheckSession))]
    [Index(1005006,typeof(L2B_CheckSession))]
    [Index(1005007,typeof(G2L_GateServerReg))]
    [Index(1005008,typeof(L2G_GateServerReg))]
    [Index(1005009,typeof(G2L_GateCheckSession))]
    [Index(1005010,typeof(L2G_GateCheckSession))]
    [Index(1005011,typeof(G2L_BeginBattle))]
    [Index(1005012,typeof(L2G_BeginBattle))]
    [Index(1005013,typeof(G2L_GetLastBattle))]
    [Index(1005014,typeof(L2G_GetLastBattle))]
    [Index(1006001,typeof(Task_L2G_ExitUser))]
    [Index(1007001,typeof(C2L_Login))]
    [Index(1007002,typeof(L2C_Login))]
    [Index(1007003,typeof(C2L_Reg))]
    [Index(1007004,typeof(L2C_Reg))]
    [Index(1008001,typeof(Task_L2B_ExitUser))]
    [Index(1009002,typeof(Notify_CharacterAlpha))]
    [Index(1009004,typeof(Notify_CharacterSetPosition))]
    [Index(1009006,typeof(Notify_CreateBattleCharacter))]
    [Index(1009008,typeof(Notify_CreateMissile))]
    [Index(1009010,typeof(Notify_CreateReleaser))]
    [Index(1009012,typeof(Notify_DamageResult))]
    [Index(1009014,typeof(Notify_Drop))]
    [Index(1009016,typeof(Notify_ElementExitState))]
    [Index(1009018,typeof(Notify_HPChange))]
    [Index(1009020,typeof(Notify_LayoutPlayMotion))]
    [Index(1009022,typeof(Notify_LayoutPlayParticle))]
    [Index(1009024,typeof(Notify_LookAtCharacter))]
    [Index(1009026,typeof(Notify_MPChange))]
    [Index(1009028,typeof(Notify_PlayerJoinState))]
    [Index(1009030,typeof(Notify_PropertyValue))]
    [Index(1009032,typeof(Notify_CharacterSetForword))]
    [Index(1009034,typeof(Notify_CharacterMoveTo))]
    [Index(1009036,typeof(Notify_CharacterStopMove))]
    [Index(1009038,typeof(Notify_CharacterDeath))]
    [Index(1009040,typeof(Notify_CharacterPriorityMove))]
    [Index(1009042,typeof(Notify_CharacterSetScale))]
    [Index(1009044,typeof(Notify_CharacterAttachMagic))]
    [Index(1009046,typeof(Notify_CharacterMoveForward))]
    [Index(1009048,typeof(Notify_CharacterSpeed))]
    [Index(1009050,typeof(Notify_CharacterLock))]
    [Index(1009052,typeof(Notify_CharacterPush))]
    [Index(1009054,typeof(Notify_CharacterRelive))]
    [Index(1009056,typeof(Notify_BattleItemChangeGroupIndex))]
    [Index(1009058,typeof(Notify_DropGold))]
    [Index(1009060,typeof(Notify_SyncServerTime))]

    [ApiVersion(0,0,1)]
    public static class MessageTypeIndexs
    {
        private static readonly Dictionary<int, Type> types = new Dictionary<int, Type>();

        private static readonly Dictionary<Type, int> indexs = new Dictionary<Type, int>();
        
        static MessageTypeIndexs()
        {
            var tys = typeof(MessageTypeIndexs).GetCustomAttributes(typeof(IndexAttribute), false) as IndexAttribute[];

            foreach(var t in tys)
            {
                types.Add(t.Index, t.TypeOfMessage);
                indexs.Add(t.TypeOfMessage, t.Index);
            }
            var ver = typeof(MessageTypeIndexs).GetCustomAttributes(typeof(ApiVersionAttribute), false) as ApiVersionAttribute[];
            if (ver != null && ver.Length > 0)
                Version = ver[0].Version;
        }
        public static int Version { get; private set; }

        /// <summary>
        /// Tries the index of the get.
        /// </summary>
        /// <returns><c>true</c>, if get index was tryed, <c>false</c> otherwise.</returns>
        /// <param name="type">Type.</param>
        /// <param name="index">Index.</param>
        public static bool TryGetIndex(Type type,out int index)
        {
            return indexs.TryGetValue(type, out index);
        }
        /// <summary>
        /// Tries the type of the get.
        /// </summary>
        /// <returns><c>true</c>, if get type was tryed, <c>false</c> otherwise.</returns>
        /// <param name="index">Index.</param>
        /// <param name="type">Type.</param>
        public static bool TryGetType(int index,out Type type)
        {
            return types.TryGetValue(index, out type);
        }
    }
}
