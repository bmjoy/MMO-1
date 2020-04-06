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
    [UIResources("UUIShopGold")]
    partial class UUIShopGold : UUIAutoGenWindow
    {
        public class ContentsTableTemplate : TableItemTemplate
        {
            public ContentsTableTemplate(){}
            public Button ButtonBlue;
            public Image icon;
            public Text lb_gold;

            public override void InitTemplate()
            {
                ButtonBlue = FindChild<Button>("ButtonBlue");
                icon = FindChild<Image>("icon");
                lb_gold = FindChild<Text>("lb_gold");

            }
        }


        protected Button ButtonClose;
        protected HorizontalLayoutGroup Contents;


        protected UITableManager<AutoGenTableItem<ContentsTableTemplate, ContentsTableModel>> ContentsTableManager = new UITableManager<AutoGenTableItem<ContentsTableTemplate, ContentsTableModel>>();


        protected override void InitTemplate()
        {
            base.InitTemplate();
            ButtonClose = FindChild<Button>("ButtonClose");
            Contents = FindChild<HorizontalLayoutGroup>("Contents");

            ContentsTableManager.InitFromLayout(Contents);

        }
    }
}