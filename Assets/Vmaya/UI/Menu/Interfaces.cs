using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Vmaya.UI.Users;

namespace Vmaya.UI.Menu
{
    public interface IMenuList
    {
        List<MenuItemData> Get();
        void Set(List<MenuItemData> a_items);
    }

    [System.Serializable]
    public class MenuItemRecord
    {
        public string name;
        public int role;
        public string action;
        public string shortCut;

        public MenuItemRecord(string a_name)
        {
            name = a_name;
            action = a_name;
        }

        public MenuItemRecord(string a_name, string a_action)
        {
            name = a_name;
            action = a_action;
        }
    }

    [System.Serializable]
    public class MenuItemData: MenuItemRecord
    {
        public List<MenuItemRecord> subItems;

        public MenuItemData(string a_name, string a_action) : base(a_name, a_action)
        {
        }

        public MenuItemData(string a_name) : base(a_name)
        {
        }

        public void addSubMenu(string a_name, string a_action)
        {
            if (subItems == null) subItems = new List<MenuItemRecord>();

            subItems.Add(new MenuItemData(a_name, a_action));
        }

        internal void removeSubMenu(string panelName)
        {
            int idx = Find(panelName);
            if (idx > -1) subItems.RemoveAt(idx);
        }

        internal int Find(string panelName)
        {
            if (subItems != null)  
                for (int i = 0; i < subItems.Count; i++)
                    if (subItems[i].name.Equals(panelName)) return i;
            return -1;
        }
    }

    public interface IMenuItem
    {
        void setData(User user, MenuItemRecord data);
    }

    [System.Serializable]
    public struct ActionItem
    {
        public string name;
        public UnityEvent action;
    }

    [System.Serializable]
    public class AvailableActionItem
    {
        public string name;
        public Component execComponent;
        public IExecutable Executable => execComponent != null ? execComponent as IExecutable : null;
    }
}