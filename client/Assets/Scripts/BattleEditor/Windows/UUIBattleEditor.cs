using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UGameTools;
using EConfig;
using ExcelConfig;
using GameLogic.Game.AIBehaviorTree;
using UnityEngine;

namespace Windows
{
    partial class UUIBattleEditor
    {
        public class GridTableModel : TableItemModel<GridTableTemplate>
        {
            public GridTableModel(){}
            public override void InitModel()
            {
                //todo
            }
        }

        protected override void InitModel()
        {
            base.InitModel();
            bt_add.onClick.AddListener(() =>
            {

                if (int.TryParse(input_skill.text, out int skillId))
                {
                    var magic = ExcelToJSONConfigManager.Current.GetConfigByID<CharacterMagicData>(skillId);
                    if (magic == null) return;
                    EditorStarter.S.releaser.AddMagic(magic);
                }
            });

            bt_remove.onClick.AddListener(() =>
            {
                if (int.TryParse(input_skill.text, out int skillId))
                {
                    var magic = ExcelToJSONConfigManager.Current.GetConfigByID<CharacterMagicData>(skillId);
                    if (magic == null) return;
                    EditorStarter.S.releaser.RemoveMaic(magic.ID);
                }
            });

            bt_releaser.onClick.AddListener(() =>
            {
                int.TryParse(input_Level.text, out int level);
                level = Mathf.Clamp(level,1, 100);

                if (int.TryParse(input_index.text, out int charId))
                {
                    var character = ExcelToJSONConfigManager.Current.GetConfigByID<CharacterData>(charId);
                    if (character == null) return;
                    EditorStarter.S.ReplaceRelease(level,character, to_do_remove.isOn, to_enable_ai.isOn);
                }
            });

            bt_targe.onClick.AddListener(() =>
            {
                int.TryParse(input_Level.text, out int level);
                level = Mathf.Clamp(level,1, 100);
                if (int.TryParse(input_index.text, out int charId))
                {
                    var character = ExcelToJSONConfigManager.Current.GetConfigByID<CharacterData>(charId);
                    if (character == null) return;
                    EditorStarter.S.ReplaceTarget(level,character, to_do_remove.isOn, to_enable_ai.isOn);
                }
            });

            Joystick_Left.GetComponent<zFrame.UI.Joystick>().OnValueChanged.AddListener((v) =>
            {
                //Debug.Log(v);
                var dir = ThridPersionCameraContollor.Current.LookRotaion* new Vector3(v.x, 0, v.y);
                //Debug.Log($"{v}->{dir}");
                EditorStarter.S.DoAction(new Proto.Action_MoveDir
                {
                    Fast = true,
                    Position = EditorStarter.S.releaser.Position.ToPVer3(),
                    Forward = new Proto.Vector3 { X = dir.x, Z = dir.z }
                });
            });

            s_distance.onValueChanged.AddListener((v) =>
            {
                EditorStarter.S.distanceCharacter = Mathf.Lerp(15, 3, v);
                EditorStarter.S.isChanged = true;
            });

            bt_normal_att.onClick.AddListener(() =>
            {
                EditorStarter.S.DoAction(new Proto.Action_NormalAttack());
            });
            //Write Code here
        }
        protected override void OnShow()
        {
            base.OnShow();
        }
        protected override void OnHide()
        {
            base.OnHide();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            EditorStarter.S.ry = Mathf.Lerp(-180, 180, s_rot_y.value);
            EditorStarter.S.slider_y = Mathf.Lerp(8,87, s_rot_x.value);
            EditorStarter.S.distance = Mathf.Lerp(2, 22, s_distance_camera.value);
           
            Time.timeScale =  s_time_scale.value;
        }
    }
}