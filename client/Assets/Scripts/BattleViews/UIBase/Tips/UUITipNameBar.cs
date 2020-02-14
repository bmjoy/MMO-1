using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Tips
{
    [UITipResources("UUITipNameBar")]
    public class UUITipNameBar : UUITip
    {
        protected override void OnCreate()
        {

            text = FindChild<Text>("Text");
        }

        private Text text;

    
        public void SetName(string name)
        {
            text.text = name;
        }
    }

}