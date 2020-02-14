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
    [UIResources("UUIPackage")]
    partial class UUIPackage : UUIAutoGenWindow
    {
        public class ContentTableTemplate : TableItemTemplate
        {
            public ContentTableTemplate(){}
            public Button Button;
            public RawImage RawImage;
            public Text Text;
            public Image i_lock;
            public Text level;

            public override void InitTemplate()
            {
                Button = FindChild<Button>("Button");
                RawImage = FindChild<RawImage>("RawImage");
                Text = FindChild<Text>("Text");
                i_lock = FindChild<Image>("i_lock");
                level = FindChild<Text>("level");

            }
        }


        protected Text t_size;
        protected Button bt_close;
        protected GridLayoutGroup Content;


        protected UITableManager<AutoGenTableItem<ContentTableTemplate, ContentTableModel>> ContentTableManager = new UITableManager<AutoGenTableItem<ContentTableTemplate, ContentTableModel>>();


        protected override void InitTemplate()
        {
            base.InitTemplate();
            t_size = FindChild<Text>("t_size");
            bt_close = FindChild<Button>("bt_close");
            Content = FindChild<GridLayoutGroup>("Content");

            ContentTableManager.InitFromGrid(Content);

        }
    }
}