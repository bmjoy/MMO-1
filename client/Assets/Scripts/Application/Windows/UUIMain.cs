using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UGameTools;

namespace Windows
{
    partial class UUIMain
    {

        protected override void InitModel()
        {
            base.InitModel();
            MenuMission.onClick.AddListener(() =>
            {
                var ui = UUIManager.Singleton.CreateWindow<UUILevelList>();
                ui.ShowWindow();
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
                UApplication.S.GotoLoginGate();
            });

            MenuWeapon.onClick.AddListener(() =>
            {
                UUIManager.S.CreateWindow<UUIHeroEquip>().ShowWindow();
            });

            var swipeEv = swip.GetComponent<UIEventSwipe>();
            swipeEv.OnSwiping.AddListener((v) =>
            {
                v *= .5f;
                ThridPersionCameraContollor.Current.RotationX(v.y);
                var gate = UApplication.G<GMainGate>();
                gate.RotationHero(v.x);
                //.RotationY(v.x);
            });

            //Write Code here
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

            lb_gold.text = gate.Coin.ToString("N0");
            lb_gem.text = gate.Gold.ToString("N0");
            if (gate.hero == null) return;
            this.Level_Number.text = $"{gate.hero.Level}";
            this.Username.text = $"{gate.hero.Name}";

            var leveUp = ExcelConfig.ExcelToJSONConfigManager.Current.FirstConfig<EConfig.CharacterLevelUpData>(t => t.Level == gate.hero.Level);

            lb_exp.text = $"{gate.hero.Exprices}/{leveUp?.NeedExprices ?? '-'}";
            float v = 0;
            if(leveUp!=null)
            v = gate.hero.Exprices / leveUp.NeedExprices;
            ExpSilder.value = v;
        }
    }
}