using System.Collections.Generic;
using UnityEngine;
using Vmaya.Command;
using Vmaya.Entity;
using Vmaya.UI.Users;

namespace Vmaya.UI.Menu
{
    public class MainMenu : BaseMenu
    {
        [SerializeField]
        private Component _availableProvider;

        [SerializeField]
        private Component _itemListProvider;

        //Устаревший, вместо этого списка исользовать itemListProvider
        [HideInInspector]
        public List<MenuItemData> items;

        public ActionMap actionMap;

        private void OnValidate()
        {
            //Migration to itemListProvider
            if (getItemListProvider() == null)
            {
                MenuItemList list = gameObject.AddComponent<MenuItemList>();
                list.Set(items);
                _itemListProvider = list as Component;
            }

            _availableProvider = availableProvider as Component;
            _itemListProvider = itemListProvider as Component;
        }

        protected override IActionCaller getActionMap()
        {
            return actionMap;
        }

        protected override IAvailableProvider getAvailableProvider()
        {
            return _availableProvider ? _availableProvider.GetComponent<IAvailableProvider>() : null;
        }

        protected override IMenuList getItemListProvider()
        {
            return _itemListProvider ? _itemListProvider.GetComponent<IMenuList>() : null;
        }

        public void refreshItems()
        {
            refreshItems(UIPlayer.instance ? UIPlayer.instance.User : null);
        }

        private void Start()
        {
            AppManager.afterStart(() =>
            {
                refreshItems();
                if (CommandManager.instance)
                {
                    CommandManager.instance.onStartExecuteList.AddListener(onChangeCommandList);
                    CommandManager.instance.onEndExecuteList.AddListener(onChangeCommandList);
                }
            });
        }

        private void onChangeCommandList()
        {
            setInteractive(!CommandManager.isCommandFast);
        }

        public void InsertItem(int index, MenuItemData item)
        {
            items.Insert(index, item);
            refreshItems();
        }

        public void RemoveItem(MenuItemData item)
        {
            int idx = items.IndexOf(item);
            if (idx > -1)
            {
                items.RemoveAt(items.IndexOf(item));
                refreshItems();
            }
        }
    }
}