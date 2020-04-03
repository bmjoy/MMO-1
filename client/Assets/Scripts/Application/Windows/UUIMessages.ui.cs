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
    [UIResources("UUIMessages")]
    partial class UUIMessages : UUIAutoGenWindow
    {
        public class ContentTableTemplate : TableItemTemplate
        {
            public ContentTableTemplate(){}

            public override void InitTemplate()
            {

            }
        }


        protected Button ButtonClose;
        protected VerticalLayoutGroup Content;


        protected UITableManager<AutoGenTableItem<ContentTableTemplate, ContentTableModel>> ContentTableManager = new UITableManager<AutoGenTableItem<ContentTableTemplate, ContentTableModel>>();


        protected override void InitTemplate()
        {
            base.InitTemplate();
            ButtonClose = FindChild<Button>("ButtonClose");
            Content = FindChild<VerticalLayoutGroup>("Content");

            ContentTableManager.InitFromLayout(Content);

        }
    }
}