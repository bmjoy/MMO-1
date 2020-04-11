using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UGameTools;
using Proto;
using P = Proto.HeroPropertyType;
using ExcelConfig;
using EConfig;
using GameLogic.Game;
using Proto.GateServerService;
using GameLogic;
using Layout.LayoutEffects;

namespace Windows
{
    partial class UUIHeroEquip
    {
        public class HeroPartData
        {
            public Image icon;
            public Text level;
            public Button bt;
            public Image rootLvl;
        }

        public class PropertyListTableModel : TableItemModel<PropertyListTableTemplate>
        {
            public PropertyListTableModel(){}
            public override void InitModel()
            {
                //todo
            }

            internal void SetLabel(string key, string value)
            {
                this.Template.lb_text.text = $"{key}:{value}";
            }
        }
        public class EquipmentPropertyTableModel : TableItemModel<EquipmentPropertyTableTemplate>
        {
            public EquipmentPropertyTableModel(){}
            public override void InitModel()
            {
                //todo
            }

            internal void SetLabel(string label)
            {
                Template.lb_text.text = label;
            }
        }

        private Dictionary<EquipmentType, HeroPartData> Equips;// = new Dictionary<EquipmentType, HeroPartData>(); 

        protected override void InitModel()
        {
            base.InitModel();
            bt_Exit.onClick.AddListener(() => { HideWindow(); });

            Equips = new Dictionary<EquipmentType, HeroPartData> {
                { EquipmentType.Arm, new HeroPartData{ bt =equip_weapon, icon = icon_weapon, level = weapon_Lvl, rootLvl=weapLeveRoot } },
                { EquipmentType.Head, new HeroPartData{ bt =equip_head, icon = icon_head, level = head_Lvl , rootLvl=HeadLevelRoot} },
                { EquipmentType.Foot, new HeroPartData{ bt = equip_shose, icon = icon_shose, level = shose_Lvl , rootLvl =ShoseLeveRoot } },
                { EquipmentType.Body, new HeroPartData{ bt = equip_cloth, icon = icon_cloth, level = cloth_Lvl, rootLvl = ClothLeveRoot } }
            };

            foreach (var i in Equips)
            {
                i.Value.bt.onClick.AddListener(() => {
                    Click(i.Key);
                });
            }

            bt_level_up.onClick.AddListener(() => {
                if (selected == null) return;

                var g = UApplication.G<GMainGate>();
                if (!g.package.Items.TryGetValue(selected.GUID, out PlayerItem item)) return;
                var req = new C2G_EquipmentLevelUp{ Guid = selected.GUID, Level = item.Level };
                EquipmentLevelUp.CreateQuery()
                .SendRequest(g.Client,req , (r) => {
                if (r.Code.IsOk())
                {
                    if (r.Level > item.Level)
                    {
                        UApplication.S.ShowNotify(LanguageManager.S.Format("UUIHeroEquip_Level_Success", $" +{r.Level}"));
                    }

                    else
                        UApplication.S.ShowNotify(LanguageManager.S["UUIHeroEquip_Level_Failure"]);
                    }
                    else {
                        UApplication.S.ShowError(r.Code);
                    }
                },UUIManager.S);
            });

            take_off.onClick.AddListener(() =>
            {
                if (selected == null) return;

                var g = UApplication.G<GMainGate>();
                if (!g.package.Items.TryGetValue(selected.GUID, out PlayerItem item)) return;
                var config = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(item.ItemID);
                var equip = ExcelToJSONConfigManager.Current.GetConfigByID<EquipmentData>(int.Parse(config.Params[0]));
                OperatorEquip.CreateQuery()
                .SendRequest(g.Client, new C2G_OperatorEquip
                {
                    Guid = selected.GUID,
                    IsWear = false,
                    Part = (EquipmentType)equip.PartType
                }, (r) => {
                    if (r.Code.IsOk())
                    {
                        UApplication.S
                        .ShowNotify(LanguageManager.S.Format("UUIHeroEquip_TakeOff_Result", $"{config.Name}"));
                        Right.ActiveSelfObject(false);
                        selected = null;
                    }
                    else { UApplication.S.ShowError(r.Code); }
                },UUIManager.S);
            });

            
        }

        private void Click(EquipmentType key)
        {
            var g = UApplication.G<GMainGate>();
            foreach (var i in g.hero.Equips)
            {
                if (i.Part == key)
                {
                    DisplayEquip(i);
                    return;
                }
            }
            Right.ActiveSelfObject(false);
            UUIManager.S.CreateWindowAsync<UUISelectEquip>(ui=>ui.SetPartType(key).ShowWindow());
        }


