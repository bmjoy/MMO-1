using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UGameTools;
using Proto;
using EConfig;
using Proto.GateServerService;

namespace Windows
{
    partial class UUILevelList
    {
        public class ContentTableModel : TableItemModel<ContentTableTemplate>
        {
            public ContentTableModel(){}
            public override void InitModel()
            {
                this.Template.ButtonGreen.onClick.AddListener(() =>
                {
                    Onclick?.Invoke(this);
                });
            }

            public Action<ContentTableModel> Onclick;
            public BattleLevelData Data{ set; get; }

            public void SetLevel(BattleLevelData level)
            {
                Template.ButtonBrown.ActiveSelfObject(false);
                Data = level;
                this.Template.Name.text = $"{level.Name} Lvl:{level.LimitLevel}";
                this.Template.Desc.text = $"{level.Description}";
                 ResourcesManager.S.LoadIcon(level,s=> this.Template.missionImage.sprite =s);
            }
        }

        protected override void InitModel()
        {
            base.InitModel();
            Bt_Return.onClick.AddListener(() =>
            {
                this.HideWindow();
            });
        }
        protected override void OnShow()
        {
            base.OnShow();
            var levels = ExcelConfig.ExcelToJSONConfigManager.Current.GetConfigs<BattleLevelData>();
            ContentTableManager.Count = levels.Length;
            int index = 0;
            foreach (var i in ContentTableManager)
            {
                i.Model.SetLevel(levels[index]);
                i.Model.Onclick = OnItemClick;
                
                index++;
            }
        }

        private void OnItemClick(ContentTableModel item)
        {
            var gate = UApplication.G<GMainGate>();
            if (gate == null) return;

            BeginGame.CreateQuery().SendRequest(gate.Client,
                new C2G_BeginGame { LevelID = item.Data.ID },
                r =>
                {
                    if (r.Code.IsOk())
                    {
                        UApplication.Singleton.GotoBattleGate(r.ServerInfo, item.Data.ID);
                    }
                    else
                    {
                        UApplication.Singleton.ShowError(r.Code);
                    }
                }, UUIManager.S);
        }
           

        protected override void OnHide()
        {
            base.OnHide();
        }
    }
}