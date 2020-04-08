using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UGameTools;
using Proto.GateServerService;
using EConfig;
using ExcelConfig;

namespace Windows
{
    partial class UUIMain
    {

        protected override void InitModel()
        {
            base.InitModel();
            MenuSkill.onClick.AddListener(() =>
            {
                
            });

            Button_Play.onClick.AddListener(() =>
            {
                UUIManager.Singleton
                .CreateWindowAsync<UUILevelList>((ui) => ui.ShowWindow() );
            });

            MenuItems.onClick.AddListener(() =>
                {
                    UUIManager.S.CreateWindowAsync<UUIPackage>((ui)=>ui.ShowWindow());
                });

            MenuSetting.onClick.AddListener(() =>
            {
                UUIManager.S.CreateWindowAsync<UUISettings>((ui) => ui.ShowWindow());
            });

            MenuWeapon.onClick.AddListener(() =>
            {
                OpenEquip();
            });

            MenuShop.onClick.AddListener(() =>
            {

                UUIManager.S.CreateWindowAsync<UUIItemShop>((ui) => ui.ShowWindow());

            });
            MenuSkill.onClick.AddListener(() => {
                UUIManager.S.CreateWindowAsync<UUIMagic>(ui =>
                {
                    ui.ShowWindow();
                });
            });
            MenuMessages.onClick.AddListener(() => {
                UUIManager.S.CreateWindowAsync<UUIMessages>(ui => ui.ShowWindow());
            });

            MenuRefresh.onClick.AddListener(() => { 
            
            });

            user_info.onClick.AddListener(() => { OpenEquip(); });

            var swipeEv = swip.GetComponent<UIEventSwipe>();
            swipeEv.OnSwiping.AddListener((v) =>
            {
                //v *= .5f;
                //ThridPersionCameraContollor.Current.RotationX(v.y);
                var gate = UApplication.G<GMainGate>();
                gate.RotationHero(v.x);
                //.RotationY(v.x);
            });

            btn_goldadd.onClick.AddListener(() =>
            {
                UUIManager.S.CreateWindowAsync<UUIShopGold>(ui => ui.ShowWindow());
            });

            //Write Code here
        }

        private void OpenEquip()
        {
            UUIManager.S.CreateWindowAsync<UUIHeroEquip>((ui) => ui.ShowWindow());
        }


        protected override void OnShow()
        {
            base.OnShow();
            this.Username.text = string.Empty;
            MenuSetting.SetKey("UI_MAIN_SETTING");
            MenuItems.SetKey("UI_MAIN_ITEM");
            MenuWeapon.SetKey("UI_MAIN_WEAPON");
            MenuSkill.SetKey("UI_MAIN_SKILL");
            MenuShop.SetKey("UI_MAIN_SHOP");
            MenuMessages.SetKey("UI_MAIN_MESSAGE");
            Button_Play.SetKey("UI_MAIN_PLAY");
            MenuRefresh.SetKey("UI_MAIN_Refresh");
            OnUpdateUIData();
        }
        protected override void OnHide()
        {
            base.OnHide();
        }

        protected override void OnUpdateUIData()
        {
            base.OnUpdateUIData();
            var gate = UApplication.G<GMainGate>();
            if (gate.hero == null) return;
            user_defalut.texture = gate.LookAtView;
            lb_gold.text = gate.Gold.ToString("N0");
            lb_gem.text = gate.Coin.ToString("N0");
            if (gate.hero == null) return;
            this.Level_Number.text = $"{gate.hero.Level}";
            this.Username.text = $"{gate.hero.Name}";
            var leveUp = ExcelToJSONConfigManager.Current
                .FirstConfig<CharacterLevelUpData>(t => t.Level == gate.hero.Level+1);
            lb_exp.text = $"{gate.hero.Exprices}/{leveUp?.NeedExprices ?? '-'}";
            float v = 0;
            if (leveUp != null)  v = (float)gate.hero.Exprices / leveUp.NeedExprices;
            ExpSilder.value = v;
        }
    }
}