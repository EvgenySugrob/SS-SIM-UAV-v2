using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vmaya.UI.Menu;
using Vmaya.UI.Users;

namespace Vmaya.UI.Menu
{
    public class MenuItemToggle : MenuItem
    {
        [SerializeField]
        private Toggle _toggle;
        private bool isToggle => (data != null) && (actionToggleList != null) ? actionToggleList.Contains(data.action) : false;
        private float toggleWidth => isToggle ? _toggle.GetComponent<RectTransform>().rect.width : 0;

        public static List<string> actionToggleList;

        protected override void Start()
        {
            if (isToggle)
            {
                _toggle.gameObject.SetActive(true);
                _toggle.onValueChanged.AddListener(onToggleChange);
                _button.interactable = false;
            }
            else base.Start();
        }

        private void onToggleChange(bool value)
        {
            (topMenu.ActionMap as ActionMapTg).callAsToogle(data.action, value);
        }

        protected override void afterChangeInteractable()
        {
            base.afterChangeInteractable();
            if (isToggle)
                _button.interactable = false;
        }

        public override float calcWidth()
        {
            return base.calcWidth() + toggleWidth;
        }
    }
}
