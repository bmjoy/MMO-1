
using Proto;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Proto.PServices;
using System.Threading.Tasks;
namespace Proto.BattleServerService
{

    /// <summary>
    /// 10030
    /// </summary>    
    [API(10030)]
    public class ExitBattle:APIBase<C2B_ExitBattle, B2C_ExitBattle> 
    {
        private ExitBattle() : base() { }
        public  static ExitBattle CreateQuery(){ return new ExitBattle();}
    }
    

    /// <summary>
    /// 10031
    /// </summary>    
    [API(10031)]
    public class JoinBattle:APIBase<C2B_JoinBattle, B2C_JoinBattle> 
    {
        private JoinBattle() : base() { }
        public  static JoinBattle CreateQuery(){ return new JoinBattle();}
    }
    

    public interface IBattleServerService
    {
        [API(10031)]B2C_JoinBattle JoinBattle(C2B_JoinBattle req);
        [API(10030)]B2C_ExitBattle ExitBattle(C2B_ExitBattle req);

    }
   

    public abstract class BattleServerService
    {
        [API(10031)]public abstract Task<B2C_JoinBattle> JoinBattle(C2B_JoinBattle request);
        [API(10030)]public abstract Task<B2C_ExitBattle> ExitBattle(C2B_ExitBattle request);

    }

}