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


    [Index(1001001,typeof(Void))]
    [Index(1001002,typeof(Notify_CharacterAlpha))]
    [Index(1001004,typeof(Notify_CharacterSetPosition))]
    [Index(1001006,typeof(Notify_CreateBattleCharacter))]
    [Index(1001008,typeof(Notify_CreateMissile))]
    [Index(1001010,typeof(Notify_CreateReleaser))]
    [Index(1001012,typeof(Notify_DamageResult))]
    [Index(1001014,typeof(Notify_Drop))]
    [Index(1001016,typeof(Notify_ElementExitState))]
    [Index(1001018,typeof(Notify_HPChange))]
    [Index(1001020,typeof(Notify_LayoutPlayMotion))]
    [Index(1001022,typeof(Notify_LayoutPlayParticle))]
    [Index(1001024,typeof(Notify_LookAtCharacter))]
    [Index(1001026,typeof(Notify_MPChange))]
    [Index(1001028,typeof(Notify_PlayerJoinState))]
    [Index(1001030,typeof(Notify_PropertyValue))]
    [Index(1001032,typeof(Notify_CharacterSetForword))]
    [Index(1001034,typeof(Notify_CharacterMoveTo))]
    [Index(1001036,typeof(Notify_CharacterStopMove))]
    [Index(1001038,typeof(Notify_CharacterDeath))]
    [Index(1001040,typeof(Notify_CharacterPriorityMove))]
    [Index(1001042,typeof(Notify_CharacterSetScale))]
    [Index(1001044,typeof(Notify_CharacterAttachMagic))]
    [Index(1001046,typeof(Notify_CharacterMoveForward))]
    [Index(1001048,typeof(Notify_CharacterSpeed))]
    [Index(1001050,typeof(Notify_CharacterLock))]
    [Index(1001052,typeof(Notify_CharacterPush))]
    [Index(1001054,typeof(Notify_CharacterRelive))]
    [Index(1002001,typeof(Action_ClickSkillIndex))]
    [Index(1002003,typeof(Action_AutoFindTarget))]
    [Index(1002005,typeof(Action_MoveDir))]
    [Index(1002007,typeof(Action_NormalAttack))]
    [Index(1003001,typeof(C2B_ExitBattle))]
    [Index(1003002,typeof(B2C_ExitBattle))]
    [Index(1003003,typeof(C2B_JoinBattle))]
    [Index(1003004,typeof(B2C_JoinBattle))]
    [Index(1004001,typeof(C2G_Login))]
    [Index(1004002,typeof(G2C_Login))]
    [Index(1004003,typeof(C2G_CreateHero))]
    [Index(1004004,typeof(G2C_CreateHero))]
    [Index(1004005,typeof(C2G_BeginGame))]
    [Index(1004006,typeof(G2C_BeginGame))]
    [Index(1004007,typeof(C2G_GetLastBattle))]
    [Index(1004008,typeof(G2C_GetLastBattle))]
    [Index(1004009,typeof(C2G_OperatorEquip))]
    [Index(1004010,typeof(G2C_OperatorEquip))]
    [Index(1004011,typeof(C2G_SaleItem))]
    [Index(1004012,typeof(G2C_SaleItem))]
    [Index(1004013,typeof(C2G_EquipmentLevelUp))]
    [Index(1004014,typeof(G2C_EquipmentLevelUp))]
    [Index(1004015,typeof(C2G_GMTool))]
    [Index(1004016,typeof(G2C_GMTool))]
    [Index(1004017,typeof(C2G_BuyPackageSize))]
    [Index(1004018,typeof(G2C_BuyPackageSize))]
    [Index(1005001,typeof(Task_G2C_SyncPackage))]
    [Index(1005003,typeof(Task_G2C_SyncHero))]
    [Index(1005005,typeof(Task_G2C_JoinBattle))]
    [Index(1006001,typeof(B2G_GetPlayerInfo))]
    [Index(1006002,typeof(G2B_GetPlayerInfo))]
    [Index(1006003,typeof(B2G_BattleReward))]
    [Index(1006004,typeof(G2B_BattleReward))]
    [Index(1007001,typeof(C2L_Login))]
    [Index(1007002,typeof(L2C_Login))]
    [Index(1007003,typeof(C2L_Reg))]
    [Index(1007004,typeof(L2C_Reg))]
    [Index(1008001,typeof(B2L_RegBattleServer))]
    [Index(1008002,typeof(L2B_RegBattleServer))]
    [Index(1008003,typeof(B2L_EndBattle))]
    [Index(1008004,typeof(L2B_EndBattle))]
    [Index(1008005,typeof(B2L_CheckSession))]
    [Index(1008006,typeof(L2B_CheckSession))]
    [Index(1008007,typeof(G2L_GateServerReg))]
    [Index(1008008,typeof(L2G_GateServerReg))]
    [Index(1008009,typeof(G2L_GateCheckSession))]
    [Index(1008010,typeof(L2G_GateCheckSession))]
    [Index(1008011,typeof(G2L_BeginBattle))]
    [Index(1008012,typeof(L2G_BeginBattle))]
    [Index(1008013,typeof(G2L_GetLastBattle))]
    [Index(1008014,typeof(L2G_GetLastBattle))]
    [Index(1009001,typeof(Task_L2B_ExitUser))]
    [Index(1010001,typeof(Task_L2G_ExitUser))]

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
