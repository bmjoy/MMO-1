using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UGameTools;
using Proto;
using ExcelConfig;
using UnityEngine;
using EConfig;

namespace Windows
{
    partial class UUIPackage
    {
        public class ContentTableModel : TableItemModel<ContentTableTemplate>
        {
            public ContentTableModel(){}
            public override void InitModel()
            {
                //todo
                Template.ItemBg.onClick.AddListener(
                    () =>
                    {
                        if (OnClickItem == null)
                            return;
                        OnClickItem(this);
                    });
            }
            public Action<ContentTableModel> OnClickItem;
            public ItemData Config;
            public PlayerItem pItem;
            public void SetItem(PlayerItem item,bool isWear)
            {
                var itemconfig = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(item.ItemID);
                Config = itemconfig;
                pItem = item;
                Template.ItemCount.ActiveSelfObject(item.Num > 1);
                Template.lb_count.text = item.Num>1? item.Num.ToString():string.Empty;
                ResourcesManager.S.LoadIcon(itemconfig,s=> Template.icon.sprite =s);
                Template.lb_level.text = item.Level > 0 ? $"+{item.Level}" : string.Empty;
                Template.ItemLevel.ActiveSelfObject(item.Level > 0);
                Template.lb_Name.text = itemconfig.Name;
                Template.Locked.ActiveSelfObject(item.Locked);
                Template.WearOn.ActiveSelfObject(isWear);
            }
        }

        protected override void InitModel()
        {
            base.InitModel();
            ButtonClose.onClick.AddListener(
                () =>
                {
                    HideWindow();
                });
        }
        protected override void OnShow()
        {
            base.OnShow();
            OnUpdateUIData();

        }
        protected override void OnHide()
        {
            base.OnHide();
        }

        protected override void OnUpdateUIData()
        {
            base.OnUpdateUIData();
            var gate = UApplication.G<GMainGate>();

            ContentTableManager.Count = gate.package.Items.Count;
            var hero = gate.hero;
            int index = 0;
            foreach (var item in gate.package.Items)
            {
                var i = ContentTableManager[index];
                i.Model.SetItem(item.Value,IsWear(item.Key,hero));
                i.Model.OnClickItem = ClickItem;
                index++;
            }
            //t_size.text = string.Format("{0}/{1}", gate.package.Items.Count, gate.package.MaxSize);
        }

        private bool IsWear(string guuid, DHero hero)
        {
            foreach (var i in hero.Equips)
                if (i.GUID == guuid) return true;
            return false;
        }

        private void ClickItem(ContentTableModel item)
        {
            UUIManager.S.CreateWindowAsync<UUIDetail>(ui => ui.Show(item.pItem));
            
        }
    }
}