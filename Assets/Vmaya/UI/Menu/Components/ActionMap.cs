using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Vmaya.UI.Menu
{
    public class ActionMap : MonoBehaviour, IActionCaller
    {

        [SerializeField]
        private List<ActionItem> items;

        protected UnityEvent findAction(string name)
        {
            int index = indexOf(name);
            if (index > -1)
            {
                if (items[index].action != null)
                    return items[index].action;
                else Debug.LogError("Action must not be null " + items[index].name);
            }
            return null;
        }

        public virtual void Call(string name)
        {
            UnityEvent action = findAction(name);
            if (action != null)
                action.Invoke();
        }

        public int indexOf(string actionName)
        {
            for (int i=0; i<items.Count; i++)
                if (items[i].name.Equals(actionName))
                    return i;

            return -1;
        }

        public int addAction(ActionItem item)
        {
            items.Add(item);
            return items.Count - 1;
        }

        public int addAction(string name, UnityEvent a_event)
        {
            ActionItem item;
            item.name = name;
            item.action = a_event;
            items.Add(item);
            return items.Count - 1;
        }

        public void removeAction(string name)
        {
            for (int i=0; i<items.Count; i++)
                if (items[i].name.Equals(name))
                {
                    items.RemoveAt(i);
                    return;
                }
        }
    }
}