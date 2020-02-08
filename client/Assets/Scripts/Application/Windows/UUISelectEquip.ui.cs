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
    [UIResources("UUISelectEquip")]
    partial class UUISelectEquip : UUIAutoGenWindow
    {
        public class ContentTableTemplate : TableItemTemplate
        {
            public ContentTableTemplate(){}
            public RawImage Icon;
            public Text level;
            public Text t_name;
            public Button bt_equip;
            public Text lb_qulity;

            public override void InitTemplate()
            {
                Icon = FindChild<RawImage>("Icon");
                level = FindChild<Text>("level");
                t_name = FindChild<Text>("t_name");
                bt_equip = FindChild<Button>("bt_equip");
                lb_qulity = FindChild<Text>("lb_qulity");

            }
        }


        protected GridLayoutGroup Content;
        protected Button bt_cancel;


        protected UITableManager<AutoGenTableItem<ContentTableTemplate, ContentTableModel>> ContentTableManager = new UITableManager<AutoGenTableItem<ContentTableTemplate, ContentTableModel>>();


        protected override void InitTemplate()
        {
            base.InitTemplate();
            Content = FindChild<GridLayoutGroup>("Content");
            bt_cancel = FindChild<Button>("bt_cancel");

            ContentTableManager.InitFromGrid(Content);

        }
    }
}