        private WearEquip selected;

        private void DisplayEquip(WearEquip eq)
        {
            this.selected = eq;
            Right.ActiveSelfObject(true);
            var g = UApplication.G<GMainGate>();
            g.package.Items.TryGetValue(eq.GUID, out PlayerItem it);

            var item = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(eq.ItemID);
            var equip = ExcelToJSONConfigManager.Current.GetConfigByID<EquipmentData>(int.Parse(item.Params[0]));
            ResourcesManager.S.LoadIcon(item,s=> icon_right.sprite = s);
            equip_lvl.text = $"+{it.Level}";
            right_name.text = equip.Name;
            des_Text.text = item.Description;
            RightERoot.ActiveSelfObject(it.Level > 0);
            var level = ExcelToJSONConfigManager.Current
                        .FirstConfig<EquipmentLevelUpData>(t => t.Level == it.Level && t.Quality == item.Quality);
            var next = ExcelToJSONConfigManager.Current
                        .FirstConfig<EquipmentLevelUpData>(t => t.Level == it.Level+1 && t.Quality == item.Quality);
            LevelUp.ActiveSelfObject(next != null);

            if (next != null)
            {
                lb_pro.text = LanguageManager.S.Format("UUIHeroEquip_pro",$"{next.Pro / 100}");
                gold_icon.ActiveSelfObject(next.CostGold > 0);
                coin_icon.ActiveSelfObject(next.CostCoin > 0);
                lb_gold.text = $"{next.CostGold}";
                lb_coin.text = $"{next.CostCoin}";
            }
  
            var properties =  it.GetProperties();

            EquipmentPropertyTableManager.Count = properties.Count;
            int index = 0;
            foreach (var i in properties)
            {
                EquipmentPropertyTableManager[index]
                    .Model.SetLabel(LanguageManager.S.Format($"UUIDetail_{i.Key}",
                    i.Value.ToString()));

                index++;
            }

        }

        protected override void OnShow()
        {
            base.OnShow();
            Right.ActiveSelfObject(false);
            
            var g = UApplication.G<GMainGate>();
            ShowHero(g.hero, g.package);

            take_off.SetKey("UUIHeroEquip_Take_off");
            bt_level_up.SetKey("UUIHeroEquip_bt_level_up");

        }

        protected override void OnHide()
        {
            base.OnHide();
        }



        protected override void OnUpdateUIData()
        {
            base.OnUpdateUIData();
            var g = UApplication.G<GMainGate>();
            ShowHero(g.hero, g.package);
            if (selected == null)
                Right.ActiveSelfObject(false);
            else DisplayEquip(selected);
        }

