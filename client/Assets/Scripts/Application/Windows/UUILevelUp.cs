using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UGameTools;

namespace Windows
{
    partial class UUILevelUp
    {

        protected override void InitModel()
        {
            base.InitModel();
            ButtonClose.onClick.AddListener(() => { HideWindow(); });
            Root.OnMouseClick((g) => { HideWindow(); });
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

        internal void ShowWindow(int level)
        {
            lb_level.text = $"{level}";
            ShowWindow();
        }
    }
}