using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UGameTools;

namespace Windows
{
    partial class UUIPopup
    {

        protected override void InitModel()
        {
            base.InitModel();
            ButtonBlue.onClick.AddListener(() => { Ok?.Invoke(); HideWindow(); });
            ButtonBrown.onClick.AddListener(() => { Cancel?.Invoke(); HideWindow(); });
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

        private Action Ok;
        private Action Cancel;

        public static UUIPopup ShowConfirm(string title, string content, Action ok, Action cancel =null)
        {
            var ui = UUIManager.S.CreateWindow<UUIPopup>();
            ui.Ok = ok;
            ui.Cancel = cancel;
            ui.lb_conent.text = content;
            ui.lb_title.text = title;
            return ui;
        }
    }
}