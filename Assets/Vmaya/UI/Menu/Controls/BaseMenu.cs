using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Vmaya.Command;
using Vmaya.UI.Components;
using Vmaya.UI.Menu.Command;
using Vmaya.UI.Users;

namespace Vmaya.UI.Menu
{
    public abstract class BaseMenu : MonoBehaviour
    {
        public MenuItem templateItem;
        public Transform menuLayer;

        private bool _isInteractive = true;
        public bool isInteractive => _isInteractive;
        public IAvailableProvider availableProvider => getAvailableProvider();
        public IMenuList itemListProvider => getItemListProvider();

        protected RectTransform Trans => GetComponent<RectTransform>();
        public IActionCaller ActionMap => getActionMap();

        private User _user;

        protected virtual IActionCaller getActionMap()
        {
            return GetComponent<IActionCaller>();
        }

        protected abstract IAvailableProvider getAvailableProvider();
        protected abstract IMenuList getItemListProvider();

        public MenuItem[] refreshItems(User user)
        {
            _user = user;
            MenuItem.updateItems(_user, menuLayer, itemListProvider.Get(), templateItem);
            MenuItem[] bList = menuLayer.GetComponentsInChildren<MenuItem>();
            Vmaya.Utils.afterEndOfFrame(this, RefreshAvailable);
            return bList;
        }

        public void RefreshAvailable()
        {
            MenuItem[] bList = menuLayer.GetComponentsInChildren<MenuItem>();
            foreach (MenuItem item in bList) menuItemEnableRefresh(item);
        }

        private void menuItemEnableRefresh(MenuItem item)
        {
            item.enabled = _isInteractive;
            if (availableProvider != null)
                item.Interactable = _isInteractive && availableProvider.GetAvailable(item.data.action);
            else item.Interactable = _isInteractive;
        }

        public void setInteractive(bool value)
        {
            if (_isInteractive != value)
            {
                _isInteractive = value;
                MenuItem[] bList = menuLayer.GetComponentsInChildren<MenuItem>();
                foreach (MenuItem item in bList) menuItemEnableRefresh(item);
            }
        }

        private void Update()
        {
            MenuItem[] childen = GetComponentsInChildren<MenuItem>(true);
            if (!Curtain.isModal && !Vmaya.Scene3D.UI.InputControl.isFocus)
            {
                foreach (MenuItem child in childen)
                    if (child.isShortcut && child.checkShortCutDown())
                        child.execAction();
            }

            if (VMouse.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                SubMenuLayer.HideCurrentSubmenu();
        }

        public static void PrepareItems(MenuItem[] items, IAvailableProvider availableProvider, float MinWidth)
        {
            MSCommandManager cm = CommandManager.instance as MSCommandManager;

            float maxWidth = MinWidth;
            if (cm) foreach (MenuItem item in items) cm.resetMenuItem(item);

            foreach (MenuItem item in items)
            {
                if (availableProvider != null)
                    item.Interactable = availableProvider.GetAvailable(item.data.action);

                maxWidth = Mathf.Max(item.calcWidth(), maxWidth);
            }
            foreach (MenuItem item in items) item.setWidth(maxWidth);
        }
    }
}
