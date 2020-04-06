using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UGameTools;
using EConfig;
using Proto.GateServerService;
using Proto;

namespace Windows
{
    partial class UUIShopGold
    {
        public class ContentsTableModel : TableItemModel<ContentsTableTemplate>
        {
            public ContentsTableModel(){}
            public override void InitModel()
            {
                Template.ButtonBlue.onClick.AddListener(() => OnClick?.Invoke(this));
            }

            public Action<ContentsTableModel> OnClick;

            public GoldShopData Config;

            internal void SetConfig(GoldShopData item)
            {
                this.Config = item;
                ResourcesManager.S.LoadIcon(item, s => Template.icon.sprite = s);
                Template.lb_gold.text = $"{item.ReceiveGold}";
                Template.ButtonBlue.SetText($"{item.Prices}");
            }
        }

        protected override void InitModel()
        {
            base.InitModel();
            ButtonClose.onClick.AddListener(() => HideWindow()) ;
        }
        protected override void OnShow()
        {
            base.OnShow();

            var goldItems = ExcelConfig.ExcelToJSONConfigManager.Current.GetConfigs<EConfig.GoldShopData>();

            ContentsTableManager.Count = goldItems.Length;
            int index = 0;
            foreach (var i in ContentsTableManager)
            {
                i.Model.SetConfig(goldItems[index]);
                i.Model.OnClick = OnItemClick;
                index++;
            }


        }

        private void OnItemClick(ContentsTableModel obj)
        {
            UUIPopup.ShowConfirm(LanguageManager.S["UUIShopGold_Title"],
                LanguageManager.S.Format("UUIShopGold_Content",
                LanguageManager.S[obj.Config.Name]), () =>
                {
                    var gate = UApplication.G<GMainGate>();
                    var request = new C2G_BuyGold { ShopId = obj.Config.ID };
                    BuyGold.CreateQuery()
                    .SendRequest(gate.Client, request,
                    res =>
                    {
                        if (res.Code.IsOk())
                        {
                            gate.Coin = res.Coin;
                            gate.Gold = res.Gold;
                            UApplication.S.ShowNotify(LanguageManager.S.Format("UUIShopGold_Receive_gold", res.ReceivedGold));
                        }
                        else {
                            UApplication.S.ShowError(res.Code);
                        }
                    },
                    UUIManager.S);
                });
        }

        protected override void OnHide()
        {
            base.OnHide();
        }
    }
}