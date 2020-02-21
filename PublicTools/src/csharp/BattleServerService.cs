
using Proto;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Proto.PServices;
using System.Threading.Tasks;
namespace Proto.BattleServerService
{

    /// <summary>
    /// 1
    /// </summary>    
    [API(1)]
    public class ExitBattle:APIBase<C2B_ExitBattle, B2C_ExitBattle> 
    {
        private ExitBattle() : base() { }
        public  static ExitBattle CreateQuery(){ return new ExitBattle();}
    }
    

    /// <summary>
    /// 3
    /// </summary>    
    [API(3)]
    public class JoinBattle:APIBase<C2B_JoinBattle, B2C_JoinBattle> 
    {
        private JoinBattle() : base() { }
        public  static JoinBattle CreateQuery(){ return new JoinBattle();}
    }
    

    public interface IBattleServerService
    {
        [API(3)]B2C_JoinBattle JoinBattle(C2B_JoinBattle req);
        [API(1)]B2C_ExitBattle ExitBattle(C2B_ExitBattle req);

    }
   

    public abstract class BattleServerService
    {
        [API(3)]public abstract Task<B2C_JoinBattle> JoinBattle(C2B_JoinBattle request);
        [API(1)]public abstract Task<B2C_ExitBattle> ExitBattle(C2B_ExitBattle request);

    }

}