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
    [UIResources("UUISettings")]
    partial class UUISettings : UUIAutoGenWindow
    {


        protected Button ButtonClose;
        protected Image ButtonLanguage;
        protected Image ButtonExit;




        protected override void InitTemplate()
        {
            base.InitTemplate();
            ButtonClose = FindChild<Button>("ButtonClose");
            ButtonLanguage = FindChild<Image>("ButtonLanguage");
            ButtonExit = FindChild<Image>("ButtonExit");


        }
    }
}