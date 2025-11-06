using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Vmaya.Styles
{
    [ExecuteInEditMode]
    public class StyleList : MonoBehaviour
    {
        [SerializeField]
        private Canvas _target;

        [SerializeField]
        private int _currentGroup;
        private int _prevCurrentGroup = -1;

        [SerializeField]
        public List<StyleGroup> Groups;

        [SerializeField]
        private bool _showAllClasses;

        private static StyleList _instance;
        public static StyleList Instance => _instance ? _instance : _instance = FindObjectOfType<StyleList>();

        public UnityEvent OnChangeCurrentGroup;
        public int currentStyleGroupId { get => _currentGroup; set => setCurrentGroup(value); }

        private void OnValidate()
        {
            if (_showAllClasses)
            {
                ShowAllClasses();
                _showAllClasses = false;
            }

            if (!_target) _target = GetComponentInParent<Canvas>();
        }

        private void Update()
        {
            if (_prevCurrentGroup != _currentGroup)
                setCurrentGroup(_currentGroup);
        }

        private void setCurrentGroup(int index)
        {
            if (index != _prevCurrentGroup)
            {
                _prevCurrentGroup = _currentGroup = index;
                OnChangeCurrentGroup.Invoke();
            }
        }

        public StyleItem GetItem(string Class)
        {
            if (_currentGroup > -1)
            {
                foreach (StyleItem item in Groups[_currentGroup].Items)
                    if (item.Class.Equals(Class)) return item;
            }
            return default;
        }

        public void SetItem(string Class, StyleItem value)
        {
            if (_currentGroup > -1)
            {
                for (int i = 0; i < Groups[_currentGroup].Items.Count; i++)
                    if (Groups[_currentGroup].Items[i].Class.Equals(Class))
                    {
                        Groups[_currentGroup].Items[i] = value;
                        Apply();
                        break;
                    }
            }
        }

        internal StyleButtonItem GetItemButton(string Class)
        {
            if (_currentGroup > -1)
            {
                foreach (StyleButtonItem item in Groups[_currentGroup].Buttons)
                    if (item.Class.Equals(Class)) return item;
            }
            return default;
        }

        internal void SetItemButton(string Class, StyleButtonItem value)
        {

            if (_currentGroup > -1)
            {
                for (int i = 0; i < Groups[_currentGroup].Buttons.Count; i++)
                    if (Groups[_currentGroup].Buttons[i].Class.Equals(Class))
                    {
                        Groups[_currentGroup].Buttons[i] = value;
                        Apply();
                        break;
                    }
            }
        }

        private void ShowAllClasses()
        {
            IStyleElement[] elems = _target.GetComponentsInChildren<IStyleElement>(true);
            List<string> names = new List<string>();
            foreach (IStyleElement elem in elems)
            {
                if (!names.Contains(elem.Class())) names.Add(elem.Class());
            }

            foreach (string name in names)
                Debug.Log("Class: " + name);
        }

        public int IndexOf(string NameGroup)
        {
            for (int i = 0; i < Groups.Count; i++)
                if (Groups[i].NameGroup.Equals(NameGroup)) 
                    return i;

            return -1;
        }

        public void Apply()
        {
            if (_currentGroup < Groups.Count)
                Apply(_target.transform, _currentGroup);
        }

        public void Apply(Transform root)
        {
            if (_currentGroup < Groups.Count) 
                Apply(root, _currentGroup);
        }

        public void Apply(Transform root, string NameGroup)
        {
            int idxg = IndexOf(NameGroup);
            Apply(root, idxg);
        }

        public void Apply(IStyleElement elem)
        {
            Apply(elem, _currentGroup);
        }

        public void Apply(IStyleElement elem, int IndexGroup)
        {
            OnChangeCurrentGroup.RemoveListener(elem.OnChangeGroup);
            OnChangeCurrentGroup.AddListener(elem.OnChangeGroup);

            if ((IndexGroup > -1) && (IndexGroup < Groups.Count))
            {
                if (elem is StyleButton)
                {
                    foreach (StyleButtonItem item in Groups[IndexGroup].Buttons)
                    {
                        if (item.Class.Equals(elem.Class()))
                        {
                            elem.ApplyStyle(item);
                            return;
                        }
                    }
                }

                foreach (StyleItem item in Groups[IndexGroup].Items)
                {
                    if (item.Class.Equals(elem.Class()))
                    {
                        elem.ApplyStyle(item);
                        return;
                    }
                }
            }

            Debug.Log((new Indent(elem as Component)) + " Class: " + elem.Class() + " not found!");
        }

        public void Apply(Transform root, int IndexGroup)
        {
            if ((IndexGroup > -1) && (Groups[IndexGroup].Items.Count > 0)) {
                IStyleElement[] elems = root.GetComponentsInChildren<IStyleElement>(true);
                foreach (IStyleElement elem in elems) Apply(elem, IndexGroup);
            }
        }

        public void NextStyle()
        {
            if (Groups.Count > 1)
                setCurrentGroup((_currentGroup + 1) % Groups.Count);
        }
    }
}
