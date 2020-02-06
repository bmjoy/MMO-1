
using Proto;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Proto.PServices;
using System.Threading.Tasks;
namespace Proto.LoginServerTaskServices
{

    /// <summary>
    /// 10039
    /// </summary>    
    [API(10039)]
    public class ExitUser:APIBase<Task_L2B_ExitUser, Task_L2B_ExitUser> 
    {
        private ExitUser() : base() { }
        public  static ExitUser CreateQuery(){ return new ExitUser();}
    }
    

    public interface ILoginServerTaskServices
    {
        [API(10039)]Task_L2B_ExitUser ExitUser(Task_L2B_ExitUser req);

    }
   

    public abstract class LoginServerTaskServices
    {
        [API(10039)]public abstract Task<Task_L2B_ExitUser> ExitUser(Task_L2B_ExitUser request);

    }

}