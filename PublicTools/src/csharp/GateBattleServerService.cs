
using Proto;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Proto.PServices;
using System.Threading.Tasks;
namespace Proto.GateBattleServerService
{

    /// <summary>
    /// 10044
    /// </summary>    
    [API(10044)]
    public class GetPlayerInfo:APIBase<B2G_GetPlayerInfo, G2B_GetPlayerInfo> 
    {
        private GetPlayerInfo() : base() { }
        public  static GetPlayerInfo CreateQuery(){ return new GetPlayerInfo();}
    }
    

    /// <summary>
    /// 10045
    /// </summary>    
    [API(10045)]
    public class BattleReward:APIBase<B2G_BattleReward, G2B_BattleReward> 
    {
        private BattleReward() : base() { }
        public  static BattleReward CreateQuery(){ return new BattleReward();}
    }
    

    public interface IGateBattleServerService
    {
        [API(10045)]G2B_BattleReward BattleReward(B2G_BattleReward req);
        [API(10044)]G2B_GetPlayerInfo GetPlayerInfo(B2G_GetPlayerInfo req);

    }
   

    public abstract class GateBattleServerService
    {
        [API(10045)]public abstract Task<G2B_BattleReward> BattleReward(B2G_BattleReward request);
        [API(10044)]public abstract Task<G2B_GetPlayerInfo> GetPlayerInfo(B2G_GetPlayerInfo request);

    }

}