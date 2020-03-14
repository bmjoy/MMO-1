using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UGameTools;
using UnityEngine.UI;
//AUTO GenCode Don't edit it.
namespace Windows
{
    [UIResources("UUILogin")]
    partial class UUILogin : UUIAutoGenWindow
    {


        protected Button ButtonClose;
        protected Button TextSignup;
        protected Button ButtonBlue;
        protected InputField TextInputBoxUserName;
        protected InputField TextInputBoxPassWord;
        protected Toggle CheckBox;




        protected override void InitTemplate()
        {
            base.InitTemplate();
            ButtonClose = FindChild<Button>("ButtonClose");
            TextSignup = FindChild<Button>("TextSignup");
            ButtonBlue = FindChild<Button>("ButtonBlue");
            TextInputBoxUserName = FindChild<InputField>("TextInputBoxUserName");
            TextInputBoxPassWord = FindChild<InputField>("TextInputBoxPassWord");
            CheckBox = FindChild<Toggle>("CheckBox");


        }
    }
}