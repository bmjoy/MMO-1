using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UGameTools;
using Proto;
using Proto.LoginServerService;
using UnityEngine;

namespace Windows
{
    partial class UUILogin
    {

        private const string UserNameKey = "KEY_NAME";
        private const string PasswordKey = "Key_Password";

        protected override void InitModel()
        {
            base.InitModel();
            //Write Code here
            this.ButtonBlue.onClick.AddListener(() =>
                {
                    var userName = TextInputBoxUserName.text;
                    var pwd = TextInputBoxPassWord.text;
                    var gate = UApplication.G< LoginGate>();
                    if (gate == null) return;
                    if (CheckBox.isOn)
                    {
                        UnityEngine.PlayerPrefs.SetString(UserNameKey, userName);
                        UnityEngine.PlayerPrefs.SetString(PasswordKey, pwd);
                    }
                    else {
                        PlayerPrefs.DeleteKey(UserNameKey);
                        PlayerPrefs.DeleteKey(PasswordKey);
                    }

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
            TextSignup.onClick.AddListener(() =>
                {
                    var userName = TextInputBoxUserName.text;
                    var pwd = TextInputBoxPassWord.text;
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
            ButtonClose.onClick.AddListener(() => {
                //do nothing
            });
        }

        protected override void OnShow()
        {
            base.OnShow();

            TextInputBoxUserName.text = UnityEngine.PlayerPrefs.GetString(UserNameKey);
            TextInputBoxPassWord.text = UnityEngine.PlayerPrefs.GetString(PasswordKey);

            CheckBox.isOn = !string.IsNullOrEmpty(TextInputBoxUserName.text);
        }

        protected override void OnHide()
        {
            base.OnHide();

        }
    }
}