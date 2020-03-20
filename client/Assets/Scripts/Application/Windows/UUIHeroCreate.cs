using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UGameTools;
using ExcelConfig;
using EConfig;
using Proto.GateServerService;
using UnityEngine;
using System.Collections;
using GameLogic.Game.Elements;

namespace Windows
{
    partial class UUIHeroCreate
    {
        public class ListTableModel : TableItemModel<ListTableTemplate>
        {
            public ListTableModel(){}
            public override void InitModel()
            {
                this.Template.BtHero.onClick.AddListener(() =>
                {
                    OnClick?.Invoke(this);
                });
                //todo
            }

            internal void SetData(CharacterPlayerData characterPlayer)
            {
                Config = characterPlayer;
                ChaData = ExcelToJSONConfigManager.Current.GetConfigByID<CharacterData>(Config.CharacterID);
                this.Template.lb_name.text = ChaData.Name;
            }

            public CharacterPlayerData Config;

            public CharacterData ChaData;

            public Action<ListTableModel> OnClick;
        }

        protected override void InitModel()
        {
            base.InitModel();
            Bt_create.onClick.AddListener(() =>
            {
                if (string.IsNullOrEmpty(InputField.text) || InputField.text.Length < 2)
                {
                    UApplication.S.ShowNotify($"英雄的名称不能为空或者长度小于2");
                    return;
                }

                var request = new Proto.C2G_CreateHero { HeroID = selectedID, HeroName = InputField.text };
                CreateHero.CreateQuery().SendRequest(UApplication.G<GMainGate>().Client, request,
                    (r) =>
                    {
                        if (r.Code.IsOk())
                        {
                            UApplication.G<GMainGate>().ShowMain();
                            HideWindow();
                        }
                        else
                            UApplication.S.ShowError(r.Code);
                    }, UUIManager.S);
            });
        }

        protected override void OnShow()
        {
            base.OnShow();

            var heros = ExcelToJSONConfigManager.Current.GetConfigs<CharacterPlayerData>();
            
            ListTableManager.Count = heros.Length;
            int index = 0;

            SetHeroId(heros[0],
                ExcelToJSONConfigManager.Current.GetConfigByID<CharacterData>(heros[0].CharacterID));
            foreach (var i in heros)
            {
                ListTableManager[index].Model.SetData(heros[index]);
                ListTableManager[index].Model.OnClick = ClickItem;
                index++;
            }
        }

        private int selectedID = 0;

        private void ClickItem(ListTableModel obj)
        {
            SetHeroId(obj.Config,obj.ChaData);
           
        }

        private void SetHeroId(CharacterPlayerData hero, CharacterData  character)
        {
            selectedID = character.ID;
            var v =UApplication.G<GMainGate>().ReCreateHero(character.ID, character.Name);
            lb_description.text = hero.Description;

            StartCoroutine(RunMotion(v, hero.Motion));
        }

        private IEnumerator RunMotion(UCharacterView view, string motion)
        {
            yield return new WaitForSeconds(.5f);
            if (!view) yield break;
            (view as IBattleCharacter).PlayMotion(motion);
        }

        protected override void OnHide()
        {
            base.OnHide();
        }
    }
}