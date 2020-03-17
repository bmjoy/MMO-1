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

            this.ButtonBlue.onClick.AddListener(() =>
            {
                var userName = TextInputBoxUserName.text;
                var pwd = TextInputBoxPassWord.text;
                var gate = UApplication.G<LoginGate>();
                if (gate == null) return;
                if (CheckBox.isOn)
                {
                    PlayerPrefs.SetString(UserNameKey, userName);
                    PlayerPrefs.SetString(PasswordKey, pwd);
                }
                else
                {
                    PlayerPrefs.DeleteKey(UserNameKey);
                    PlayerPrefs.DeleteKey(PasswordKey);
                }
                UUIManager.S.MaskEvent();
                gate.GoLogin(userName, pwd, (r) =>
                {
                    UUIManager.S.UnMaskEvent();
                    if (r.Code.IsOk())
                    {
                        UApplication.Singleton.GoServerMainGate(r.GateServer, r.UserID, r.Session);
                    }
                    else
                    {
                        UApplication.Singleton.ShowError(r.Code);
                    }
                });
            });
            TextSignup.onClick.AddListener(() =>
            {
                UUIManager.S.CreateWindow<UUISignup>().ShowWindow();
            });
            ButtonClose.onClick.AddListener(() =>
            {
                //do nothing
            });

        }

        protected override void OnShow()
        {
            base.OnShow();

            TextInputBoxUserName.text = PlayerPrefs.GetString(UserNameKey);
            TextInputBoxPassWord.text = PlayerPrefs.GetString(PasswordKey);
            CheckBox.isOn = !string.IsNullOrEmpty(TextInputBoxUserName.text);
        }

        protected override void OnHide()
        {
            base.OnHide();

        }
    }
}