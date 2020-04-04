using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UGameTools;
using Proto;
using EConfig;
using ExcelConfig;

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
            //Write Code here
        }
        protected override void OnShow()
        {
            base.OnShow();

            bt_level_up.SetKey("UUIMagic_LevelUp");

            var gata = UApplication.G<GMainGate>();
            int index = 0;
            var configs = ExcelToJSONConfigManager
                .Current.GetConfigs<CharacterMagicData>(t => t.CharacterID == gata.hero.HeroID);

            ContentTableManager.Count = configs.Length;
            foreach (var i in ContentTableManager)
            {
                i.Model.SetMagic(configs[index],null);
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
            Desc_Root.ActiveSelfObject(true);
            int level = magic?.Level ?? 1;
            lb_sel_level.SetKey("UUIMagic_SEL_Level", level);
            lb_sel_name.SetKey(config.Name);
        }

        protected override void OnHide()
        {
            base.OnHide();
        }
    }
}