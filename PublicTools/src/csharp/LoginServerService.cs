
using Proto;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Proto.PServices;
using System.Threading.Tasks;
namespace Proto.LoginServerService
{

    /// <summary>
    /// 1
    /// </summary>    
    [API(1)]
    public class Login:APIBase<C2L_Login, L2C_Login> 
    {
        private Login() : base() { }
        public  static Login CreateQuery(){ return new Login();}
    }
    

    /// <summary>
    /// 2
    /// </summary>    
    [API(2)]
    public class Reg:APIBase<C2L_Reg, L2C_Reg> 
    {
        private Reg() : base() { }
        public  static Reg CreateQuery(){ return new Reg();}
    }
    

    public interface ILoginServerService
    {
        [API(2)]L2C_Reg Reg(C2L_Reg req);
        [API(1)]L2C_Login Login(C2L_Login req);

    }
   

    public abstract class LoginServerService
    {
        [API(2)]public abstract Task<L2C_Reg> Reg(C2L_Reg request);
        [API(1)]public abstract Task<L2C_Login> Login(C2L_Login request);

    }

}