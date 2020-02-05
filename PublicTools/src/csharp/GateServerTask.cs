
using Proto;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Proto.PServices;
using System.Threading.Tasks;
namespace Proto.GateServerTask
{

    /// <summary>
    /// 10047
    /// </summary>    
    [API(10047)]
    public class SyncPackage:APIBase<Task_G2C_SyncPackage, Task_G2C_SyncPackage> 
    {
        private SyncPackage() : base() { }
        public  static SyncPackage CreateQuery(){ return new SyncPackage();}
    }
    

    /// <summary>
    /// 10048
    /// </summary>    
    [API(10048)]
    public class SyncHero:APIBase<Task_G2C_SyncHero, Task_G2C_SyncHero> 
    {
        private SyncHero() : base() { }
        public  static SyncHero CreateQuery(){ return new SyncHero();}
    }
    

    /// <summary>
    /// 10049
    /// </summary>    
    [API(10049)]
    public class JoinBattle:APIBase<Task_G2C_JoinBattle, Task_G2C_JoinBattle> 
    {
        private JoinBattle() : base() { }
        public  static JoinBattle CreateQuery(){ return new JoinBattle();}
    }
    

    public interface IGateServerTask
    {
        [API(10049)]Task_G2C_JoinBattle JoinBattle(Task_G2C_JoinBattle req);
        [API(10048)]Task_G2C_SyncHero SyncHero(Task_G2C_SyncHero req);
        [API(10047)]Task_G2C_SyncPackage SyncPackage(Task_G2C_SyncPackage req);

    }
   

    public abstract class GateServerTask
    {
        [API(10049)]public abstract Task<Task_G2C_JoinBattle> JoinBattle(Task_G2C_JoinBattle request);
        [API(10048)]public abstract Task<Task_G2C_SyncHero> SyncHero(Task_G2C_SyncHero request);
        [API(10047)]public abstract Task<Task_G2C_SyncPackage> SyncPackage(Task_G2C_SyncPackage request);

    }

}