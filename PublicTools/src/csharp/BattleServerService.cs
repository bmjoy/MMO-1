
using Proto;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Proto.PServices;
using System.Threading.Tasks;
namespace Proto.BattleServerService
{

    /// <summary>
    /// 10027
    /// </summary>    
    [API(10027)]
    public class ExitBattle:APIBase<C2B_ExitBattle, B2C_ExitBattle> 
    {
        private ExitBattle() : base() { }
        public  static ExitBattle CreateQuery(){ return new ExitBattle();}
    }
    

    /// <summary>
    /// 10028
    /// </summary>    
    [API(10028)]
    public class JoinBattle:APIBase<C2B_JoinBattle, B2C_JoinBattle> 
    {
        private JoinBattle() : base() { }
        public  static JoinBattle CreateQuery(){ return new JoinBattle();}
    }
    

    public interface IBattleServerService
    {
        [API(10028)]B2C_JoinBattle JoinBattle(C2B_JoinBattle req);
        [API(10027)]B2C_ExitBattle ExitBattle(C2B_ExitBattle req);

    }
   

    public abstract class BattleServerService
    {
        [API(10028)]public abstract Task<B2C_JoinBattle> JoinBattle(C2B_JoinBattle request);
        [API(10027)]public abstract Task<B2C_ExitBattle> ExitBattle(C2B_ExitBattle request);

    }

}