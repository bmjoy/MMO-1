using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UGameTools;
using Proto;
using ExcelConfig;
using EConfig;
using GameLogic.Game;
using P = Proto.HeroPropertyType;

namespace Windows
{
    partial class UUIItemRefresh
    {
        public class PropertyListTableModel : TableItemModel<PropertyListTableTemplate>
        {
            public PropertyListTableModel(){}
            public override void InitModel()
            {
                //todo
            }

            internal void SetLabel(string v)
            {
                Template.lb_text.text = v;
            }
        }
        public class ItemListTableModel : TableItemModel<ItemListTableTemplate>
        {
            public ItemListTableModel(){}
            public override void InitModel()
            {
                this.Template.BtSelected.onClick.AddListener(() => OnClick?.Invoke(this));
            }

            public Action<ItemListTableModel> OnClick;


            public PlayerItem PlayerItem { private set; get; }
            internal void SetEmpty()
            {
                Template.icon_right.ActiveSelfObject(false);
                Template.AddIconSel.ActiveSelfObject(true);
                Template.ERoot.ActiveSelfObject(false);
            }

            public void SetPlayItem(PlayerItem item)
            {
                PlayerItem = item;
                Template.ERoot.ActiveSelfObject(item.Level>0);
                Template.AddIconSel.ActiveSelfObject(false);
                var config = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(PlayerItem.ItemID);
                Template.equip_lvl.text = $"+{item.Level}";
                //var equip = ExcelToJSONConfigManager.Current.GetConfigByID<EquipmentData>(int.Parse(item.Params[0]));
                ResourcesManager.S.LoadIcon(config, s =>
                {
                    this.Template.icon_right.sprite = s;
                    Template.icon_right.ActiveSelfObject(true);
                });
            }
        }
        public class EquipmentPropertyTableModel : TableItemModel<EquipmentPropertyTableTemplate>
        {
            public EquipmentPropertyTableModel(){}

            public override void InitModel() { }
  
            internal void SetLabel(string v)
            {
                Template.lb_text.text = v;
            }
        }

        protected override void InitModel()
        {
            base.InitModel();
            CloseButton.onClick.AddListener(() => HideWindow());
            equipRefresh.onClick.AddListener(() => SelectedItem());

            bt_level_up.onClick.AddListener(() => BeginRefresh());
        }

        private void BeginRefresh()
        {
            if (currentRefreshData == null) return;
            if (refreshItem == null)
            {
                UApplication.S.ShowNotify(LanguageManager.S["UUIItemRefresh_noItem"]);
                return;
            }
            if (customItems == null || customItems.Count < currentRefreshData.NeedItemCount)
            {
                //UUIItemRefresh_custom_empty
                UApplication.S.ShowNotify(LanguageManager.S["UUIItemRefresh_custom_empty"]);
                return;
            }

            

            var gate = UApplication.G<GMainGate>();
            var request = new C2G_RefreshEquip { EquipUuid = refreshItem.GUID };

            foreach (var i in customItems)
            {
                request.CoustomItem.Add(i.GUID);
            }
            Proto.GateServerService.RefreshEquip.CreateQuery()
            .SendRequest(gate.Client, request,
            res =>
            {
                if (res.Code.IsOk())
                {
                    UApplication.S.ShowNotify(LanguageManager.S["UUIItemRefresh_Sucess"]);
                    return;
                }

                UApplication.S.ShowError(res.Code);
            }, UUIManager.S);
        }


        private void SelectedItem()
        {
            UUIManager.S.CreateWindowAsync<UUISelectItem>(ui =>
            {
                ui.ShowSelect(1, false);
                ui.OnSelectedItems = OnSelectRefresh;
            } );
        }

        private void OnSelectRefresh(List<PlayerItem> obj)
        {
            customItems = null;
            refreshItem = obj[0];
            ShowRefreshItem(refreshItem);
            ShowRight(refreshItem);
        }

        private PlayerItem refreshItem;
        private EquipRefreshData currentRefreshData;
        private List<PlayerItem> customItems;

        private void ShowRight(PlayerItem item)
        {
            var config = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(item.ItemID);
            var refreshData = currentRefreshData;
            if (refreshData.MaxRefreshTimes <= item.Data?.RefreshTime)
            {
                UApplication.S.ShowNotify(LanguageManager.S["UUIItemRefresh_max_times"]);
                return;
            }

            Right.ActiveSelfObject(true);
            
            var equip = ExcelToJSONConfigManager.Current.GetConfigByID<EquipmentData>(int.Parse(config.Params[0]));
            //var refreshData = currentRefreshData = ExcelToJSONConfigManager.Current.GetConfigByID<EquipRefreshData>(config.Quality);
            ItemListTableManager.Count = refreshData.NeedItemCount;
            foreach (var i in ItemListTableManager)
            {
                i.Model.SetEmpty();
                i.Model.OnClick = ClickCustom;
            }
            LevelUp.ActiveSelfObject(refreshData.MaxRefreshTimes > item.Data?.RefreshTime);
            lb_pro.SetKey("UUIRefreshItem_pro", currentRefreshData.Pro / 100);
            coin_icon.ActiveSelfObject(false);
            lb_gold.text = $"{currentRefreshData.CostGold}";
            EquipmentPropertyTableManager.Count = 0;
        }

