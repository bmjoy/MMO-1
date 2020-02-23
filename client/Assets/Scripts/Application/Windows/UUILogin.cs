using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UGameTools;
using Proto;
using Proto.LoginServerService;

namespace Windows
{
    partial class UUILogin
    {

        protected override void InitModel()
        {
            base.InitModel();
            //Write Code here
            bt_submit.onClick.AddListener(() =>
                {
                    var userName = if_userName.text;
                    var pwd = if_pwd.text;
                    var gate = UApplication.G< LoginGate>();
                    if (gate == null) return;
                    Login.CreateQuery()
                    .SendRequest(gate.Client,
                    new C2L_Login { Password = pwd, UserName = userName, Version = MessageTypeIndexs.Version },
                    r =>
                    {
                        if (r.Code.IsOk())
                        {
                            UApplication.Singleton.GoServerMainGate(r.GateServer, r.UserID, r.Session);
                        }
                        else
                        {
                            UApplication.Singleton.ShowError(r.Code);
                        }
                    }
                    );
                });
            bt_reg.onClick.AddListener(() =>
                {
                    var userName = if_userName.text;
                    var pwd = if_pwd.text;
                    var gate = UApplication.G<LoginGate>();
                    Reg.CreateQuery()
                      .SendRequest(gate.Client,
                      new C2L_Reg
                      {
                          Password = pwd,
                          UserName = userName,
                          Version = 1
                      },
                      r =>
                      {
                          if (r.Code == ErrorCode.Ok)
                          {
                              UApplication.Singleton.GoServerMainGate(r.GateServer, r.UserID, r.Session);
                          }
                          else
                          {
                              UUITipDrawer.Singleton.ShowNotify("Server Response:" + r.Code);
                          }
                      });

                });
        }

        protected override void OnShow()
        {
            base.OnShow();
        }

        protected override void OnHide()
        {
            base.OnHide();

        }
    }
}