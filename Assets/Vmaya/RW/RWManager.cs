using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vmaya.RW
{
    public class RWManager: RWEvents, INameProvider
    {
        private List<IRW> _listeners;

        private int currentIndex;
        private static float waitTimeStep = 0.04f;

        [System.Serializable]
        public class dataList
        {
            [SerializeField]
            public List<dataRecord> children;

            public dataList()
            {
                children = new List<dataRecord>();
            }
        }

        virtual protected void Awake()
        {
            currentIndex = 0;
            _listeners = new List<IRW>();
        }

        public void addListener(IRW listener)
        {
            if ((listener != this as IRW) && (_listeners.IndexOf(listener) == -1))  
                _listeners.Add(listener);
        }

        public void removeListener(IRW listener)
        {
            if (_listeners != null) _listeners.Remove(listener);
        }

        public string checkNextName(string checkName)
        {
            /*
            string[] a = checkName.Split('-', ' ');
            if (a.Length > 1)
            {
                int num;
                int.TryParse(a[a.Length - 1], out num);
                if (num > 0) return checkName;
            }*/

            string result = Indent.GetOrigin(checkName) + '-' + currentIndex.ToString();
            currentIndex++;
            return result;
        }

        public void reset()
        {
            currentIndex = 0;
        }

        //--------------------------REWRITE METHODS

        override protected void doReadData(dataRecord rec)
        {
        }

        override protected string doWriteData()
        {
            return "";
        }

        protected int indexOf(Indent indent)
        {
            for (int i=0; i<_listeners.Count; i++)
                if (_listeners[i].getIndent().Equals(indent)) return i;

            return -1;
        }

        protected IEnumerator tryDelayReadRecord(Indent indent, dataRecord record, float wtime)
        {
            yield return new WaitForSeconds(wtime);
            int index = indexOf(indent);
            if (index > -1) _listeners[index].readRecord(record);
            else
            {
                if (wtime < 1) StartCoroutine(tryDelayReadRecord(indent, record, wtime + waitTimeStep));
                else Debug.Log("Couldn't find " + indent);
            }
        }

        override protected void doReadRecord(dataRecord rec)
        {
            base.doReadRecord(rec);

            currentIndex = rec.pointer;

            if (!string.IsNullOrEmpty(rec.data))
            {
                dataList list = JsonUtility.FromJson<dataList>(rec.data);

                for (int i = 0; i < list.children.Count; i++)
                {
                    int index = indexOf(list.children[i].indent);
                    if (index != -1) _listeners[index].readRecord(list.children[i]);
                    else StartCoroutine(tryDelayReadRecord(list.children[i].indent, list.children[i], waitTimeStep));
                }
            }
        }

        override protected dataRecord doWriteRecord()
        {
            base.doWriteRecord();

            dataList _dataList = new dataList();

            foreach (IRW rw in _listeners)
                _dataList.children.Add(rw.writeRecord());

            writeBuffer.pointer = currentIndex;
            writeBuffer.data = JsonUtility.ToJson(_dataList);
            return writeBuffer;
        }

        public void ResetNames()
        {
            for (int i = 0; i < _listeners.Count; i++)
            {
                Component comp = _listeners[i] as Component;
                if (comp) comp.name = checkNextName(comp.name);
            }
        }
    }
}