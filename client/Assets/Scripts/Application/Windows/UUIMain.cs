using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UGameTools;
using Proto.GateServerService;

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
                .CreateWindow<UUILevelList>().ShowWindow();
            });

            MenuItems.onClick.AddListener(() =>
                {
                    UUIManager.S.CreateWindow<UUIPackage>().ShowWindow();
                });

            MenuSetting.onClick.AddListener(() =>
            {
                UUIManager.S.CreateWindow<UUISettings>().ShowWindow();
            });

            MenuWeapon.onClick.AddListener(() =>
            {
                OpenEquip();
            });

            MenuShop.onClick.AddListener(() =>
            {
                var ui = UUIManager.S.CreateWindow<UUIItemShop>();
                ui.ShowWindow();

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

            //Write Code here
        }

        private void OpenEquip()
        {
            UUIManager.S.CreateWindow<UUIHeroEquip>().ShowWindow();
        }

        protected override void OnShow()
        {
            base.OnShow();
            this.Username.text = string.Empty;
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

            lb_gold.text = gate.Gold.ToString("N0");
            lb_gem.text = gate.Coin.ToString("N0");
            if (gate.hero == null) return;
            this.Level_Number.text = $"{gate.hero.Level}";
            this.Username.text = $"{gate.hero.Name}";
            var leveUp = ExcelConfig.ExcelToJSONConfigManager.Current.FirstConfig<EConfig.CharacterLevelUpData>(t => t.Level == gate.hero.Level);
            lb_exp.text = $"{gate.hero.Exprices}/{leveUp?.NeedExprices ?? '-'}";
            float v = 0;
            if (leveUp != null)
                v = (float)gate.hero.Exprices / leveUp.NeedExprices;
            ExpSilder.size = v;
        }
    }
}