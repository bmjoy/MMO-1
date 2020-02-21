
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
    /// 3
    /// </summary>    
    [API(3)]
    public class MoveDir:APIBase<Action_MoveDir, Void> 
    {
        private MoveDir() : base() { }
        public  static MoveDir CreateQuery(){ return new MoveDir();}
    }
    

    /// <summary>
    /// 4
    /// </summary>    
    [API(4)]
    public class NormalAttack:APIBase<Action_NormalAttack, Action_NormalAttack> 
    {
        private NormalAttack() : base() { }
        public  static NormalAttack CreateQuery(){ return new NormalAttack();}
    }
    

    public interface IActionService
    {
        [API(4)]Action_NormalAttack NormalAttack(Action_NormalAttack req);
        [API(3)]Void MoveDir(Action_MoveDir req);
        [API(2)]Void AutoFindTarget(Action_AutoFindTarget req);
        [API(1)]Void ClickSkillIndex(Action_ClickSkillIndex req);

    }
   

    public abstract class ActionService
    {
        [API(4)]public abstract Task<Action_NormalAttack> NormalAttack(Action_NormalAttack request);
        [API(3)]public abstract Task<Void> MoveDir(Action_MoveDir request);
        [API(2)]public abstract Task<Void> AutoFindTarget(Action_AutoFindTarget request);
        [API(1)]public abstract Task<Void> ClickSkillIndex(Action_ClickSkillIndex request);

    }

}