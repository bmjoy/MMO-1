using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UGameTools;
using Proto;
using EConfig;
using ExcelConfig;
using Proto.GateServerService;

namespace Windows
{
    partial class UUISelectEquip
    {


        public class ContentTableModel : TableItemModel<ContentTableTemplate>
        {
            public ContentTableModel() { }

            public Action<ContentTableModel> OnWearClick { get; set; }

            public override void InitModel()
            {
                Template.bt_equip.onClick.AddListener(() => {
                    this.OnWearClick?.Invoke(this);
                });
            }

            public EquipmentData Equip;
            public PlayerItem IItem;

            internal void SetItem(PlayerItem playerItem)
            {
                this.IItem = playerItem;
                var item = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(playerItem.ItemID);
                Equip = ExcelToJSONConfigManager.Current.GetConfigByID<EquipmentData>(int.Parse(item.Params[0]));

                this.Template.level.text = playerItem.Level > 0 ? $"+{ playerItem.Level}" : string.Empty;
                this.Template.t_name.text = $"{item.Name}";
                this.Template.lb_qulity.text = $"品质 {Equip.Quality}";
                this.Template.Icon.texture = ResourcesManager.S.LoadIcon(item);
            }
        }

        protected override void InitModel()
        {
            base.InitModel();
            this.bt_cancel.onClick.AddListener(() => { HideWindow(); });
            //Write Code here
        }
        protected override void OnShow()
        {
            base.OnShow();
            if (!part.HasValue) { HideWindow(); }
            else
                ShowEquipList();
        }

        protected override void OnHide()
        {
            base.OnHide();
        }




        private void ShowEquipList()
        {
            var equip = new List<PlayerItem>();
            var g = UApplication.G<GMainGate>();
            foreach (var i in g.package.Items)
            {
                var item = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(i.Value.ItemID);
                if ((ItemType)item.ItemType != ItemType.ItEquip) continue;
                var wear = false;
                foreach (var e in g.hero.Equips)
                {
                    if (e.GUID == i.Key) {
                        wear = true;
                        break;
                    }
                }
                if (wear) continue ;
                var ec = ExcelToJSONConfigManager.Current.GetConfigByID<EquipmentData>(int.Parse(item.Params[0]));
                if ((EquipmentType)ec.PartType != part) continue;

                equip.Add(i.Value);

            }

            this.ContentTableManager.Count = equip.Count;
            int index = 0;
            foreach (var i in ContentTableManager)
            {
                i.Model.SetItem(equip[index]);
                i.Model.OnWearClick = WearClick;
                index++;
            }
        }

        private void WearClick(ContentTableModel obj)
        {
            var g = UApplication.G<GMainGate>();
            OperatorEquip.CreateQuery().SendRequest(g.Client, new C2G_OperatorEquip
            {
                Guid = obj.IItem.GUID,
                IsWear = true,
                Part = (EquipmentType)obj.Equip.PartType
            },
                (r) =>
                {
                    if (!r.Code.IsOk())
                    {
                        UApplication.S.ShowError(r.Code);
                    }
                });
            HideWindow();
        }

        private  EquipmentType? part;

        public UUISelectEquip SetPartType(EquipmentType type)
        {
            this.part = type;
            return this;
        }
    }
}