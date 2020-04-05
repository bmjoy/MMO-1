using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UGameTools;
using Proto;
using EConfig;
using ExcelConfig;
using Proto.GateServerService;

namespace Windows
{
    partial class UUIMagic
    {
        public class ContentTableModel : TableItemModel<ContentTableTemplate>
        {
            public ContentTableModel(){}
            public override void InitModel()
            {
                Template.BtClick.onClick.AddListener(() => { OnClick?.Invoke(this); });
            }

            public HeroMagic magic;
            public CharacterMagicData config;
            public Action<ContentTableModel> OnClick;

            internal void SetMagic(CharacterMagicData config,HeroMagic heroMagic)
            {
                magic = heroMagic;
                this.config = config ;
                Template.lb_name.SetKey(config.Name);
                Template.lb_Level.SetKey("UUIMagic_SEL_Level", heroMagic?.Level ?? 1);
                ResourcesManager.S.LoadIcon(config, s => Template.Icon.sprite = s);
            }

            internal void Selected()
            {
                Template.Selected.ActiveSelfObject(true);
            }

            internal void UnSelected()
            {
                Template.Selected.ActiveSelfObject(false);
            }
        }

        protected override void InitModel()
        {
            base.InitModel();

            ButtonClose.onClick.AddListener(() => { this.HideWindow(); });
            bt_level_up.onClick.AddListener(() =>
            {
                var gate = UApplication.G<GMainGate>();
                var request = new C2G_MagicLevelUp { Level = selectMagic?.Level??1, MagicId = selectConfig.ID };
                MagicLevelUp.CreateQuery()
                .SendRequest(gate.Client, request, (res) =>
                {
                    if (res.Code.IsOk())
                    {
                        OnUpdateUIData();
                    }
                    else {
                        UApplication.S.ShowError(res.Code);
                    }
                }, UUIManager.S);
                }
            );
        
        }
        protected override void OnShow()
        {
            base.OnShow();
            OnUpdateUIData();
        }

        protected override void OnUpdateUIData()
        {

            bt_level_up.SetKey("UUIMagic_LevelUp");
            var gata = UApplication.G<GMainGate>();
            int index = 0;
            var configs = ExcelToJSONConfigManager
                .Current.GetConfigs<CharacterMagicData>(t => t.CharacterID == gata.hero.HeroID);

            ContentTableManager.Count = configs.Length;
            foreach (var i in ContentTableManager)
            {
                i.Model.SetMagic(configs[index], null);
                i.Model.OnClick = OnItemClick;
                i.Model.UnSelected();
                index++;
            }

            Desc_Root.ActiveSelfObject(false);
        }

        private void OnItemClick(ContentTableModel obj)
        {
            foreach (var i in ContentTableManager)
                i.Model.UnSelected();
            obj.Selected();
            ShowDetail(obj.config, obj.magic);
        }

        private void ShowDetail(CharacterMagicData config, HeroMagic magic)
        {
            this.selectConfig = config;
            selectMagic = magic;

            Desc_Root.ActiveSelfObject(true);
            int level = magic?.Level ?? 1;
            lb_sel_level.SetKey("UUIMagic_SEL_Level", level);
            lb_sel_name.SetKey(config.Name);
            ResourcesManager.S.LoadIcon(config, s => SelectedIcon.sprite = s);
            des_Text.SetKey(config.Description);

            var levelData = ExcelToJSONConfigManager.Current
                .FirstConfig<MagicLevelUpData>(t => t.Level == level && t.MagicID == config.ID);
            var nextLevel= ExcelToJSONConfigManager.Current
                .FirstConfig<MagicLevelUpData>(t => t.Level == level+1 && t.MagicID == config.ID);

            coin_icon.ActiveSelfObject(false);
            lb_gold.text =$"{levelData?.NeedGold}";
            des_current.SetKey(levelData?.Description);
            des_next.SetKey(nextLevel.Description);
        }

        private CharacterMagicData selectConfig;
        private HeroMagic selectMagic;

        protected override void OnHide()
        {
            base.OnHide();
        }
    }
}