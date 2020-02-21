
using Proto;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Proto.PServices;
using System.Threading.Tasks;
namespace Proto.LoginBattleGameServerService
{

    /// <summary>
    /// 3
    /// </summary>    
    [API(3)]
    public class RegBattleServer:APIBase<B2L_RegBattleServer, L2B_RegBattleServer> 
    {
        private RegBattleServer() : base() { }
        public  static RegBattleServer CreateQuery(){ return new RegBattleServer();}
    }
    

    /// <summary>
    /// 4
    /// </summary>    
    [API(4)]
    public class EndBattle:APIBase<B2L_EndBattle, L2B_EndBattle> 
    {
        private EndBattle() : base() { }
        public  static EndBattle CreateQuery(){ return new EndBattle();}
    }
    

    /// <summary>
    /// 5
    /// </summary>    
    [API(5)]
    public class CheckSession:APIBase<B2L_CheckSession, L2B_CheckSession> 
    {
        private CheckSession() : base() { }
        public  static CheckSession CreateQuery(){ return new CheckSession();}
    }
    

    /// <summary>
    /// 6
    /// </summary>    
    [API(6)]
    public class RegGateServer:APIBase<G2L_GateServerReg, L2G_GateServerReg> 
    {
        private RegGateServer() : base() { }
        public  static RegGateServer CreateQuery(){ return new RegGateServer();}
    }
    

    /// <summary>
    /// 7
    /// </summary>    
    [API(7)]
    public class GateServerSession:APIBase<G2L_GateCheckSession, L2G_GateCheckSession> 
    {
        private GateServerSession() : base() { }
        public  static GateServerSession CreateQuery(){ return new GateServerSession();}
    }
    

    /// <summary>
    /// 8
    /// </summary>    
    [API(8)]
    public class BeginBattle:APIBase<G2L_BeginBattle, L2G_BeginBattle> 
    {
        private BeginBattle() : base() { }
        public  static BeginBattle CreateQuery(){ return new BeginBattle();}
    }
    

    /// <summary>
    /// 9
    /// </summary>    
    [API(9)]
    public class GetLastBattle:APIBase<G2L_GetLastBattle, L2G_GetLastBattle> 
    {
        private GetLastBattle() : base() { }
        public  static GetLastBattle CreateQuery(){ return new GetLastBattle();}
    }
    

    public interface ILoginBattleGameServerService
    {
        [API(9)]L2G_GetLastBattle GetLastBattle(G2L_GetLastBattle req);
        [API(8)]L2G_BeginBattle BeginBattle(G2L_BeginBattle req);
        [API(7)]L2G_GateCheckSession GateServerSession(G2L_GateCheckSession req);
        [API(6)]L2G_GateServerReg RegGateServer(G2L_GateServerReg req);
        [API(5)]L2B_CheckSession CheckSession(B2L_CheckSession req);
        [API(4)]L2B_EndBattle EndBattle(B2L_EndBattle req);
        [API(3)]L2B_RegBattleServer RegBattleServer(B2L_RegBattleServer req);

    }
   

    public abstract class LoginBattleGameServerService
    {
        [API(9)]public abstract Task<L2G_GetLastBattle> GetLastBattle(G2L_GetLastBattle request);
        [API(8)]public abstract Task<L2G_BeginBattle> BeginBattle(G2L_BeginBattle request);
        [API(7)]public abstract Task<L2G_GateCheckSession> GateServerSession(G2L_GateCheckSession request);
        [API(6)]public abstract Task<L2G_GateServerReg> RegGateServer(G2L_GateServerReg request);
        [API(5)]public abstract Task<L2B_CheckSession> CheckSession(B2L_CheckSession request);
        [API(4)]public abstract Task<L2B_EndBattle> EndBattle(B2L_EndBattle request);
        [API(3)]public abstract Task<L2B_RegBattleServer> RegBattleServer(B2L_RegBattleServer request);

    }

}