        private void ClickCustom(ItemListTableModel obj)
        {
            UUIManager.S.CreateWindowAsync<UUISelectItem>(ui =>
            {
                ui.ShowSelect(currentRefreshData?.NeedItemCount??1,true, refreshItem.GUID, currentRefreshData.NeedQuality);
                ui.OnSelectedItems = OnSelectCustomItems;
            });
        }

        private void OnSelectCustomItems(List<PlayerItem> obj)
        {
            customItems = obj;
            int index = 0;
            foreach (var i in ItemListTableManager)
            {
                i.Model.SetPlayItem(obj[index]);
                index++;
            }
            ShowProperty(obj);
        }
        private void ShowProperty(List<PlayerItem> obj)
        {

            //var refreshProperty = ExcelToJSONConfigManager.Current.GetConfigs<RefreshPropertyValueData>();

            var properties = new Dictionary<P, ComplexValue>();
            foreach (var it in obj)
            {
                var item = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(it.ItemID);
                var equip = ExcelToJSONConfigManager.Current.GetConfigByID<EquipmentData>(int.Parse(item.Params[0]));
                var pro = equip.Properties.SplitToInt();
                var val = equip.PropertyValues.SplitToInt();

                for (var ip = 0; ip < pro.Count; ip++)
                {
                    var pr = (P)pro[ip];
                    var fpv = ExcelToJSONConfigManager.Current.GetConfigByID<RefreshPropertyValueData>((int)pr);
                    if (fpv == null) continue;
                    if (!properties.ContainsKey(pr))  properties.Add(pr, 0);
                    if (properties.TryGetValue(pr, out ComplexValue value))
                    {
                        value.SetBaseValue(value.BaseValue + val[ip]* fpv.Value);
                    }
                }
            }

            EquipmentPropertyTableManager.Count = properties.Count;
            int index = 0;
            foreach (var i in properties)
            {
                EquipmentPropertyTableManager[index].Model.SetLabel(LanguageManager.S.Format($"UUIDetail_{i.Key}",
                    $"{currentRefreshData.PropertyAppendMin}~{currentRefreshData.PropertyAppendMax}"));
                index++;
            }
        }


        private void ShowRefreshItem(PlayerItem it)
        {
            var item = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(it.ItemID);
            var equip = ExcelToJSONConfigManager.Current.GetConfigByID<EquipmentData>(int.Parse(item.Params[0]));
            ResourcesManager.S.LoadIcon(item, s => icon.sprite = s);
            currentRefreshData = ExcelToJSONConfigManager.Current.GetConfigByID<EquipRefreshData>(item.Quality);
            lb_Lvl.text = $"+{it.Level}";
            lb_equipname.text = equip.Name;
            lb_description.text = item.Description;
            lb_equiprefresh.SetKey("UUIItemRefresh_RefreshTimes", $"{ currentRefreshData.MaxRefreshTimes - it.Data?.RefreshTime}");
            LevelRoot.ActiveSelfObject(it.Level > 0);
            var level = ExcelToJSONConfigManager.Current
                        .FirstConfig<EquipmentLevelUpData>(t => t.Level == it.Level && t.Quality == item.Quality);        
            var pro = equip.Properties.SplitToInt();
            var val = equip.PropertyValues.SplitToInt();


            var properties = new Dictionary<P, ComplexValue>();
            for (var ip = 0; ip < pro.Count; ip++)
            {
                var pr = (P)pro[ip];
                if (!properties.ContainsKey(pr))
                    properties.Add(pr, 0);

                if (properties.TryGetValue(pr, out ComplexValue value))
                {
                    value.SetBaseValue(value.BaseValue + val[ip]);
                    value.SetRate(level?.AppendRate ?? 0);
                }
            }
            if (it.Data != null)
            {
                foreach (var i in it.Data.Values)
                {
                    var k = (P)i.Key;
                    if (!properties.ContainsKey(k))
                        properties.Add(k, 0);

                    if (properties.TryGetValue(k, out ComplexValue value))
                    {
                        value.SetAppendValue(value.AppendValue + i.Value);
                    }
                }
            }

            PropertyListTableManager .Count = properties.Count;
            int index = 0;
            foreach (var i in properties)
            {
                PropertyListTableManager[index]
                    .Model.SetLabel(LanguageManager.S.Format($"UUIDetail_{i.Key}",
                    i.Value.ToString()));

                index++;
            }
        }

        protected override void OnShow()
        {
            base.OnShow();
            customItems = null;
            lb_Lvl.text =
            lb_equipname.text = lb_equiprefresh.text=
            lb_description.text = string.Empty;

            LevelRoot.ActiveSelfObject(false);
            Right.ActiveSelfObject(false);


            SelectedItem();


        }

        protected override void OnUpdateUIData()
        {
            base.OnUpdateUIData();
            if (refreshItem != null)
            {
                var gata = UApplication.G<GMainGate>();
                if (gata.package.Items.TryGetValue(refreshItem.GUID, out refreshItem))
                {
                    OnSelectRefresh(new List<PlayerItem> { refreshItem });
                }
            }
        }

        protected override void OnHide()
        {
            base.OnHide();
        }
    }
}