        private void ShowHero(DHero dHero,PlayerPackage package)
        {
            this.Level.text = LanguageManager.S.Format("UUIHeroEquip_level", $"{dHero.Level}");
            var data = ExcelToJSONConfigManager.Current.GetConfigByID<CharacterData>(dHero.HeroID);
            var properties = new Dictionary<P, ComplexValue>
            {
                { P.Agility, (int)(data.Agility + dHero.Level *data.AgilityGrowth )},
                { P.DamageMax, data.DamageMax },
                { P.DamageMin, data.DamageMin },
                { P.Force, (int)(data.Force+dHero.Level *data.ForceGrowth) },
                { P.Knowledge, (int)(data.Knowledge+dHero.Level*data.KnowledgeGrowth) },
                { P.MaxHp, data.HPMax },
                { P.MaxMp, data.MPMax },
                { P.Jouk,0},
                { P.Resistibility,0 },
                { P.SuckingRate, 0 },
                { P.Defance, data.Defance},
                { P.Crt,0}
            };
            var nextLevel = ExcelToJSONConfigManager.Current.FirstConfig<CharacterLevelUpData>(t => t.Level == dHero.Level + 1);

            foreach (var i in Equips)
            {
                i.Value.icon.ActiveSelfObject(false);
                i.Value.level.text = string.Empty;
                i.Value.rootLvl.ActiveSelfObject(false);
            }

            foreach (var i in dHero.Equips)
            {
                if (package.Items.TryGetValue(i.GUID, out PlayerItem pItem))
                {
                    var item = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(i.ItemID);
                    var equip = ExcelToJSONConfigManager.Current.GetConfigByID<EquipmentData>(int.Parse(item.Params[0]));
                    var ps = pItem.GetProperties();
                    foreach (var kv in ps)
                    {
                        if (properties.TryGetValue(kv.Key, out ComplexValue v))
                        {
                            v.ModifyValueAdd(AddType.Append, kv.Value.FinalValue);
                        }
                    }
                    if (Equips.TryGetValue((EquipmentType)equip.PartType, out HeroPartData partIcon))
                    {
                        partIcon.icon.ActiveSelfObject(true);
                        ResourcesManager.S.LoadIcon(item, s => partIcon.icon.sprite = s);
                        if (pItem.Level > 0) partIcon.level.text = $"+{pItem.Level}";
                        partIcon.rootLvl.ActiveSelfObject(pItem.Level > 0);
                    }
                }
            }

            var appMaxHp = properties[P.Force].FinalValue * BattleAlgorithm.FORCE_HP;
            var appMaxMp = properties[P.Knowledge].FinalValue * BattleAlgorithm.KNOWLEGDE_MP;
            var appDefance = properties[P.Agility].FinalValue * BattleAlgorithm.AGILITY_DEFANCE;
            var speedAdd = properties[P.Agility].FinalValue * BattleAlgorithm.AGILITY_ADDSPEED;
            var attackSpeed = properties[P.Agility].FinalValue * BattleAlgorithm.AGILITY_SUBWAITTIME;
            properties[P.MaxHp].ModifyValueAdd( AddType.Append, appMaxHp);
            properties[P.MaxMp].ModifyValueAdd(AddType.Append, appMaxMp);
            properties[P.Defance].ModifyValueAdd( AddType.Append, appDefance);
            var damage = 0;
            var category = (HeroCategory)data.Category;
            switch (category)
            {
                case HeroCategory.HcAgility:
                    damage = properties[P.Agility].FinalValue;
                    break;
                case HeroCategory.HcForce:
                    damage = properties[P.Force].FinalValue;
                    break;
                case HeroCategory.HcKnowledge:
                    damage = properties[P.Knowledge].FinalValue;
                    break;
            }
            var str = LanguageManager.S["UUIHeroEquip_Main"];// "主属性";
            var list = new Dictionary<string, string>
            {
                { LanguageManager.S["UUIHeroEquip_damage"], $"{properties[P.DamageMin].FinalValue+damage}-{properties[P.DamageMax].FinalValue+damage}" },
                { LanguageManager.S["UUIHeroEquip_Force"], $"{properties[P.Force].FinalValue}"+ (category== HeroCategory.HcForce?str:"")},
                { LanguageManager.S["UUIHeroEquip_Agility"], $"{properties[P.Agility].FinalValue}" + (category== HeroCategory.HcAgility?str:"")},
                { LanguageManager.S["UUIHeroEquip_Knowledge"], $"{properties[P.Knowledge].FinalValue}"+ (category== HeroCategory.HcKnowledge?str:"")},
                { LanguageManager.S["UUIHeroEquip_MaxHp"], $"{properties[P.MaxHp].FinalValue}"},
                { LanguageManager.S["UUIHeroEquip_MaxMp"], $"{properties[P.MaxMp].FinalValue}"},
                { LanguageManager.S["UUIHeroEquip_Defance"], $"{properties[P.Defance].FinalValue}"},
                { LanguageManager.S["UUIHeroEquip_SuckingRate"], $"{(properties[P.SuckingRate].FinalValue/100)}%"},
                { LanguageManager.S["UUIHeroEquip_AttackSpeed"], LanguageManager.S.Format("UUIHeroEquip_AttackSpeed_F", $"{Math.Max(BattleAlgorithm.ATTACK_MIN_WAIT/1000f, data.AttackSpeed - attackSpeed/1000f)}")},
                { LanguageManager.S["UUIHeroEquip_MoveSpeed"], LanguageManager.S.Format("UUIHeroEquip_MoveSpeed_F", $"{Math.Min(data.MoveSpeed +speedAdd,BattleAlgorithm.MAX_SPEED)}")},
                { LanguageManager.S["UUIHeroEquip_Crt"], $"{properties[P.Crt].FinalValue/100}%"},
                { LanguageManager.S["UUIHeroEquip_Exp"], $"{dHero.Exprices}/{nextLevel?.NeedExprices??'-'}"}
            };

            PropertyListTableManager.Count = list.Count;
            int index = 0;
            foreach (var i in list)
            {
                PropertyListTableManager[index].Model.SetLabel(i.Key, i.Value);
                index++;
            }

        }
    }
}