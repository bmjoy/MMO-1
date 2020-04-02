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
                //todo
            }

            public HeroMagic magic;
            public CharacterMagicData config;

            internal void SetMagic(CharacterMagicData config)
            {
                // magic = heroMagic;
                this.config = config ;
                Template.lb_name.text = $"{config.Name}";
                Template.lb_Level.text = $"{0}";
                ResourcesManager.S.LoadIcon(config, s => Template.Icon.sprite = s);
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

            var gata = UApplication.G<GMainGate>();
            int index = 0;
            var configs = ExcelToJSONConfigManager
                .Current.GetConfigs<CharacterMagicData>(t => t.CharacterID == gata.hero.HeroID);

            ContentTableManager.Count = configs.Length;
            foreach (var i in ContentTableManager)
            {
                i.Model.SetMagic(configs[index]);
                index++;
            }
        }
        protected override void OnHide()
        {
            base.OnHide();
        }
    }
}