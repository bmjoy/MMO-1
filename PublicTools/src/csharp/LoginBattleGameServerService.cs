
using Proto;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Proto.PServices;
using System.Threading.Tasks;
namespace Proto.LoginBattleGameServerService
{

    /// <summary>
    /// 10048
    /// </summary>    
    [API(10048)]
    public class RegBattleServer:APIBase<B2L_RegBattleServer, L2B_RegBattleServer> 
    {
        private RegBattleServer() : base() { }
        public  static RegBattleServer CreateQuery(){ return new RegBattleServer();}
    }
    

    /// <summary>
    /// 10049
    /// </summary>    
    [API(10049)]
    public class EndBattle:APIBase<B2L_EndBattle, L2B_EndBattle> 
    {
        private EndBattle() : base() { }
        public  static EndBattle CreateQuery(){ return new EndBattle();}
    }
    

    /// <summary>
    /// 10050
    /// </summary>    
    [API(10050)]
    public class CheckSession:APIBase<B2L_CheckSession, L2B_CheckSession> 
    {
        private CheckSession() : base() { }
        public  static CheckSession CreateQuery(){ return new CheckSession();}
    }
    

    /// <summary>
    /// 10051
    /// </summary>    
    [API(10051)]
    public class RegGateServer:APIBase<G2L_GateServerReg, L2G_GateServerReg> 
    {
        private RegGateServer() : base() { }
        public  static RegGateServer CreateQuery(){ return new RegGateServer();}
    }
    

    /// <summary>
    /// 10052
    /// </summary>    
    [API(10052)]
    public class GateServerSession:APIBase<G2L_GateCheckSession, L2G_GateCheckSession> 
    {
        private GateServerSession() : base() { }
        public  static GateServerSession CreateQuery(){ return new GateServerSession();}
    }
    

    /// <summary>
    /// 10053
    /// </summary>    
    [API(10053)]
    public class BeginBattle:APIBase<G2L_BeginBattle, L2G_BeginBattle> 
    {
        private BeginBattle() : base() { }
        public  static BeginBattle CreateQuery(){ return new BeginBattle();}
    }
    

    /// <summary>
    /// 10054
    /// </summary>    
    [API(10054)]
    public class GetLastBattle:APIBase<G2L_GetLastBattle, L2G_GetLastBattle> 
    {
        private GetLastBattle() : base() { }
        public  static GetLastBattle CreateQuery(){ return new GetLastBattle();}
    }
    

    public interface ILoginBattleGameServerService
    {
        [API(10054)]L2G_GetLastBattle GetLastBattle(G2L_GetLastBattle req);
        [API(10053)]L2G_BeginBattle BeginBattle(G2L_BeginBattle req);
        [API(10052)]L2G_GateCheckSession GateServerSession(G2L_GateCheckSession req);
        [API(10051)]L2G_GateServerReg RegGateServer(G2L_GateServerReg req);
        [API(10050)]L2B_CheckSession CheckSession(B2L_CheckSession req);
        [API(10049)]L2B_EndBattle EndBattle(B2L_EndBattle req);
        [API(10048)]L2B_RegBattleServer RegBattleServer(B2L_RegBattleServer req);

    }
   

    public abstract class LoginBattleGameServerService
    {
        [API(10054)]public abstract Task<L2G_GetLastBattle> GetLastBattle(G2L_GetLastBattle request);
        [API(10053)]public abstract Task<L2G_BeginBattle> BeginBattle(G2L_BeginBattle request);
        [API(10052)]public abstract Task<L2G_GateCheckSession> GateServerSession(G2L_GateCheckSession request);
        [API(10051)]public abstract Task<L2G_GateServerReg> RegGateServer(G2L_GateServerReg request);
        [API(10050)]public abstract Task<L2B_CheckSession> CheckSession(B2L_CheckSession request);
        [API(10049)]public abstract Task<L2B_EndBattle> EndBattle(B2L_EndBattle request);
        [API(10048)]public abstract Task<L2B_RegBattleServer> RegBattleServer(B2L_RegBattleServer request);

    }

}