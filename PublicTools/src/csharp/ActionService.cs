
using Proto;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Proto.PServices;
using System.Threading.Tasks;
namespace Proto.ActionService
{

    /// <summary>
    /// 1
    /// </summary>    
    [API(1)]
    public class ClickSkillIndex:APIBase<Action_ClickSkillIndex, Void> 
    {
        private ClickSkillIndex() : base() { }
        public  static ClickSkillIndex CreateQuery(){ return new ClickSkillIndex();}
    }
    

    /// <summary>
    /// 2
    /// </summary>    
    [API(2)]
    public class AutoFindTarget:APIBase<Action_AutoFindTarget, Void> 
    {
        private AutoFindTarget() : base() { }
        public  static AutoFindTarget CreateQuery(){ return new AutoFindTarget();}
    }
    

    /// <summary>
    /// 4
    /// </summary>    
    [API(4)]
    public class NormalAttack:APIBase<Action_NormalAttack, Void> 
    {
        private NormalAttack() : base() { }
        public  static NormalAttack CreateQuery(){ return new NormalAttack();}
    }
    

    /// <summary>
    /// 5
    /// </summary>    
    [API(5)]
    public class CollectItem:APIBase<Action_CollectItem, Void> 
    {
        private CollectItem() : base() { }
        public  static CollectItem CreateQuery(){ return new CollectItem();}
    }
    

    /// <summary>
    /// 6
    /// </summary>    
    [API(6)]
    public class UseItem:APIBase<Action_UseItem, Void> 
    {
        private UseItem() : base() { }
        public  static UseItem CreateQuery(){ return new UseItem();}
    }
    

    /// <summary>
    /// 7
    /// </summary>    
    [API(7)]
    public class MoveJoystick:APIBase<Action_MoveJoystick, Void> 
    {
        private MoveJoystick() : base() { }
        public  static MoveJoystick CreateQuery(){ return new MoveJoystick();}
    }
    

    /// <summary>
    /// 8
    /// </summary>    
    [API(8)]
    public class StopMove:APIBase<Action_StopMove, Void> 
    {
        private StopMove() : base() { }
        public  static StopMove CreateQuery(){ return new StopMove();}
    }
    

    /// <summary>
    /// 9
    /// </summary>    
    [API(9)]
    public class LookRotation:APIBase<Action_LookRotation, Void> 
    {
        private LookRotation() : base() { }
        public  static LookRotation CreateQuery(){ return new LookRotation();}
    }
    

    public interface IActionService
    {
        [API(9)]Void LookRotation(Action_LookRotation req);
        [API(8)]Void StopMove(Action_StopMove req);
        [API(7)]Void MoveJoystick(Action_MoveJoystick req);
        [API(6)]Void UseItem(Action_UseItem req);
        [API(5)]Void CollectItem(Action_CollectItem req);
        [API(4)]Void NormalAttack(Action_NormalAttack req);
        [API(2)]Void AutoFindTarget(Action_AutoFindTarget req);
        [API(1)]Void ClickSkillIndex(Action_ClickSkillIndex req);

    }
   

    public abstract class ActionService
    {
        [API(9)]public abstract Task<Void> LookRotation(Action_LookRotation request);
        [API(8)]public abstract Task<Void> StopMove(Action_StopMove request);
        [API(7)]public abstract Task<Void> MoveJoystick(Action_MoveJoystick request);
        [API(6)]public abstract Task<Void> UseItem(Action_UseItem request);
        [API(5)]public abstract Task<Void> CollectItem(Action_CollectItem request);
        [API(4)]public abstract Task<Void> NormalAttack(Action_NormalAttack request);
        [API(2)]public abstract Task<Void> AutoFindTarget(Action_AutoFindTarget request);
        [API(1)]public abstract Task<Void> ClickSkillIndex(Action_ClickSkillIndex request);

    }

}