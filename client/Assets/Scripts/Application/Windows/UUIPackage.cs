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
using Proto.GateServerService;

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

            Bt_Buy.onClick.AddListener(() =>
            {
                UUIPopup.ShowConfirm(LanguageManager.S["UI_PACKAGE_BUY_SIZE_TITLE"],
                    LanguageManager.S.Format("UI_PACKAGE_BUY_SIZE",UApplication.S.Constant.PACKAGE_BUY_COST,UApplication.S.Constant.PACKAGE_BUY_SIZE),
                    () =>
                    {
                        var gate = UApplication.G<GMainGate>();
                        BuyPackageSize.CreateQuery()
                        .SendRequest(gate.Client,
                        new C2G_BuyPackageSize { SizeCurrent = gate.package.MaxSize }, (res) =>
                        {
                            if (res.Code.IsOk())
                            {
                                gate.package.MaxSize = res.PackageCount;
                                OnUpdateUIData();
                            }
                            else
                                UApplication.S.ShowError(res.Code);
                        }, UUIManager.S);
                    });
            });
        }
        protected override void OnShow()
        {
            base.OnShow();
            lb_title.text = LanguageManager.S["UI_PACKAGE_UI_TITLE"];
            Bt_Buy.SetText(LanguageManager.S["UI_PACKAGE_UI_BUY_BT"]);
            TextSizeTitle.text = LanguageManager.S["UI_PACKAGE_UI_SIZE_TITLE"];
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

            lb_TextCountCur.text = $"{ gate.package.Items.Count}";
            lb_TextCountSize.text = $"/{gate.package.MaxSize}";
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