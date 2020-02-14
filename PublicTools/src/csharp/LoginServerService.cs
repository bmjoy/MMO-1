
using Proto;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Proto.PServices;
using System.Threading.Tasks;
namespace Proto.LoginServerService
{

    /// <summary>
    /// 10032
    /// </summary>    
    [API(10032)]
    public class Login:APIBase<C2L_Login, L2C_Login> 
    {
        private Login() : base() { }
        public  static Login CreateQuery(){ return new Login();}
    }
    

    /// <summary>
    /// 10033
    /// </summary>    
    [API(10033)]
    public class Reg:APIBase<C2L_Reg, L2C_Reg> 
    {
        private Reg() : base() { }
        public  static Reg CreateQuery(){ return new Reg();}
    }
    

    public interface ILoginServerService
    {
        [API(10033)]L2C_Reg Reg(C2L_Reg req);
        [API(10032)]L2C_Login Login(C2L_Login req);

    }
   

    public abstract class LoginServerService
    {
        [API(10033)]public abstract Task<L2C_Reg> Reg(C2L_Reg request);
        [API(10032)]public abstract Task<L2C_Login> Login(C2L_Login request);

    }

}