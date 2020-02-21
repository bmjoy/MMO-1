
using Proto;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Proto.PServices;
using System.Threading.Tasks;
namespace Proto.LoginServerGateServerTaskServices
{

    /// <summary>
    /// 1
    /// </summary>    
    [API(1)]
    public class ExitUser:APIBase<Task_L2G_ExitUser, Task_L2G_ExitUser> 
    {
        private ExitUser() : base() { }
        public  static ExitUser CreateQuery(){ return new ExitUser();}
    }
    

    public interface ILoginServerGateServerTaskServices
    {
        [API(1)]Task_L2G_ExitUser ExitUser(Task_L2G_ExitUser req);

    }
   

    public abstract class LoginServerGateServerTaskServices
    {
        [API(1)]public abstract Task<Task_L2G_ExitUser> ExitUser(Task_L2G_ExitUser request);

    }

}