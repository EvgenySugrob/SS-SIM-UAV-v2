using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Vmaya.Collections
{
    [System.Serializable]
    public enum FSType { File, Dir, Disk };

    public abstract class BaseFSListSource : MonoBehaviour, IListSource
    {
        [System.Serializable]
        public struct FileRecord
        {
            public FSType type;
            public string name;
            public string path;

            public FileRecord(FSType type, string name, string path)
            {
                this.type = type;
                this.name = name;
                this.path = path;
            }
        }

        private List<FileRecord> _list;

        public FileRecord this[int index] => _list[index];
        public int Count { get { return _list.Count; } }

        public UnityEvent onAfterOpen;

        public void onAfterChange(UnityAction complete)
        {
            onAfterOpen.AddListener(complete);
            Refresh();
        }

        public int getCount()
        {
            return (_list != null) ? _list.Count : 0;
        }

        public virtual string getData(int idx)
        {
            return JsonUtility.ToJson(_list[idx]);
        }

        public string getId(int idx)
        {
            return idx.ToString();
        }

        public virtual string getName(int idx)
        {
            return _list[idx].name;
        }

        public int FindByName(string fileName)
        {
            for (int i = 0; i < _list.Count; i++)
                if (_list[i].name.Equals(fileName))
                    return i;

            return -1;
        }

        public int IndexOf(string id)
        {
            return !string.IsNullOrEmpty(id) ? int.Parse(id) : -1;
        }

        protected void setList(List<FileRecord> a_list)
        {
            _list = a_list;
            onAfterOpen.Invoke();
        }

        public abstract void Refresh();
    }
}