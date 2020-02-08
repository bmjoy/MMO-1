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
    partial class UUIDetail
    {

        protected override void InitModel()
        {
            base.InitModel();
            bt_cancel.onClick.AddListener(() =>
                {
                    HideWindow();
                });
            bt_sale.onClick.AddListener(() =>
                {
                    this.HideWindow();
                    var ui = UUIManager.S.CreateWindow<UUISaleItem>();
                    ui.Show(this.item);
                    //show sale ui
                });
            bt_equip.onClick.AddListener(() =>
            {
                var equip = ExcelToJSONConfigManager.Current.GetConfigByID<EquipmentData>(int.Parse(config.Params[0]));
                if (equip == null) return;
                OperatorEquip.CreateQuery()
                .SendRequest(UApplication.G<GMainGate>().Client,
                new C2G_OperatorEquip
                {
                    IsWear = true,
                    Guid = item.GUID,
                    Part = (EquipmentType)equip.PartType
                },
                (r) =>
                {
                    if (r.Code.IsOk())
                    {
                        UApplication.S.ShowNotify($"成功装备{equip.Name}");
                        HideWindow();
                    }
                    else {
                        UApplication.S.ShowError(r.Code);
                    }
                });
            });
            //Write Code here
        }



        protected override void OnShow()
        {
            base.OnShow();

            config = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(item.ItemID);
            t_num.text = item.Num > 1 ? item.Num.ToString() : string.Empty;
            t_descript.text = config.Description;
            t_name.text = config.Name;
            t_prices.text = "售价 " + config.SalePrice;
            Icon.texture = ResourcesManager.S.LoadResources<Texture2D>("Icon/" + config.Icon);

            var g = UApplication.G<GMainGate>();
            var wear = false;
            foreach (var i in g.hero.Equips)
                if (i.GUID == item.GUID) {
                    wear = true;
                    break;
                }
            
            bt_equip.ActiveSelfObject(!wear && (ItemType)config.ItemType == ItemType.ItEquip);
            bt_sale.ActiveSelfObject(!wear);
        }

        private ItemData config;

        protected override void OnHide()
        {
            base.OnHide();
        }

        private PlayerItem item;

        public void Show(PlayerItem item)
        {
            this.item = item;
            this.ShowWindow();
        }
    }
}