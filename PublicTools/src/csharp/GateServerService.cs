
using Proto;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Proto.PServices;
using System.Threading.Tasks;
namespace Proto.GateServerService
{

    /// <summary>
    /// 1
    /// </summary>    
    [API(1)]
    public class Login:APIBase<C2G_Login, G2C_Login> 
    {
        private Login() : base() { }
        public  static Login CreateQuery(){ return new Login();}
    }
    

    /// <summary>
    /// 2
    /// </summary>    
    [API(2)]
    public class CreateHero:APIBase<C2G_CreateHero, G2C_CreateHero> 
    {
        private CreateHero() : base() { }
        public  static CreateHero CreateQuery(){ return new CreateHero();}
    }
    

    /// <summary>
    /// 3
    /// </summary>    
    [API(3)]
    public class BeginGame:APIBase<C2G_BeginGame, G2C_BeginGame> 
    {
        private BeginGame() : base() { }
        public  static BeginGame CreateQuery(){ return new BeginGame();}
    }
    

    /// <summary>
    /// 4
    /// </summary>    
    [API(4)]
    public class GetLastBattle:APIBase<C2G_GetLastBattle, G2C_GetLastBattle> 
    {
        private GetLastBattle() : base() { }
        public  static GetLastBattle CreateQuery(){ return new GetLastBattle();}
    }
    

    /// <summary>
    /// 5
    /// </summary>    
    [API(5)]
    public class OperatorEquip:APIBase<C2G_OperatorEquip, G2C_OperatorEquip> 
    {
        private OperatorEquip() : base() { }
        public  static OperatorEquip CreateQuery(){ return new OperatorEquip();}
    }
    

    /// <summary>
    /// 6
    /// </summary>    
    [API(6)]
    public class SaleItem:APIBase<C2G_SaleItem, G2C_SaleItem> 
    {
        private SaleItem() : base() { }
        public  static SaleItem CreateQuery(){ return new SaleItem();}
    }
    

    /// <summary>
    /// 7
    /// </summary>    
    [API(7)]
    public class EquipmentLevelUp:APIBase<C2G_EquipmentLevelUp, G2C_EquipmentLevelUp> 
    {
        private EquipmentLevelUp() : base() { }
        public  static EquipmentLevelUp CreateQuery(){ return new EquipmentLevelUp();}
    }
    

    /// <summary>
    /// 8
    /// </summary>    
    [API(8)]
    public class GMTool:APIBase<C2G_GMTool, G2C_GMTool> 
    {
        private GMTool() : base() { }
        public  static GMTool CreateQuery(){ return new GMTool();}
    }
    

    /// <summary>
    /// 9
    /// </summary>    
    [API(9)]
    public class BuyPackageSize:APIBase<C2G_BuyPackageSize, G2C_BuyPackageSize> 
    {
        private BuyPackageSize() : base() { }
        public  static BuyPackageSize CreateQuery(){ return new BuyPackageSize();}
    }
    

    public interface IGateServerService
    {
        [API(9)]G2C_BuyPackageSize BuyPackageSize(C2G_BuyPackageSize req);
        [API(8)]G2C_GMTool GMTool(C2G_GMTool req);
        [API(7)]G2C_EquipmentLevelUp EquipmentLevelUp(C2G_EquipmentLevelUp req);
        [API(6)]G2C_SaleItem SaleItem(C2G_SaleItem req);
        [API(5)]G2C_OperatorEquip OperatorEquip(C2G_OperatorEquip req);
        [API(4)]G2C_GetLastBattle GetLastBattle(C2G_GetLastBattle req);
        [API(3)]G2C_BeginGame BeginGame(C2G_BeginGame req);
        [API(2)]G2C_CreateHero CreateHero(C2G_CreateHero req);
        [API(1)]G2C_Login Login(C2G_Login req);

    }
   

    public abstract class GateServerService
    {
        [API(9)]public abstract Task<G2C_BuyPackageSize> BuyPackageSize(C2G_BuyPackageSize request);
        [API(8)]public abstract Task<G2C_GMTool> GMTool(C2G_GMTool request);
        [API(7)]public abstract Task<G2C_EquipmentLevelUp> EquipmentLevelUp(C2G_EquipmentLevelUp request);
        [API(6)]public abstract Task<G2C_SaleItem> SaleItem(C2G_SaleItem request);
        [API(5)]public abstract Task<G2C_OperatorEquip> OperatorEquip(C2G_OperatorEquip request);
        [API(4)]public abstract Task<G2C_GetLastBattle> GetLastBattle(C2G_GetLastBattle request);
        [API(3)]public abstract Task<G2C_BeginGame> BeginGame(C2G_BeginGame request);
        [API(2)]public abstract Task<G2C_CreateHero> CreateHero(C2G_CreateHero request);
        [API(1)]public abstract Task<G2C_Login> Login(C2G_Login request);

    }

}