
using Proto;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Proto.PServices;
using System.Threading.Tasks;
namespace Proto.LoginServerTaskServices
{

    /// <summary>
    /// 2
    /// </summary>    
    [API(2)]
    public class ExitUser:APIBase<Task_L2B_ExitUser, Task_L2B_ExitUser> 
    {
        private ExitUser() : base() { }
        public  static ExitUser CreateQuery(){ return new ExitUser();}
    }
    

    public interface ILoginServerTaskServices
    {
        [API(2)]Task_L2B_ExitUser ExitUser(Task_L2B_ExitUser req);

    }
   

    public abstract class LoginServerTaskServices
    {
        [API(2)]public abstract Task<Task_L2B_ExitUser> ExitUser(Task_L2B_ExitUser request);

    }

}