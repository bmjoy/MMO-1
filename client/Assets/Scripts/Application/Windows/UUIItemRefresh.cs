using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UGameTools;
using Proto;

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
            UUIManager.S.CreateWindowAsync<UUISelectItem>(ui =>
            {
                ui.ShowSelect(1, false);
                ui.OnSelectedItems = OnSelectRefresh;
            } );
        }

        private void OnSelectRefresh(List<PlayerItem> obj)
        {
             
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