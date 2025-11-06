using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vmaya.UI.Menu
{
    public class MenuItemList : MonoBehaviour, IMenuList
    {
        [SerializeField]
        private List<MenuItemData> _items;

        public List<MenuItemData> Get()
        {
            return _items;
        }

        public void Set(List<MenuItemData> a_items)
        {
            _items = a_items;
        }
    }
}
