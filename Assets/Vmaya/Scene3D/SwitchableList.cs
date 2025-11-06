using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Vmaya.Scene3D
{
    public class SwitchableList
    {
        private Dictionary<ISwitchable, Dictionary<Object, bool>> _list = new Dictionary<ISwitchable, Dictionary<Object, bool>>();

        private static SwitchableList _instance;

        public bool this[ISwitchable index] => isOn(index);

        public static SwitchableList instance => (_instance != null) ? _instance : _instance = new SwitchableList();

        public bool isOn(ISwitchable index, Object Initiator)
        {
            if (_list.ContainsKey(index) && _list[index].ContainsKey(Initiator))
                return _list[index][Initiator];

            return false;
        }

        internal bool isOn(ISwitchable index)
        {
            if (_list.ContainsKey(index))
                foreach (bool value in _list[index].Values)
                    if (value) return true;

            return false;
        }

        private void CleanNull()
        {
            List<ISwitchable> kyes = new List<ISwitchable>(_list.Keys);

            foreach (ISwitchable index in kyes)
                if (Utils.IsDestroyed(index as Component))
                    _list.Remove(index);
                else
                {
                    List<Object> i_kyes = new List<Object>(_list[index].Keys);
                    foreach (Object Initiator in i_kyes)
                        if (Utils.IsDestroyed(Initiator as Component))
                            _list[index].Remove(Initiator);
                }
        }

        public static void AddSwitchable(ISwitchable index, bool value)
        {
            AddSwitchable(index, index as Object, value);
        }

        public static void AddSwitchable(ISwitchable index, Object Initiator, bool value)
        {
            if ((instance != null) && !Utils.IsDestroyed(index as Component))
            {
                instance.CleanNull();

                if (!instance._list.ContainsKey(index)) 
                    instance._list[index] = new Dictionary<Object, bool>();

                if (instance._list[index].ContainsKey(Initiator) &&
                    (instance._list[index][Initiator] == value))
                    return;

                instance._list[index][Initiator] = value;
                index.setOn(instance[index]);
            }
        }

        internal void Clear<T>()
        {
            List<ISwitchable> kyes = new List<ISwitchable>(_list.Keys);

            foreach (ISwitchable index in kyes)
                if (index is T)
                {
                    _list.Remove(index);
                    if (!Utils.IsDestroyed(index as Component))
                        index.setOn(false);
                }
        }
        /*
public void Begin()
{
   _list.Clear();
   _collectionState = true;
}

public void Release()
{
   _collectionState = false;

   List<ISwitchable> items = new List<ISwitchable>(_list.Keys);

   foreach (ISwitchable item in items)
       item.setOn(this[item]);
   _list.Clear();
}*/
    }
}