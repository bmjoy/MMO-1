
using Proto;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Proto.PServices;
using System.Threading.Tasks;
namespace Proto.GateServerTask
{

    /// <summary>
    /// 1
    /// </summary>    
    [API(1)]
    public class SyncPackage:APIBase<Task_G2C_SyncPackage, Task_G2C_SyncPackage> 
    {
        private SyncPackage() : base() { }
        public  static SyncPackage CreateQuery(){ return new SyncPackage();}
    }
    

    /// <summary>
    /// 2
    /// </summary>    
    [API(2)]
    public class SyncHero:APIBase<Task_G2C_SyncHero, Task_G2C_SyncHero> 
    {
        private SyncHero() : base() { }
        public  static SyncHero CreateQuery(){ return new SyncHero();}
    }
    

    /// <summary>
    /// 3
    /// </summary>    
    [API(3)]
    public class JoinBattle:APIBase<Task_G2C_JoinBattle, Task_G2C_JoinBattle> 
    {
        private JoinBattle() : base() { }
        public  static JoinBattle CreateQuery(){ return new JoinBattle();}
    }
    

    /// <summary>
    /// 4
    /// </summary>    
    [API(4)]
    public class ModifyItem:APIBase<Task_ModifyItem, Task_ModifyItem> 
    {
        private ModifyItem() : base() { }
        public  static ModifyItem CreateQuery(){ return new ModifyItem();}
    }
    

    /// <summary>
    /// 5
    /// </summary>    
    [API(5)]
    public class CoinAndGold:APIBase<Task_CoinAndGold, Task_CoinAndGold> 
    {
        private CoinAndGold() : base() { }
        public  static CoinAndGold CreateQuery(){ return new CoinAndGold();}
    }
    

    /// <summary>
    /// 6
    /// </summary>    
    [API(6)]
    public class PackageSize:APIBase<Task_PackageSize, Task_PackageSize> 
    {
        private PackageSize() : base() { }
        public  static PackageSize CreateQuery(){ return new PackageSize();}
    }
    

    public interface IGateServerTask
    {
        [API(6)]Task_PackageSize PackageSize(Task_PackageSize req);
        [API(5)]Task_CoinAndGold CoinAndGold(Task_CoinAndGold req);
        [API(4)]Task_ModifyItem ModifyItem(Task_ModifyItem req);
        [API(3)]Task_G2C_JoinBattle JoinBattle(Task_G2C_JoinBattle req);
        [API(2)]Task_G2C_SyncHero SyncHero(Task_G2C_SyncHero req);
        [API(1)]Task_G2C_SyncPackage SyncPackage(Task_G2C_SyncPackage req);

    }
   

    public abstract class GateServerTask
    {
        [API(6)]public abstract Task<Task_PackageSize> PackageSize(Task_PackageSize request);
        [API(5)]public abstract Task<Task_CoinAndGold> CoinAndGold(Task_CoinAndGold request);
        [API(4)]public abstract Task<Task_ModifyItem> ModifyItem(Task_ModifyItem request);
        [API(3)]public abstract Task<Task_G2C_JoinBattle> JoinBattle(Task_G2C_JoinBattle request);
        [API(2)]public abstract Task<Task_G2C_SyncHero> SyncHero(Task_G2C_SyncHero request);
        [API(1)]public abstract Task<Task_G2C_SyncPackage> SyncPackage(Task_G2C_SyncPackage request);

    }

}