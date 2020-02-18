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
    [UIResources("UUIBattle")]
    partial class UUIBattle : UUIAutoGenWindow
    {
        public class GridTableTemplate : TableItemTemplate
        {
            public GridTableTemplate(){}
            public Button Button;
            public Text Cost;
            public Image ICdMask;

            public override void InitTemplate()
            {
                Button = FindChild<Button>("Button");
                Cost = FindChild<Text>("Cost");
                ICdMask = FindChild<Image>("ICdMask");

            }
        }


        protected RoundGridLayout Grid;
        protected Text Time;
        protected Button bt_Exit;
        protected RectTransform Text;
        protected Image Joystick_Left;
        protected Image swipe;
        protected Button bt_normal_att;


        protected UITableManager<AutoGenTableItem<GridTableTemplate, GridTableModel>> GridTableManager = new UITableManager<AutoGenTableItem<GridTableTemplate, GridTableModel>>();


        protected override void InitTemplate()
        {
            base.InitTemplate();
            Grid = FindChild<RoundGridLayout>("Grid");
            Time = FindChild<Text>("Time");
            bt_Exit = FindChild<Button>("bt_Exit");
            Text = FindChild<RectTransform>("Text");
            Joystick_Left = FindChild<Image>("Joystick_Left");
            swipe = FindChild<Image>("swipe");
            bt_normal_att = FindChild<Button>("bt_normal_att");

            GridTableManager.InitFromGrid(Grid);

        }
    }
}