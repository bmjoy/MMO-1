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
    [UIResources("UUIMain")]
    partial class UUIMain : UUIAutoGenWindow
    {


        protected Button MenuSetting;
        protected Button MenuWeapon;
        protected Button MenuMission;
        protected Image mission_notification;
        protected Button MenuMessages;
        protected Image message_notification;
        protected Text MessagCountText;
        protected Button MenuShop;
        protected Image item_notification;
        protected Button MenuItems;
        protected Slider ExpSilder;
        protected Text lb_exp;
        protected Text Level_Number;
        protected Text Username;
        protected Text lb_gold;
        protected Button btn_goldadd;
        protected Text lb_gem;
        protected Image btn_addgem;
        protected Button Button_AddFriend;
        protected Button Button_Facebook;
        protected Image swip;




        protected override void InitTemplate()
        {
            base.InitTemplate();
            MenuSetting = FindChild<Button>("MenuSetting");
            MenuWeapon = FindChild<Button>("MenuWeapon");
            MenuMission = FindChild<Button>("MenuMission");
            mission_notification = FindChild<Image>("mission_notification");
            MenuMessages = FindChild<Button>("MenuMessages");
            message_notification = FindChild<Image>("message_notification");
            MessagCountText = FindChild<Text>("MessagCountText");
            MenuShop = FindChild<Button>("MenuShop");
            item_notification = FindChild<Image>("item_notification");
            MenuItems = FindChild<Button>("MenuItems");
            ExpSilder = FindChild<Slider>("ExpSilder");
            lb_exp = FindChild<Text>("lb_exp");
            Level_Number = FindChild<Text>("Level_Number");
            Username = FindChild<Text>("Username");
            lb_gold = FindChild<Text>("lb_gold");
            btn_goldadd = FindChild<Button>("btn_goldadd");
            lb_gem = FindChild<Text>("lb_gem");
            btn_addgem = FindChild<Image>("btn_addgem");
            Button_AddFriend = FindChild<Button>("Button_AddFriend");
            Button_Facebook = FindChild<Button>("Button_Facebook");
            swip = FindChild<Image>("swip");


        }
    }
}