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

                Template.ItemBg.onClick.AddListener(() => { OnDetail?.Invoke(this); });
            }

            public Action<ContentTableModel> OnDetail;

            public Action<ContentTableModel> OnBuy;

            public ItemsShop.Types.ShopItem ShopItem { get; private set; }
            public ItemsShop Shop { get; private set; }
            public EConfig.ItemData Config { get; private set; }
            internal void SetItem(ItemsShop.Types.ShopItem shopItem, ItemsShop shop)
            {
                this.ShopItem = shopItem;
                this.Shop = shop;
                Config = ExcelConfig.ExcelToJSONConfigManager.Current.GetConfigByID<EConfig.ItemData>(ShopItem.ItemId);
                ResourcesManager.S.LoadIcon(Config,s=> Template.icon.sprite = s);
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

            var gate = UApplication.G<GMainGate>();
            QueryShop.CreateQuery()
                .SendRequest(gate.Client, new C2G_Shop { }, (res) =>
            {
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
            
            if (Shops.Count > 0) ShopTabTableManager[0].Template.ToggleSelected.isOn = true;
        }

        private void Selected(ShopTabTableModel obj)
        {
            ContentTableManager.Count = obj.Shop.Items.Count;
            int index = 0;
            foreach (var i in ContentTableManager)
            {
                i.Model.SetItem(obj.Shop.Items[index], obj.Shop);
                i.Model.OnBuy = Buy;
                i.Model.OnDetail = ShowDetail;
                index++;
            }
        }

        private void ShowDetail(ContentTableModel obj)
        {
            var item = new PlayerItem { ItemID = obj.Config.ID, Level = 0, Num = obj.ShopItem.PackageNum };
            UUIManager.S.CreateWindowAsync<UUIDetail>(ui => ui.Show(item, true));
        }

        private void Buy(ContentTableModel obj)
        {

            UUIPopup.ShowConfirm(LanguageManager.S["UUIItemShop_Confirm_Title"],
                LanguageManager.S.Format("UUIItemShop_Confirm_Content", obj.Config.Name),
                () => {
                    var request = new C2G_BuyItem
                    {
                        ItemId = obj.ShopItem.ItemId,
                        ShopId = obj.Shop.ShopId
                    };
                    var gate = UApplication.G<GMainGate>();
                    BuyItem.CreateQuery().SendRequest(gate.Client,request, (r) =>
                    {
                        //UUIManager.S.UnMaskEvent();
                        if (r.Code.IsOk())
                        {
                            UApplication.S.ShowNotify(LanguageManager.S.Format("UUIItemShop_BUY", $"{obj.Config.Name}", $"{obj.ShopItem.PackageNum}"));
                        }
                        else
                        {
                            UApplication.S.ShowError(r.Code);
                        }
                    }, UUIManager.S);
                }
                );
        }

        protected override void OnHide()
        {
            base.OnHide();
        }


    }
}