using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UGameTools;
using Proto;
using EConfig;
using ExcelConfig;

namespace Windows
{
    partial class UUISelectItem
    {

        public class DisplayItemData 
        {
            public PlayerItem Item;
            public ItemData Config;
        }
        public class ContentTableModel : TableItemModel<ContentTableTemplate>
        {
            public ContentTableModel(){}
            public override void InitModel()
            {
                Template.ItemBg.onClick.AddListener(() => OnClickItem?.Invoke(this));
            }
            public Action<ContentTableModel> OnClickItem;
            public ItemData Config;
            public PlayerItem pItem;
            public void SetItem(PlayerItem item, bool isWear)
            {
                var itemconfig = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(item.ItemID);
                Config = itemconfig;
                pItem = item;
                Template.ItemCount.ActiveSelfObject(item.Num > 1);
                Template.lb_count.text = item.Num > 1 ? item.Num.ToString() : string.Empty;
                ResourcesManager.S.LoadIcon(itemconfig, s => Template.icon.sprite = s);
                Template.lb_level.text = item.Level > 0 ? $"+{item.Level}" : string.Empty;
                Template.ItemLevel.ActiveSelfObject(item.Level > 0);
                Template.lb_Name.text = itemconfig.Name;
                Template.Locked.ActiveSelfObject(item.Locked);
                Template.WearOn.ActiveSelfObject(isWear);
                Template.Selected.ActiveSelfObject(false);
            }

            internal void UnSelect()
            {
                Template.Selected.ActiveSelfObject(false);
            }

            internal void Uelect()
            {
                Template.Selected.ActiveSelfObject(true);
            }
        }
    
        protected override void InitModel()
        {
            base.InitModel();
            //Write Code here
        }
        protected override void OnShow()
        {
            base.OnShow();
            ContentTableManager.Count = ListItems.Count;
            int index = 0;
            foreach (var i in ContentTableManager)
            {
                i.Model.SetItem(ListItems[index].Item, false);
                i.Model.OnClickItem = ClickItem;
                index++;
            }

        }

        private void ClickItem(ContentTableModel obj)
        {
            if (selected.Contains(obj))
            {
                selected.Remove(obj);
                obj.UnSelect();
            }

            else
            {
                selected.Add(obj);
                obj.Uelect();

                if (selected.Count == needcount)
                {
                    var list = new List<PlayerItem>();
                    foreach (var i in selected)
                        list.Add(i.pItem);
                    OnSelectedItems?.Invoke(list);
                    HideWindow();
                }
            }
        }

        protected override void OnHide()
        {
            base.OnHide();
        }

        public Action<List<PlayerItem>> OnSelectedItems;

        private List<ContentTableModel> selected = new List<ContentTableModel>();

        private List<DisplayItemData> ListItems;

        private int needcount = 0;

        public void ShowSelect(int count, bool nowear, string exceptId = null)
        {
            needcount = count;
            var gata = UApplication.G<GMainGate>();
            ListItems = gata.package.Items.Where(t => t.Key != exceptId)
                .Select(t => new DisplayItemData
                {
                    Item = t.Value,
                    Config = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(t.Value.ItemID)
                }).Where(t => t.Config.ItemType == (int)ItemType.ItEquip)
                .Select(t => t).ToList();

            if (nowear)
            {
                HashSet<string> wears = new HashSet<string>();
                foreach (var i in gata.hero.Equips)
                {
                    wears.Add(i.GUID);
                }

                ListItems= ListItems.Where(t => !wears.Contains(t.Item.GUID)).ToList();

            }

            ShowWindow();
        }
        
    }
}