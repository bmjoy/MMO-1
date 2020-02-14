
using Proto;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Proto.PServices;
using System.Threading.Tasks;
namespace Proto.GateServerService
{

    /// <summary>
    /// 10043
    /// </summary>    
    [API(10043)]
    public class Login:APIBase<C2G_Login, G2C_Login> 
    {
        private Login() : base() { }
        public  static Login CreateQuery(){ return new Login();}
    }
    

    /// <summary>
    /// 10044
    /// </summary>    
    [API(10044)]
    public class CreateHero:APIBase<C2G_CreateHero, G2C_CreateHero> 
    {
        private CreateHero() : base() { }
        public  static CreateHero CreateQuery(){ return new CreateHero();}
    }
    

    /// <summary>
    /// 10045
    /// </summary>    
    [API(10045)]
    public class BeginGame:APIBase<C2G_BeginGame, G2C_BeginGame> 
    {
        private BeginGame() : base() { }
        public  static BeginGame CreateQuery(){ return new BeginGame();}
    }
    

    /// <summary>
    /// 10046
    /// </summary>    
    [API(10046)]
    public class GetLastBattle:APIBase<C2G_GetLastBattle, G2C_GetLastBattle> 
    {
        private GetLastBattle() : base() { }
        public  static GetLastBattle CreateQuery(){ return new GetLastBattle();}
    }
    

    /// <summary>
    /// 10047
    /// </summary>    
    [API(10047)]
    public class OperatorEquip:APIBase<C2G_OperatorEquip, G2C_OperatorEquip> 
    {
        private OperatorEquip() : base() { }
        public  static OperatorEquip CreateQuery(){ return new OperatorEquip();}
    }
    

    /// <summary>
    /// 10048
    /// </summary>    
    [API(10048)]
    public class SaleItem:APIBase<C2G_SaleItem, G2C_SaleItem> 
    {
        private SaleItem() : base() { }
        public  static SaleItem CreateQuery(){ return new SaleItem();}
    }
    

    /// <summary>
    /// 10049
    /// </summary>    
    [API(10049)]
    public class EquipmentLevelUp:APIBase<C2G_EquipmentLevelUp, G2C_EquipmentLevelUp> 
    {
        private EquipmentLevelUp() : base() { }
        public  static EquipmentLevelUp CreateQuery(){ return new EquipmentLevelUp();}
    }
    

    /// <summary>
    /// 10050
    /// </summary>    
    [API(10050)]
    public class GMTool:APIBase<C2G_GMTool, G2C_GMTool> 
    {
        private GMTool() : base() { }
        public  static GMTool CreateQuery(){ return new GMTool();}
    }
    

    /// <summary>
    /// 10051
    /// </summary>    
    [API(10051)]
    public class BuyPackageSize:APIBase<C2G_BuyPackageSize, G2C_BuyPackageSize> 
    {
        private BuyPackageSize() : base() { }
        public  static BuyPackageSize CreateQuery(){ return new BuyPackageSize();}
    }
    

    public interface IGateServerService
    {
        [API(10051)]G2C_BuyPackageSize BuyPackageSize(C2G_BuyPackageSize req);
        [API(10050)]G2C_GMTool GMTool(C2G_GMTool req);
        [API(10049)]G2C_EquipmentLevelUp EquipmentLevelUp(C2G_EquipmentLevelUp req);
        [API(10048)]G2C_SaleItem SaleItem(C2G_SaleItem req);
        [API(10047)]G2C_OperatorEquip OperatorEquip(C2G_OperatorEquip req);
        [API(10046)]G2C_GetLastBattle GetLastBattle(C2G_GetLastBattle req);
        [API(10045)]G2C_BeginGame BeginGame(C2G_BeginGame req);
        [API(10044)]G2C_CreateHero CreateHero(C2G_CreateHero req);
        [API(10043)]G2C_Login Login(C2G_Login req);

    }
   

    public abstract class GateServerService
    {
        [API(10051)]public abstract Task<G2C_BuyPackageSize> BuyPackageSize(C2G_BuyPackageSize request);
        [API(10050)]public abstract Task<G2C_GMTool> GMTool(C2G_GMTool request);
        [API(10049)]public abstract Task<G2C_EquipmentLevelUp> EquipmentLevelUp(C2G_EquipmentLevelUp request);
        [API(10048)]public abstract Task<G2C_SaleItem> SaleItem(C2G_SaleItem request);
        [API(10047)]public abstract Task<G2C_OperatorEquip> OperatorEquip(C2G_OperatorEquip request);
        [API(10046)]public abstract Task<G2C_GetLastBattle> GetLastBattle(C2G_GetLastBattle request);
        [API(10045)]public abstract Task<G2C_BeginGame> BeginGame(C2G_BeginGame request);
        [API(10044)]public abstract Task<G2C_CreateHero> CreateHero(C2G_CreateHero request);
        [API(10043)]public abstract Task<G2C_Login> Login(C2G_Login request);

    }

}