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


        protected Text lb_coin;
        protected Text lb_gold;
        protected Button bt_package;
        protected Button bt_fight;
        protected Button bt_equip;
        protected Button bt_magic;
        protected Button bt_close;
        protected Text HeroName;




        protected override void InitTemplate()
        {
            base.InitTemplate();
            lb_coin = FindChild<Text>("lb_coin");
            lb_gold = FindChild<Text>("lb_gold");
            bt_package = FindChild<Button>("bt_package");
            bt_fight = FindChild<Button>("bt_fight");
            bt_equip = FindChild<Button>("bt_equip");
            bt_magic = FindChild<Button>("bt_magic");
            bt_close = FindChild<Button>("bt_close");
            HeroName = FindChild<Text>("HeroName");


        }
    }
}