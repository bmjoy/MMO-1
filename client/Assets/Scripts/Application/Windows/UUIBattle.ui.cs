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
            public Image Icon;
            public Text CDTime;
            public Image ICdMask;

            public override void InitTemplate()
            {
                Button = FindChild<Button>("Button");
                Icon = FindChild<Image>("Icon");
                CDTime = FindChild<Text>("CDTime");
                ICdMask = FindChild<Image>("ICdMask");

            }
        }


        protected Scrollbar ExpSilder;
        protected Text lb_exp;
        protected Button user_info;
        protected Text Level_Number;
        protected Text Username;
        protected RoundGridLayout Grid;
        protected Text Time;
        protected Button bt_Exit;
        protected Image Joystick_Left;
        protected Image swipe;
        protected Button bt_normal_att;
        protected Image att_Icon;
        protected Button bt_hp;
        protected Image hp_item_Icon;
        protected Text hp_num;
        protected Button bt_mp;
        protected Image mp_item_Icon;
        protected Text mp_num;
        protected RawImage MapTexture;
        protected RectTransform ViewForward;


        protected UITableManager<AutoGenTableItem<GridTableTemplate, GridTableModel>> GridTableManager = new UITableManager<AutoGenTableItem<GridTableTemplate, GridTableModel>>();


        protected override void InitTemplate()
        {
            base.InitTemplate();
            ExpSilder = FindChild<Scrollbar>("ExpSilder");
            lb_exp = FindChild<Text>("lb_exp");
            user_info = FindChild<Button>("user_info");
            Level_Number = FindChild<Text>("Level_Number");
            Username = FindChild<Text>("Username");
            Grid = FindChild<RoundGridLayout>("Grid");
            Time = FindChild<Text>("Time");
            bt_Exit = FindChild<Button>("bt_Exit");
            Joystick_Left = FindChild<Image>("Joystick_Left");
            swipe = FindChild<Image>("swipe");
            bt_normal_att = FindChild<Button>("bt_normal_att");
            att_Icon = FindChild<Image>("att_Icon");
            bt_hp = FindChild<Button>("bt_hp");
            hp_item_Icon = FindChild<Image>("hp_item_Icon");
            hp_num = FindChild<Text>("hp_num");
            bt_mp = FindChild<Button>("bt_mp");
            mp_item_Icon = FindChild<Image>("mp_item_Icon");
            mp_num = FindChild<Text>("mp_num");
            MapTexture = FindChild<RawImage>("MapTexture");
            ViewForward = FindChild<RectTransform>("ViewForward");

            GridTableManager.InitFromLayout(Grid);

        }
    }
}