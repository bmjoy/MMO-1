using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UGameTools;

namespace Windows
{
    partial class UUIItemRefresh
    {
        public class PropertyListTableModel : TableItemModel<PropertyListTableTemplate>
        {
            public PropertyListTableModel(){}
            public override void InitModel()
            {
                //todo
            }
        }
        public class ItemListTableModel : TableItemModel<ItemListTableTemplate>
        {
            public ItemListTableModel(){}
            public override void InitModel()
            {
                //todo
            }
        }
        public class EquipmentPropertyTableModel : TableItemModel<EquipmentPropertyTableTemplate>
        {
            public EquipmentPropertyTableModel(){}
            public override void InitModel()
            {
                //todo
            }
        }

        protected override void InitModel()
        {
            base.InitModel();
            CloseButton.onClick.AddListener(() => HideWindow());
            equipRefresh.onClick.AddListener(() => SelectedItem());
        }


        private void SelectedItem()
        {
            //UUIManager.S.CreateWindowAsync<UUISelectEquip>(ui => ui.ShowWindow());
        }

        protected override void OnShow()
        {
            base.OnShow();
            Right.ActiveSelfObject(false);
        }
        protected override void OnHide()
        {
            base.OnHide();
        }
    }
}