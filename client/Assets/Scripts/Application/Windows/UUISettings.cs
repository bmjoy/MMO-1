using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UGameTools;

namespace Windows
{
    partial class UUISettings
    {

        protected override void InitModel()
        {
            base.InitModel();

            ButtonClose.onClick.AddListener(() => { HideWindow(); });
            ButtonExit.OnMouseClick((o) => { UApplication.S.GotoLoginGate(); HideWindow(); });
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
    }
}