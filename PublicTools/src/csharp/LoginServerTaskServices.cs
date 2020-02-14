
using Proto;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Proto.PServices;
using System.Threading.Tasks;
namespace Proto.LoginServerTaskServices
{

    /// <summary>
    /// 10041
    /// </summary>    
    [API(10041)]
    public class ExitUser:APIBase<Task_L2B_ExitUser, Task_L2B_ExitUser> 
    {
        private ExitUser() : base() { }
        public  static ExitUser CreateQuery(){ return new ExitUser();}
    }
    

    public interface ILoginServerTaskServices
    {
        [API(10041)]Task_L2B_ExitUser ExitUser(Task_L2B_ExitUser req);

    }
   

    public abstract class LoginServerTaskServices
    {
        [API(10041)]public abstract Task<Task_L2B_ExitUser> ExitUser(Task_L2B_ExitUser request);

    }

}