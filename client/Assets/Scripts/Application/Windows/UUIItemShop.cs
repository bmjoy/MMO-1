using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UGameTools;
using Google.Protobuf.Collections;
using Proto;
using Proto.GateServerService;

namespace Windows
{
    partial class UUIItemShop
    {
        public RepeatedField<ItemsShop> Shops { get; private set; }

        public class ShopTabTableModel : TableItemModel<ShopTabTableTemplate>
        {
            public ShopTabTableModel(){}
            public override void InitModel()
            {
                this.Template.ToggleSelected.onValueChanged.AddListener((isOn) => {
                    if (isOn) OnSelected?.Invoke(this);
                });
            }
            public Action<ShopTabTableModel> OnSelected;

            public ItemsShop Shop { get; private set; }

            internal void SetData(ItemsShop itemsShop)
            {
                this.Shop = itemsShop;
                var config = ExcelConfig.ExcelToJSONConfigManager.Current.GetConfigByID<EConfig.ItemShopData>(itemsShop.ShopId);
                this.Template.ShopName.text = $"{config?.Name}";
            }
        }
        public class ContentTableModel : TableItemModel<ContentTableTemplate>
        {
            public ContentTableModel(){}
            public override void InitModel()
            {
                Template.ButtonCoin.onClick.AddListener(() => {
                    OnBuy?.Invoke(this);
                });

                Template.ButtonGold.onClick.AddListener(() => {
                    OnBuy?.Invoke(this);
                });
            }

            public Action<ContentTableModel> OnBuy;

            public ItemsShop.Types.ShopItem ShopItem { get; private set; }
            public ItemsShop Shop { get; private set; }
            public EConfig.ItemData Config { get; private set; }
            internal void SetItem(ItemsShop.Types.ShopItem shopItem, ItemsShop shop)
            {
                this.ShopItem = shopItem;
                this.Shop = shop;
                Config = ExcelConfig.ExcelToJSONConfigManager.Current.GetConfigByID<EConfig.ItemData>(ShopItem.ItemId);
                Template.icon.sprite = ResourcesManager.S.LoadIcon(Config);
                Template.Name.text = Config.Name;
                Template.ItemCount.ActiveSelfObject(shopItem.PackageNum > 1);
                Template.t_num.text = $"{ShopItem.PackageNum}";
                Template.ButtonCoin.ActiveSelfObject(ShopItem.CType == ItemsShop.Types.CoinType.Coin);
                Template.ButtonGold.ActiveSelfObject(ShopItem.CType == ItemsShop.Types.CoinType.Gold);
                Template.ButtonGold.SetText($"{shopItem.Prices}");
                Template.ButtonCoin.SetText($"{shopItem.Prices}");
            }
        }

        protected override void InitModel()
        {
            base.InitModel();
            ButtonClose.onClick.AddListener(() =>
            {
                HideWindow();
            });
        }
        protected override void OnShow()
        {
            base.OnShow();

            //UUIManager.S.MaskEvent();
            var gate = UApplication.G<GMainGate>();
            QueryShop.CreateQuery()
                .SendRequest(gate.Client, new C2G_Shop { }, (res) =>
            {
                //UUIManager.S.UnMaskEvent();
                if (res.Code.IsOk())
                {
                    this.Shops = res.Shops;
                    ShowData();
                    return;
                }
                HideWindow();
                UApplication.S.ShowError(res.Code);
            },UUIManager.S);

        }

        private void ShowData()
        {


            this.ShopTabTableManager.Count = Shops.Count;
            int index = 0;
            foreach (var i in ShopTabTableManager)
            {
                i.Model.SetData(Shops[index]);
                i.Model.OnSelected = Selected;
                index++;
            }
            //todo 
            if (Shops.Count > 0) ShopTabTableManager[0].Template.ToggleSelected.isOn = true;
        }

        //private int last = -1;

        private void Selected(ShopTabTableModel obj)
        {
            ContentTableManager.Count = obj.Shop.Items.Count;
            int index = 0;
            foreach (var i in ContentTableManager)
            {
                i.Model.SetItem(obj.Shop.Items[index], obj.Shop);
                i.Model.OnBuy = Buy;
                index++;
            }
        }

        private void Buy(ContentTableModel obj)
        {
            //UUIManager.S.MaskEvent();
            var gate = UApplication.G<GMainGate>();
            BuyItem.CreateQuery().SendRequest(gate.Client, new C2G_BuyItem
            {
                ItemId = obj.ShopItem.ItemId,
                ShopId = obj.Shop.ShopId
            }, (r) =>
            {
                //UUIManager.S.UnMaskEvent();
                if (r.Code.IsOk())
                {
                    UApplication.S.ShowNotify($"购买 {obj.Config.Name}*{obj.ShopItem.PackageNum}");
                }
                else {
                    UApplication.S.ShowError(r.Code);
                }
            },UUIManager.S);
        }

        protected override void OnHide()
        {
            base.OnHide();
        }


    }
}