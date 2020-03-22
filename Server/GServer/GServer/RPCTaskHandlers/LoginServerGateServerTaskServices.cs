using System;
using GServer;
using Proto;
using Proto.LoginServerGateServerTaskServices;
using ServerUtility;
using XNet.Libs.Net;

namespace GateServer
{
    [TaskHandler(typeof(ILoginServerGateServerTaskServices))]
    public class LoginServerGateServerTaskServices: TaskHandler, ILoginServerGateServerTaskServices
    {
        Task_L2G_ExitUser ILoginServerGateServerTaskServices.ExitUser(Task_L2G_ExitUser req)
        {
            Client client = null;
            Application.Current.ServiceServer.CurrentConnectionManager.Each(t =>
            {

                if ((string)t.UserState == req.AccountId)
                {
                    client = t;
                    return true;
                }
                return false;
            });

            if (client != null)
                Application.Current.ServiceServer.DisConnectClient(client);
            return req;
        }
    }
}
