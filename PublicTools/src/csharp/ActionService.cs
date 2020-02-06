
using Proto;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Proto.PServices;
using System.Threading.Tasks;
namespace Proto.ActionService
{

    /// <summary>
    /// 10025
    /// </summary>    
    [API(10025)]
    public class ClickSkillIndex:APIBase<Action_ClickSkillIndex, Void> 
    {
        private ClickSkillIndex() : base() { }
        public  static ClickSkillIndex CreateQuery(){ return new ClickSkillIndex();}
    }
    

    /// <summary>
    /// 10026
    /// </summary>    
    [API(10026)]
    public class AutoFindTarget:APIBase<Action_AutoFindTarget, Void> 
    {
        private AutoFindTarget() : base() { }
        public  static AutoFindTarget CreateQuery(){ return new AutoFindTarget();}
    }
    

    /// <summary>
    /// 10027
    /// </summary>    
    [API(10027)]
    public class MoveDir:APIBase<Action_MoveDir, Void> 
    {
        private MoveDir() : base() { }
        public  static MoveDir CreateQuery(){ return new MoveDir();}
    }
    

    public interface IActionService
    {
        [API(10027)]Void MoveDir(Action_MoveDir req);
        [API(10026)]Void AutoFindTarget(Action_AutoFindTarget req);
        [API(10025)]Void ClickSkillIndex(Action_ClickSkillIndex req);

    }
   

    public abstract class ActionService
    {
        [API(10027)]public abstract Task<Void> MoveDir(Action_MoveDir request);
        [API(10026)]public abstract Task<Void> AutoFindTarget(Action_AutoFindTarget request);
        [API(10025)]public abstract Task<Void> ClickSkillIndex(Action_ClickSkillIndex request);

    }

}