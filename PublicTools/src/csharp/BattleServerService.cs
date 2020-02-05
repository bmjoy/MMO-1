
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
    public class ExitGame:APIBase<C2B_ExitGame, B2C_ExitGame> 
    {
        private ExitGame() : base() { }
        public  static ExitGame CreateQuery(){ return new ExitGame();}
    }
    

    /// <summary>
    /// 10029
    /// </summary>    
    [API(10029)]
    public class JoinBattle:APIBase<C2B_JoinBattle, B2C_JoinBattle> 
    {
        private JoinBattle() : base() { }
        public  static JoinBattle CreateQuery(){ return new JoinBattle();}
    }
    

    public interface IBattleServerService
    {
        [API(10029)]B2C_JoinBattle JoinBattle(C2B_JoinBattle req);
        [API(10028)]B2C_ExitGame ExitGame(C2B_ExitGame req);
        [API(10027)]B2C_ExitBattle ExitBattle(C2B_ExitBattle req);

    }
   

    public abstract class BattleServerService
    {
        [API(10029)]public abstract Task<B2C_JoinBattle> JoinBattle(C2B_JoinBattle request);
        [API(10028)]public abstract Task<B2C_ExitGame> ExitGame(C2B_ExitGame request);
        [API(10027)]public abstract Task<B2C_ExitBattle> ExitBattle(C2B_ExitBattle request);

    }

}