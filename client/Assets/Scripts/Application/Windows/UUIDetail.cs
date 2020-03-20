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
                    var ui = UUIManager.S.CreateWindow<UUISaleItem>();
                    ui.Show(this.item);
                    //show sale ui
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
                        UApplication.S.ShowNotify($"成功装备{equip.Name}");
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

            config = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(item.ItemID);
            t_num.text = $"{ item.Num}";
            t_descript.text = config.Description;
            t_name.text = config.Name;
            t_prices.text = $"售价:{ config.SalePrice}";
            icon.sprite = ResourcesManager.S.LoadIcon(config);

            ItemLevel.ActiveSelfObject(item.Level > 0);
            lb_level.text = $"{item.Level}";
            ItemCount.ActiveSelfObject(item.Num > 1);
            Locked.ActiveSelfObject(item.Locked);
            
            var g = UApplication.G<GMainGate>();
            var wear = false;
            foreach (var i in g.hero.Equips)
                if (i.GUID == item.GUID) {
                    wear = true;
                    break;
                }
            WearOn.ActiveSelfObject(wear);

            bt_equip.ActiveSelfObject(!wear && (ItemType)config.ItemType == ItemType.ItEquip);
            bt_sale.ActiveSelfObject(!wear);


            if ((ItemType)config.ItemType == ItemType.ItEquip)
            {
                var eq = ExcelToJSONConfigManager.Current.GetConfigByID<EquipmentData>(int.Parse(config.Params[0]));
                ShowEquip(config, eq, item.Level);
            }
            else {
                EquipmentPropertyTableManager.Count = 0;
            }
        }


        private void ShowEquip(ItemData config, EquipmentData equip,int lvl)
        {
            var level = ExcelToJSONConfigManager.Current
                        .FirstConfig<EquipmentLevelUpData>(t => t.Level == lvl && t.Quality == config.Quality);
            var pro = equip.Properties.SplitToInt();
            var val = equip.PropertyValues.SplitToInt();

            var names = new Dictionary<P, string>() {
                { P.Agility, "敏捷:{0}" },
                { P.Crt,"暴击:{0}"},
                { P.DamageMax,"攻击上限:{0}"},
                { P.DamageMin,"攻击下限:{0}"},
                { P.Defance,"防御:{0}"},
                { P.Force,"力量:{0}"},
                //{ P.Hit,"命中:{0}"},
                {P.Jouk,"闪避:{0}" },
                {P.Knowledge,"智力:{0}" },
                { P.MagicWaitTime,"攻击速度:{0}"},
                { P.MaxHp,"HP:{0}"},
                { P.MaxMp,"MP:{0}"},
                {P.Resistibility,"魔法闪避:{0}" },
                {P.SuckingRate,"吸血等级:{0}"}
            };

            //var level = 
            var properties = new Dictionary<P, ComplexValue>();
            for (var ip = 0; ip < pro.Count; ip++)
            {
                var pr = (P)pro[ip];
                if (!properties.ContainsKey(pr))
                    properties.Add(pr, 0);

                if (properties.TryGetValue(pr, out ComplexValue value))
                {
                    //计算装备加成
                    var eVal = value.AppendValue + (1 + ((level?.AppendRate ?? 0) / 10000f)) * val[ip];
                    value.SetAppendValue((int)eVal);
                }
            }

            EquipmentPropertyTableManager.Count = properties.Count;
            int index = 0;
            foreach (var i in properties)
            {
                if (names.TryGetValue(i.Key, out string format))
                {
                    EquipmentPropertyTableManager[index]
                        .Model.SetLabel(string.Format(format, i.Value.FinalValue));
                }
                index++;
            }
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