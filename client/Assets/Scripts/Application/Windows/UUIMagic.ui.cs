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
    [UIResources("UUIMagic")]
    partial class UUIMagic : UUIAutoGenWindow
    {
        public class ContentTableTemplate : TableItemTemplate
        {
            public ContentTableTemplate(){}
            public Image Pic;
            public Image Icon;
            public Text lb_name;
            public Text lb_Level;

            public override void InitTemplate()
            {
                Pic = FindChild<Image>("Pic");
                Icon = FindChild<Image>("Icon");
                lb_name = FindChild<Text>("lb_name");
                lb_Level = FindChild<Text>("lb_Level");

            }
        }


        protected Button ButtonClose;
        protected VerticalLayoutGroup Content;
        protected Image Desc_Root;
        protected Text des_Text;


        protected UITableManager<AutoGenTableItem<ContentTableTemplate, ContentTableModel>> ContentTableManager = new UITableManager<AutoGenTableItem<ContentTableTemplate, ContentTableModel>>();


        protected override void InitTemplate()
        {
            base.InitTemplate();
            ButtonClose = FindChild<Button>("ButtonClose");
            Content = FindChild<VerticalLayoutGroup>("Content");
            Desc_Root = FindChild<Image>("Desc_Root");
            des_Text = FindChild<Text>("des_Text");

            ContentTableManager.InitFromLayout(Content);

        }
    }
}