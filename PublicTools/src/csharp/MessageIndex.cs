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

    [Index(10001,typeof(Void))]
    [Index(10002,typeof(Notify_CharacterAlpha))]
    [Index(10003,typeof(Notify_CharacterSetPosition))]
    [Index(10004,typeof(Notify_CreateBattleCharacter))]
    [Index(10005,typeof(Notify_CreateMissile))]
    [Index(10006,typeof(Notify_CreateReleaser))]
    [Index(10007,typeof(Notify_DamageResult))]
    [Index(10008,typeof(Notify_Drop))]
    [Index(10009,typeof(Notify_ElementExitState))]
    [Index(10010,typeof(Notify_HPChange))]
    [Index(10011,typeof(Notify_LayoutPlayMotion))]
    [Index(10012,typeof(Notify_LayoutPlayParticle))]
    [Index(10013,typeof(Notify_LookAtCharacter))]
    [Index(10014,typeof(Notify_MPChange))]
    [Index(10015,typeof(Notify_PlayerJoinState))]
    [Index(10016,typeof(Notify_PropertyValue))]
    [Index(10017,typeof(Notify_CharacterSetForword))]
    [Index(10018,typeof(Notify_CharacterMoveTo))]
    [Index(10019,typeof(Notify_CharacterStopMove))]
    [Index(10020,typeof(Notify_CharacterDeath))]
    [Index(10021,typeof(Notify_CharacterPriorityMove))]
    [Index(10022,typeof(Notify_CharacterSetScale))]
    [Index(10023,typeof(Notify_CharacterAttachMagic))]
    [Index(10024,typeof(Notify_CharacterMoveForward))]
    [Index(10025,typeof(Notify_CharacterSpeed))]
    [Index(10026,typeof(Action_ClickSkillIndex))]
    [Index(10027,typeof(Action_AutoFindTarget))]
    [Index(10028,typeof(Action_MoveDir))]
    [Index(10029,typeof(C2B_ExitBattle))]
    [Index(10030,typeof(B2C_ExitBattle))]
    [Index(10031,typeof(C2B_JoinBattle))]
    [Index(10032,typeof(B2C_JoinBattle))]
    [Index(10033,typeof(C2L_Login))]
    [Index(10034,typeof(L2C_Login))]
    [Index(10035,typeof(C2L_Reg))]
    [Index(10036,typeof(L2C_Reg))]
    [Index(10037,typeof(B2L_RegBattleServer))]
    [Index(10038,typeof(L2B_RegBattleServer))]
    [Index(10039,typeof(B2L_EndBattle))]
    [Index(10040,typeof(L2B_EndBattle))]
    [Index(10041,typeof(B2L_CheckSession))]
    [Index(10042,typeof(L2B_CheckSession))]
    [Index(10043,typeof(G2L_GateServerReg))]
    [Index(10044,typeof(L2G_GateServerReg))]
    [Index(10045,typeof(G2L_GateCheckSession))]
    [Index(10046,typeof(L2G_GateCheckSession))]
    [Index(10047,typeof(G2L_BeginBattle))]
    [Index(10048,typeof(L2G_BeginBattle))]
    [Index(10049,typeof(G2L_GetLastBattle))]
    [Index(10050,typeof(L2G_GetLastBattle))]
    [Index(10051,typeof(Task_L2B_ExitUser))]
    [Index(10052,typeof(C2G_Login))]
    [Index(10053,typeof(G2C_Login))]
    [Index(10054,typeof(C2G_CreateHero))]
    [Index(10055,typeof(G2C_CreateHero))]
    [Index(10056,typeof(C2G_BeginGame))]
    [Index(10057,typeof(G2C_BeginGame))]
    [Index(10058,typeof(C2G_GetLastBattle))]
    [Index(10059,typeof(G2C_GetLastBattle))]
    [Index(10060,typeof(C2G_OperatorEquip))]
    [Index(10061,typeof(G2C_OperatorEquip))]
    [Index(10062,typeof(C2G_SaleItem))]
    [Index(10063,typeof(G2C_SaleItem))]
    [Index(10064,typeof(C2G_EquipmentLevelUp))]
    [Index(10065,typeof(G2C_EquipmentLevelUp))]
    [Index(10066,typeof(C2G_GMTool))]
    [Index(10067,typeof(G2C_GMTool))]
    [Index(10068,typeof(Task_G2C_SyncPackage))]
    [Index(10069,typeof(Task_G2C_SyncHero))]
    [Index(10070,typeof(Task_G2C_JoinBattle))]
    [Index(10071,typeof(B2G_GetPlayerInfo))]
    [Index(10072,typeof(G2B_GetPlayerInfo))]
    [Index(10073,typeof(B2G_BattleReward))]
    [Index(10074,typeof(G2B_BattleReward))]

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
        }

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
