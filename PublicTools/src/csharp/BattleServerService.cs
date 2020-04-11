
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
    

    /// <summary>
    /// 4
    /// </summary>    
    [API(4)]
    public class ViewPlayerHero:APIBase<C2B_ViewPlayerHero, B2C_ViewPlayerHero> 
    {
        private ViewPlayerHero() : base() { }
        public  static ViewPlayerHero CreateQuery(){ return new ViewPlayerHero();}
    }
    

    public interface IBattleServerService
    {
        [API(4)]B2C_ViewPlayerHero ViewPlayerHero(C2B_ViewPlayerHero req);
        [API(3)]B2C_JoinBattle JoinBattle(C2B_JoinBattle req);
        [API(1)]B2C_ExitBattle ExitBattle(C2B_ExitBattle req);

    }
   

    public abstract class BattleServerService
    {
        [API(4)]public abstract Task<B2C_ViewPlayerHero> ViewPlayerHero(C2B_ViewPlayerHero request);
        [API(3)]public abstract Task<B2C_JoinBattle> JoinBattle(C2B_JoinBattle request);
        [API(1)]public abstract Task<B2C_ExitBattle> ExitBattle(C2B_ExitBattle request);

    }

}