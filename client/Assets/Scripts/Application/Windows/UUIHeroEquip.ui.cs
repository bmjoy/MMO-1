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
    [UIResources("UUIHeroEquip")]
    partial class UUIHeroEquip : UUIAutoGenWindow
    {
        public class PropertyListTableTemplate : TableItemTemplate
        {
            public PropertyListTableTemplate(){}

            public override void InitTemplate()
            {

            }
        }
        public class EquipmentPropertyTableTemplate : TableItemTemplate
        {
            public EquipmentPropertyTableTemplate(){}

            public override void InitTemplate()
            {

            }
        }


        protected Button equip_head;
        protected Text head_Lvl;
        protected Button equip_weapon;
        protected Text weapon_Lvl;
        protected Button equip_cloth;
        protected Text cloth_Lvl ;
        protected Button equip_shose;
        protected Text shose_Lvl;
        protected Text Level;
        protected GridLayoutGroup PropertyList;
        protected Image EquipRight;
        protected Text equip_lvl;
        protected GridLayoutGroup EquipmentProperty;
        protected Text lb_pro;
        protected Image gold_icon;
        protected Image coin_icon;
        protected Button bt_Exit;
        protected RectTransform Text;


        protected UITableManager<AutoGenTableItem<PropertyListTableTemplate, PropertyListTableModel>> PropertyListTableManager = new UITableManager<AutoGenTableItem<PropertyListTableTemplate, PropertyListTableModel>>();
        protected UITableManager<AutoGenTableItem<EquipmentPropertyTableTemplate, EquipmentPropertyTableModel>> EquipmentPropertyTableManager = new UITableManager<AutoGenTableItem<EquipmentPropertyTableTemplate, EquipmentPropertyTableModel>>();


        protected override void InitTemplate()
        {
            base.InitTemplate();
            equip_head = FindChild<Button>("equip_head");
            head_Lvl = FindChild<Text>("head_Lvl");
            equip_weapon = FindChild<Button>("equip_weapon");
            weapon_Lvl = FindChild<Text>("weapon_Lvl");
            equip_cloth = FindChild<Button>("equip_cloth");
            cloth_Lvl  = FindChild<Text>("cloth_Lvl ");
            equip_shose = FindChild<Button>("equip_shose");
            shose_Lvl = FindChild<Text>("shose_Lvl");
            Level = FindChild<Text>("Level");
            PropertyList = FindChild<GridLayoutGroup>("PropertyList");
            EquipRight = FindChild<Image>("EquipRight");
            equip_lvl = FindChild<Text>("equip_lvl");
            EquipmentProperty = FindChild<GridLayoutGroup>("EquipmentProperty");
            lb_pro = FindChild<Text>("lb_pro");
            gold_icon = FindChild<Image>("gold_icon");
            coin_icon = FindChild<Image>("coin_icon");
            bt_Exit = FindChild<Button>("bt_Exit");
            Text = FindChild<RectTransform>("Text");

            PropertyListTableManager.InitFromGrid(PropertyList);
            EquipmentPropertyTableManager.InitFromGrid(EquipmentProperty);

        }
    }
}