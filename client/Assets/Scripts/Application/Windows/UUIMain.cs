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
            bt_fight.onClick.AddListener(() =>
                {
                    var ui = UUIManager.Singleton.CreateWindow<Windows.UUILevelList>();
                    ui.ShowWindow();
                    //UAppliaction.Singleton.GoToGameBattleGate(1);
                });

            bt_package.onClick.AddListener(() =>
                {
                    UUIManager.S.CreateWindow<UUIPackage>().ShowWindow();
                });

            bt_close.onClick.AddListener(() => {
                UApplication.S.GotoLoginGate();
            });

            bt_equip.onClick.AddListener(() => {
                UUIManager.S.CreateWindow<UUIHeroEquip>().ShowWindow();
            });

            var swipeEv = swipe.GetComponent<UIEventSwipe>();
            swipeEv.OnSwiping.AddListener((v) => {
                v = v * .5f;
                ThridPersionCameraContollor.Current.RotationX(v.y);//.RotationY(v.x);
            });

            //Write Code here
        }
        protected override void OnShow()
        {
            base.OnShow();
            this.HeroName.text = string.Empty;
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

            lb_coin.text = gate.Coin.ToString("N0");
            lb_gold.text = gate.Gold.ToString("N0");
            if (gate.hero == null) return;
            this.HeroName.text = $"{gate.hero.Name} 等级{gate.hero.Level}";
        }
    }
}