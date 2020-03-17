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

namespace Windows
{
    partial class UUIHeroEquip
    {
        public class HeroPartData
        {
            public RawImage icon;
            public Text level;
            public Button bt;
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
                { EquipmentType.Arm, new HeroPartData{ bt =equip_weapon, icon = icon_weapon, level = weapon_Lvl } },
                { EquipmentType.Head, new HeroPartData{ bt =equip_head, icon = icon_head, level = head_Lvl } },
                { EquipmentType.Foot, new HeroPartData{ bt = equip_shose, icon = icon_shose, level = shose_Lvl } },
                { EquipmentType.Body, new HeroPartData{ bt = equip_cloth, icon = icon_cloth, level = cloth_Lvl } }
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

                EquipmentLevelUp.CreateQuery()
                .SendRequest(g.Client, new
                C2G_EquipmentLevelUp { Guid = selected.GUID, Level = item.Level }, (r) => {
                    if (r.Code.IsOk())
                    {
                        if (r.Level > item.Level)
                            UApplication.S.ShowNotify($"成功升级 +{r.Level}");
                        else
                            UApplication.S.ShowNotify($"装备没有变化");
                    }
                    else {
                        UApplication.S.ShowError(r.Code);
                    }
                });

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
                        UApplication.S.ShowNotify($"成功脱下了{config.Name}");
                        Right.ActiveSelfObject(false);
                        selected = null;
                    }
                    else { UApplication.S.ShowError(r.Code); }
                });
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
            UUIManager.S.CreateWindow<UUISelectEquip>().SetPartType(key).ShowWindow();
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
            //icon_right.texture = ResourcesManager.S.LoadIcon(item);
            equip_lvl.text = $"+{it.Level}";
            right_name.text = equip.Name;
            des_Text.text = item.Description;

            var level = ExcelToJSONConfigManager.Current
                        .FirstConfig<EquipmentLevelUpData>(t => t.Level == it.Level && t.Quality == item.Quality);
            var next = ExcelToJSONConfigManager.Current
                        .FirstConfig<EquipmentLevelUpData>(t => t.Level == it.Level+1 && t.Quality == item.Quality);
            LevelUp.ActiveSelfObject(next != null);

            if (next != null)
            {
                lb_pro.text = $"成功概率:{next.Pro / 100}%";
                gold_icon.ActiveSelfObject(next.CostGold > 0);
                coin_icon.ActiveSelfObject(next.CostCoin > 0);
                lb_gold.text = $"{next.CostGold}";
                lb_coin.text = $"{next.CostCoin}";
            }
            var pro = equip.Properties.SplitToInt();
            var val = equip.PropertyValues.SplitToInt();

            var names = new Dictionary<P, string>() {
                { P.Agility, "敏捷:{0}" },
                { P.Crt,"暴击:{0}"},
                { P.DamageMax,"攻击上限:{0}"},
                { P.DamageMin,"攻击下限:{0}"},
                { P.Defance,"防御:{0}"},
                { P.Force,"力量:{0}"},
                //{ P.Hit,"命中:{0}"},
                {P.Jouk,"闪避:{0}" },
                {P.Knowledge,"智力:{0}" },
                { P.MagicWaitTime,"攻击速度:{0}"},
                { P.MaxHp,"HP:{0}"},
                { P.MaxMp,"MP:{0}"},
                {P.Resistibility,"魔法闪避:{0}" },
                {P.SuckingRate,"吸血等级:{0}"}
            };

            //var level = 
            var properties = new Dictionary<P, ComplexValue>();
            for (var ip = 0; ip < pro.Count; ip++)
            {
                var pr = (P)pro[ip];
                if (!properties.ContainsKey(pr))
                    properties.Add(pr, 0);

                if (properties.TryGetValue(pr, out ComplexValue value))
                {
                    //计算装备加成
                    var eVal = value.AppendValue + (1 + ((level?.AppendRate ?? 0) / 10000f)) * val[ip];
                    value.SetAppendValue((int)eVal);
                }
            }

            EquipmentPropertyTableManager.Count = properties.Count;
            int index = 0;
            foreach (var i in properties)
            {
                if (names.TryGetValue(i.Key, out string format))
                {
                    EquipmentPropertyTableManager[index]
                        .Model.SetLabel(string.Format(format, i.Value.FinalValue));
                }
                index++;
            }

        }

        protected override void OnShow()
        {
            base.OnShow();
            Right.ActiveSelfObject(false);
            var g = UApplication.G<GMainGate>();
            ShowHero(g.hero, g.package);
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
            this.Level.text = $"等级:{dHero.Level}";
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
            }

            foreach (var i in dHero.Equips)
            {
                if (package.Items.TryGetValue(i.GUID, out PlayerItem pItem))
                {
                    var item = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(i.ItemID);
                    var equip = ExcelToJSONConfigManager.Current.GetConfigByID<EquipmentData>(int.Parse(item.Params[0]));
                    var level = ExcelToJSONConfigManager.Current
                        .FirstConfig<EquipmentLevelUpData>(t => t.Level == pItem.Level && t.Quality == item.Quality);

                    var pro = equip.Properties.SplitToInt();
                    var val = equip.PropertyValues.SplitToInt();
                    //var level = 

                    for (var ip = 0; ip < pro.Count; ip++)
                    {
                        if (properties.TryGetValue((P)pro[ip], out ComplexValue value))
                        {
                            //计算装备加成
                            var eVal = value.AppendValue + (1 + ((level?.AppendRate ?? 0) / 10000f)) * val[ip];
                            value.SetAppendValue((int)eVal);
                        }
                    }

                    if (Equips.TryGetValue((EquipmentType)equip.PartType, out HeroPartData partIcon))
                    {
                        partIcon.icon.ActiveSelfObject(true);
                        //partIcon.icon.texture = ResourcesManager.S.LoadIcon(item);
                        if (pItem.Level > 0) partIcon.level.text = $"+{pItem.Level}";
                        //partIcon.bt.onClick.AddListener(() => { })
                    }
                }
            }

            var appMaxHp = properties[P.Force].FinalValue * BattleAlgorithm.FORCE_HP;
            var appMaxMp = properties[P.Knowledge].FinalValue * BattleAlgorithm.KNOWLEGDE_MP;
            var appDefance = properties[P.Agility].FinalValue * BattleAlgorithm.AGILITY_DEFANCE;
            var speedAdd = properties[P.Agility].FinalValue * BattleAlgorithm.AGILITY_ADDSPEED;
            var attackSpeed = properties[P.Agility].FinalValue * BattleAlgorithm.AGILITY_SUBWAITTIME;

            properties[P.MaxHp].SetAppendValue((int)(properties[P.MaxHp].AppendValue + appMaxHp));
            properties[P.MaxMp].SetAppendValue((int)(properties[P.MaxMp].AppendValue + appMaxMp));
            properties[P.Defance].SetAppendValue((int)(properties[P.Defance].AppendValue + appDefance));
            //int damageMin = properties[P.DamageMin].FinalValue;
            //int damageMax = properties[P.DamageMax].FinalValue;
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
            var str = "主属性";
            var list = new Dictionary<string, string>
            {
                { "伤害", $"{properties[P.DamageMin].FinalValue+damage}-{properties[P.DamageMax].FinalValue+damage}" },
                { "力量", $"{properties[P.Force].FinalValue}"+ (category== HeroCategory.HcForce?str:"")},
                { "敏捷", $"{properties[P.Agility].FinalValue}" + (category== HeroCategory.HcAgility?str:"")},
                { "智力", $"{properties[P.Knowledge].FinalValue}"+ (category== HeroCategory.HcKnowledge?str:"")},
                { "血量", $"{properties[P.MaxHp].FinalValue}"},
                { "魔法", $"{properties[P.MaxMp].FinalValue}"},
                { "防御", $"{properties[P.Defance].FinalValue}"},
                { "吸血比例", $"{(properties[P.SuckingRate].FinalValue/100)}%"},
                { "攻击间隔", $"{Math.Max(BattleAlgorithm.ATTACK_MIN_WAIT/1000f, data.AttackSpeed - attackSpeed/1000f)}秒"},
                { "移动速度", $"{Math.Min(data.MoveSpeed +speedAdd,BattleAlgorithm.MAX_SPEED)}米/秒"},
                { "暴击", $"{properties[P.Crt].FinalValue/100}%"},
                { "经验", $"{dHero.Exprices}/{nextLevel?.NeedExprices??'-'}"}
            };

            PropertyListTableManager.Count = list.Count;
            int index = 0;
            foreach (var i in list)
            {
                PropertyListTableManager[index].Model.SetLabel(i.Key, i.Value);
                index++;
            }

            //foreach9var 
        }
    }
}