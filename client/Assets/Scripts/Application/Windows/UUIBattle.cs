using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UGameTools;
using UnityEngine;
using Proto;
using ExcelConfig;
using GameLogic.Game.Perceptions;
using EConfig;
using Proto.BattleServerService;
using Vector3 = UnityEngine.Vector3;

namespace Windows
{
    partial class UUIBattle
    {
        public class GridTableModel : TableItemModel<GridTableTemplate>
        {
            public GridTableModel() { }
            public override void InitModel()
            {
                this.Template.Button.onClick.AddListener(
                    () =>
                    {
                        if ((lastTime + 0.3f > UnityEngine.Time.time)) return;
                        lastTime = UnityEngine.Time.time;
                        if (OnClick == null)
                            return;
                        OnClick(this);
                    });
            }

            public Action<GridTableModel> OnClick;

            public void SetMagic(int id, float cdTime)
            {
                if (magicID != id)
                {
                    magicID = id;
                    MagicData = ExcelToJSONConfigManager.Current.GetConfigByID<CharacterMagicData>(id);
                    var per = UApplication.G<BattleGate>().PreView as IBattlePerception;
                    var magic = per.GetMagicByKey(MagicData.MagicKey);
                    if (magic != null) Template.Button.SetText(magic.name);
                    Template.Icon.sprite = ResourcesManager.S.LoadIcon(MagicData);
                }
            }

            private int magicID = -1;
            public CharacterMagicData MagicData;
            private float cdTime = 0.01f;

            private float lastTime = 0;

            public void Update(UCharacterView view, float now)
            {
                if (view.TryGetMagicData(magicID, out HeroMagicData data))
                {
                    var time = Mathf.Max(0, data.CDTime - now);
                    this.Template.CDTime.text = time > 0 ? string.Format("{0:0.0}", time) : string.Empty;
                    if (cdTime < time)
                        cdTime = time;
                    if (time > 0)
                    {
                        lastTime = UnityEngine.Time.time;
                    }
                    if (cdTime > 0)
                    {
                        this.Template.ICdMask.fillAmount = time / cdTime;
                    }
                    else
                    {
                        this.Template.ICdMask.fillAmount = 0;
                    }
                }
            }
        }

        protected override void InitModel()
        {
            base.InitModel();
            

            bt_Exit.onClick.AddListener(() =>
                {
                    UUIPopup.ShowConfirm("Quit", "Do you want quit?", () =>
                    {
                        var gate = UApplication.G<BattleGate>();
                        ExitBattle.CreateQuery()
                        .SendRequest(gate.Client,
                        new C2B_ExitBattle
                        {
                            AccountUuid = UApplication.S.AccountUuid
                        },
                        (r)=>
                        {
                            UApplication.S.GoBackToMainGate();
                            if (!r.Code.IsOk())
                                UApplication.S.ShowError(r.Code);
                        }, UUIManager.S);
                    });
                });

            var bt = this.Joystick_Left.GetComponent<zFrame.UI.Joystick>();
            float lastTime = -1;
            //Vector2 last = Vector2.zero;
            bt.OnValueChanged.AddListener((v) =>
            {
                if (lastTime > UnityEngine.Time.time) return;
                lastTime = UnityEngine.Time.time + .3f;
                //if ((v - last).magnitude <= 0.03f) return;
                //last = v;
                var dir = ThridPersionCameraContollor.Current.LookRotaion * new Vector3(v.x, 0, v.y);
                var g = UApplication.G<BattleGate>();
                if (g == null) return;
                g.MoveDir(dir);
            });

            var swipeEv = swipe.GetComponent<UIEventSwipe>();
            swipeEv.OnSwiping.AddListener((v) =>
            {
                v = v * .5f;
                ThridPersionCameraContollor.Current.RotationX(v.y).RotationY(v.x);
            });

            bt_normal_att.onClick.AddListener(() =>
            {
                var g = UApplication.G<BattleGate>();
                if (g == null) return;
                g.DoNormalAttack();
            });

            bt_hp.onClick.AddListener(() => 
            {
                var g = UApplication.G<BattleGate>();
                if (g == null) return;
                if (g.IsHpFull()) { UApplication.S.ShowNotify($"Hp 已经满"); }
                g.SendUserItem(ItemType.ItHpitem);
            });
            bt_mp.onClick.AddListener(() => {
                var g = UApplication.G<BattleGate>();
                if (g == null) return;
                if (g.IsMpFull()) { UApplication.S.ShowNotify($"Mp 已经满"); }
                g.SendUserItem(ItemType.ItMpitem);
            });
        }

        protected override void OnShow()
        {
            base.OnShow();
            this.GridTableManager.Count = 0;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            var gate = UApplication.G<BattleGate>();
            if (gate == null) return;
            var timeSpan = TimeSpan.FromSeconds(gate.TimeServerNow);
            this.Time.text = string.Format("{0:00}:{1:00}", (int)timeSpan.TotalMinutes, timeSpan.Seconds);
            foreach (var i in GridTableManager)
            {
                i.Model.Update(view, gate.TimeServerNow);
            }
        }

        protected override void OnHide()
        {
            base.OnHide();
        }

        public void InitCharacter(UCharacterView view)
        {
            if (view.TryGetMagicsType(MagicType.MtMagic, out IList<HeroMagicData> list))
            {
                //var magic = view.TryGetMagicByType.Where(t => IsMaigic(t.MagicID)).ToList();
                this.GridTableManager.Count = list.Count;
                int index = 0;
                foreach (var i in GridTableManager)
                {
                    i.Model.SetMagic(list[index].MagicID, list[index].CDTime);
                    i.Model.OnClick = OnRelease;
                    index++;
                }
            }

            if (view.TryGetMagicByType(MagicType.MtNormal, out HeroMagicData data))
            {
                var config = ExcelToJSONConfigManager.Current.GetConfigByID<CharacterMagicData>(data.MagicID);
                att_Icon.sprite = ResourcesManager.S.LoadIcon(config);
            }
            this.view = view;
        }


        public void SetPackage(PlayerPackage package)
        {
            int hp = 0;
            int mp = 0;

            foreach (var i in package.Items)
            {
                var config = ExcelToJSONConfigManager.Current.GetConfigByID<ItemData>(i.Value.ItemID);
                if ((ItemType)config.ItemType == ItemType.ItHpitem)
                {
                    hp += i.Value.Num;
                    hp_item_Icon.sprite = ResourcesManager.S.LoadIcon(config);
                }
                if ((ItemType)config.ItemType == ItemType.ItMpitem)
                {
                    mp_item_Icon.sprite = ResourcesManager.S.LoadIcon(config);
                    mp += i.Value.Num;
                }
            }

            bt_hp.ActiveSelfObject(hp > 0);
            bt_mp.ActiveSelfObject(mp > 0);
            hp_num.text = $"{hp}";
            mp_num.text = $"{mp}";
        }
        

        private void OnRelease(GridTableModel item)
        {
            UApplication.G<BattleGate>().ReleaseSkill(item.MagicData);
        }

        private UCharacterView view;

        public bool IsMaigic(int id)
        {
            var data = ExcelToJSONConfigManager.Current.GetConfigByID<CharacterMagicData>(id);
            if (data == null) return false;
            return data.ReleaseType == (int)MagicReleaseType.MrtMagic;
        }

    }
}