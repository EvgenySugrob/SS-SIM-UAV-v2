using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vmaya.Scene3D;

namespace Vmaya.UI.Menu
{
    public class PopupMenuProvider : MenuItemList
    {
        [SerializeField]
        private int _mouseButtonCode;
        public int MouseButtonCode => _mouseButtonCode;

        [SerializeField]
        private PopupMenu _popupMenu;
        protected PopupMenu PopupMenu => _popupMenu ? _popupMenu : _popupMenu = FindObjectOfType<PopupMenu>(true);

        private void Awake()
        {
            HitMouse hmouse = GetComponent<HitMouse>();
            if (hmouse) hmouse.onClick.AddListener(doClick);
        }

        private void doClick(baseHitMouse hit)
        {
            if (VMouse.GetMouseButtonUp(_mouseButtonCode))
                Show();
        }

        public void Show()
        {
            PopupMenu.Show(VMouse.mousePosition, this, GetComponent<IActionCaller>(), GetComponent<IAvailableProvider>());
        }

        public void Show(IAvailableProvider aprovider)
        {
            PopupMenu.Show(VMouse.mousePosition, this, GetComponent<IActionCaller>(), aprovider);
        }

        public void Show(IActionCaller caller, IAvailableProvider aprovider)
        {
            PopupMenu.Show(VMouse.mousePosition, this, caller, aprovider);
        }
    }
}
