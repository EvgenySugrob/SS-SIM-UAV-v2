using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Vmaya.Util
{
    public class ActionAvMap : MonoBehaviour, IActionCaller, IAvailableProvider
    {

        [System.Serializable]
        public class CheckItem
        {
            public Component provider;
            public bool invert;
        }

        [System.Serializable]
        public class ActionItem
        {
            public string name;
            public UnityEvent action;
            public List<CheckItem> checkItems;
        }

        [SerializeField]
        private List<ActionItem> _items;

        private void OnValidate()
        {
            for (int i = 0; i < _items.Count; i++)
                if (_items[i].checkItems != null)
                {
                    for (int n = 0; n < _items[i].checkItems.Count; n++)
                    {
                        Component pc = _items[i].checkItems[n].provider;
                        if (pc && !(pc is IExecutable))
                            _items[i].checkItems[n].provider = pc.GetComponent<IExecutable>() as Component;
                    }
                }
        }

        public virtual void Call(string name)
        {
            int index = indexOf(name);
            if ((index > -1) && GetAvailable(index))
            {
                if (_items[index].action != null)
                    _items[index].action.Invoke();
                else Debug.LogError("Action must not be null " + _items[index].name);
            }
        }

        public int indexOf(string actionName)
        {
            for (int i = 0; i < _items.Count; i++)
                if (_items[i].name.Equals(actionName))
                    return i;

            return -1;
        }

        protected bool GetAvailable(int idx)
        {
            bool result = true;
            if (idx > -1)
            {
                for (int i = 0; i < _items[idx].checkItems.Count; i++)
                {
                    CheckItem ci = _items[idx].checkItems[i];
                    IExecutable executable = ci.provider as IExecutable;
                    result = result && (ci.invert ? !executable.getPerformed() : executable.getPerformed());
                }
            }
            return result;
        }

        public bool GetAvailable(string actionName)
        {
            return GetAvailable(indexOf(actionName));
        }
    }
}
