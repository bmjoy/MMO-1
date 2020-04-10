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
using GameLogic.Game;

using P = Proto.HeroPropertyType;

namespace Windows
{
    partial class UUIDetail
    {
        public class EquipmentPropertyTableModel : TableItemModel<EquipmentPropertyTableTemplate>
        {
            public EquipmentPropertyTableModel() { }
            public override void InitModel()
            {
                //todo
            }

            internal void SetLabel(string label)
            {
                Template.lb_text.text = label;
            }
        }

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
                   UUIManager.S.CreateWindowAsync<UUISaleItem>((ui)=> {
                       ui.Show(this.item);
                   });
                });
            bt_equip.onClick.AddListener(() =>
            {
                var equip = ExcelToJSONConfigManager.Current.GetConfigByID<EquipmentData>(int.Parse(config.Params[0]));
                if (equip == null) return;
                var requ = new C2G_OperatorEquip
                {
                    IsWear = true,
                    Guid = item.GUID,
                    Part = (EquipmentType)equip.PartType
                };
                OperatorEquip.CreateQuery()
                .SendRequest(UApplication.G<GMainGate>().Client,requ,
                (r) =>
                {
                    if (r.Code.IsOk())
                    {
                        UApplication.S.ShowNotify(LanguageManager.S.Format("UUIDETAIL_WEAR_SUCESS", $"{equip.Name}"));
                        HideWindow();
                    }
                    else {
                        UApplication.S.ShowError(r.Code);
                    }
                },UUIManager.S);
            });

            this.uiRoot.transform.OnMouseClick((obj) => {
                HideWindow();
            });
            //Write Code here
        }



        protected override void OnShow()
        {
            base.OnShow();

            bt_equip.SetKey("UUIDetail_WEAR");
            bt_sale.SetKey("UUIDetail_SELL");

            config = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(item.ItemID);
            t_num.text = $"{ item.Num}";
            t_descript.text = config.Description;
            t_name.text = config.Name;
            t_prices.SetKey("UUIDetail_PRICES", $"{ config.SalePrice}") ;
            ResourcesManager.S.LoadIcon(config,s=> icon.sprite = s);

            ItemLevel.ActiveSelfObject(item.Level > 0);
            lb_level.text = $"{item.Level}";
            ItemCount.ActiveSelfObject(item.Num > 1);
            Locked.ActiveSelfObject(item.Locked);
            
           

            if (nobt)
            {
                bt_equip.ActiveSelfObject(false);
                bt_sale.ActiveSelfObject(false);
                WearOn.ActiveSelfObject(false);
            }
            else
            {
                var g = UApplication.G<GMainGate>();
                var wear = false;
                foreach (var i in g.hero.Equips)
                    if (i.GUID == item.GUID)
                    {
                        wear = true;
                        break;
                    }
                WearOn.ActiveSelfObject(wear);
                bt_equip.ActiveSelfObject(!wear && (ItemType)config.ItemType == ItemType.ItEquip);
                bt_sale.ActiveSelfObject(!wear);
            }

            if ((ItemType)config.ItemType == ItemType.ItEquip)
            {
                var eq = ExcelToJSONConfigManager.Current.GetConfigByID<EquipmentData>(int.Parse(config.Params[0]));
                ShowEquip(item, config, eq, item.Level);
            }
            else {
                EquipmentPropertyTableManager.Count = 0;
            }
        }


        private void ShowEquip(PlayerItem pItem, ItemData config, EquipmentData equip,int lvl)
        {
            var level = ExcelToJSONConfigManager.Current
                        .FirstConfig<EquipmentLevelUpData>(t => t.Level == lvl && t.Quality == config.Quality);
            var pro = equip.Properties.SplitToInt();
            var val = equip.PropertyValues.SplitToInt();
           
            var properties = new Dictionary<P, ComplexValue>();
            for (var ip = 0; ip < pro.Count; ip++)
            {
                var pr = (P)pro[ip];
                if (!properties.ContainsKey(pr)) properties.Add(pr, 0);
                if (properties.TryGetValue(pr, out ComplexValue value))
                {
                    value.SetBaseValue(value.BaseValue + val[ip]);
                    value.SetRate(level?.AppendRate ?? 0);
                }
            }
            if (pItem.Data != null)
            {
                foreach (var v in pItem.Data.Values)
                {
                    var k = (P)v.Key;
                    if (!properties.ContainsKey(k)) properties.Add(k, 0);
                    if (properties.TryGetValue(k, out ComplexValue value))
                    {
                        value.SetAppendValue(value.AppendValue + v.Value);
                    }
                }
            }

            EquipmentPropertyTableManager.Count = properties.Count;
            int index = 0;
            foreach (var i in properties)
            {
                EquipmentPropertyTableManager[index]
                    .Model
                    .SetLabel(LanguageManager.S.Format($"UUIDetail_{i.Key}", i.Value.ToString()));
                index++;
            }
        }

        private ItemData config;
        private bool nobt = false;

        protected override void OnHide()
        {
            base.OnHide();
        }

        private PlayerItem item;

        public void Show(PlayerItem item,bool nobt =false)
        {
            this.nobt = nobt;
            this.item = item;
            this.ShowWindow();
        }
    }
}