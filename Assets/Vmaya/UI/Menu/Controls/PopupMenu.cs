using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Vmaya.UI.Users;

namespace Vmaya.UI.Menu
{
    public class PopupMenu : BaseMenu, IPointerEnterHandler, IPointerExitHandler
    {
        private bool _focus;
        private IActionCaller _actionCaller;
        private IAvailableProvider _availableProvider;
        private IMenuList _menuListProvider;

        private SubMenuLayer _layer => GetComponent<SubMenuLayer>();

        private void Show(Vector2 pos, IActionCaller actionCaller, IAvailableProvider availableProvider)
        {
            if (_layer) _layer.Show();
            else gameObject.SetActive(true);

            PrepareItems(refreshItems(UIPlayer.instance ? UIPlayer.instance.User : null), null, 0);
            Trans.position = pos;
            _actionCaller = actionCaller;
            _availableProvider = availableProvider;
        }

        public void Show(Vector2 pos, IMenuList a_menuList, IActionCaller actionCaller, IAvailableProvider availableProvider)
        {
            _menuListProvider = a_menuList;
            Show(pos, actionCaller, availableProvider);
        }

        public void Show(Vector2 pos, Component component)
        {
            Show(pos, component.GetComponent<IMenuList>(), component.GetComponent<IActionCaller>(), component.GetComponent<IAvailableProvider>());
        }

        protected override IActionCaller getActionMap()
        {
            return _actionCaller != null ? _actionCaller : base.getActionMap();
        }

        public void Hide()
        {
            if (_layer && (SubMenuLayer.CurrentSubmenu == _layer)) 
                SubMenuLayer.HideCurrentSubmenu();
            else gameObject.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _focus = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _focus = false;
            Vmaya.Utils.setTimeout(this, () =>
            {
                if (!_focus) Hide();
            }, 0.3f);
        }

        protected override IAvailableProvider getAvailableProvider()
        {
            return _availableProvider;
        }

        protected override IMenuList getItemListProvider()
        {
            return _menuListProvider;
        }
    }
}
