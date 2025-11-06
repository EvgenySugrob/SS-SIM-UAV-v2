using MyBox;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Vmaya.RW
{
    abstract public class RWEvents : MonoBehaviour, IRW
    {

        [System.Serializable]
        public class dataRecord
        {
            [SerializeField]
            public Indent indent;
            [SerializeField]
            public int pointer;
            [SerializeField]
            public string data;
        }

        protected dataRecord writeBuffer;

        [SerializeField]
        protected RWManager Provider;
        
        [SerializeField]
        private bool _findProvider = true;

        [SerializeField]
        private bool _allowReassignProvider;
        public bool FindProvider => _findProvider;
        public bool AllowReassignProvider => _allowReassignProvider;

        public UnityEvent onBeforeRead;

        private bool _initializeProvider = false;

        [HideInInspector]
        public bool deleted = false;

        virtual protected void Start()
        {
            resetProvider();
        }

        protected virtual void resetProvider()
        {
            if (!Provider)
            {
                if (_findProvider) notSetProvider(transform.parent);
            }
            else initProvider();
        }

        private void notSetProvider(Transform parent)
        {
            void NotFound()
            {
                if (Application.isPlaying && !FindProvider) 
                    Debug.Log("RW provider not found " + this + ", " + name);
            }

            if (parent)
            {
                RWEvents top = parent.GetComponentInParent<RWEvents>();
                if (top == null) top = MyData.instance;

                if (top != null)
                {
                    if (top is RWManager) setProvider(top as RWManager);
                    else
                    {
                        if (top.Provider) setProvider(top.Provider);
                        else notSetProvider(top.transform.parent);
                    }
                }
                else NotFound();
            }
            else NotFound();
        }

        public void setProvider(RWManager a_provider)
        {
            if (Provider) closeProvider();
            Provider = a_provider;
            initProvider();
        }

        public RWManager getProvider()
        {
            return Provider;
        }

        protected void closeProvider()
        {
            if (_initializeProvider && Provider)
            {
                Provider.removeListener(this);
                Provider.onBeforeRead.RemoveListener(onProviderBeforeRead);
                _initializeProvider = false;
            }
        }

        protected void initProvider()
        {
            if (!_initializeProvider)
            {
                Provider.addListener(this);
                Provider.onBeforeRead.AddListener(onProviderBeforeRead);
                _initializeProvider = true;
            }
        }

        public virtual Indent getIndent()
        {
            return Indent.New(this);
        }

        //--------------------------REWRITE METHODS
        abstract protected void doReadData(dataRecord rec);
        abstract protected string doWriteData();
        virtual protected void onProviderBeforeRead()
        {

        }
        //----------------------

        virtual protected dataRecord createRecord()
        {
            return new dataRecord();
        }

        protected dataRecord createRecord(string json)
        {
            dataRecord rec = createRecord();
            JsonUtility.FromJsonOverwrite(json, rec);
            return rec;
        }

        virtual protected void doReadRecord(dataRecord rec)
        {
            onBeforeRead.Invoke();
            doReadData(rec);
        }

        virtual protected dataRecord doWriteRecord()
        {
            createDataBuffer();
            writeBuffer.data = doWriteData();
            return writeBuffer;
        }

        public dataRecord createDataBuffer()
        {
            writeBuffer         = createRecord();
            writeBuffer.indent  = getIndent();
            writeBuffer.data = "";
            return writeBuffer;
        }

        /*
        private void onReadDataA(dataRecord rec)
        {
            if ((rec.indent == getIndent()) && (rec.data.Length > 0))
            {
                doReadRecord(rec);
            }
        }

        private void onWriteDataA()
        {
            if (!deleted) doWriteRecord();
        }*/

        public dataRecord writeRecord()
        {
            return doWriteRecord();
        }

        public bool readRecord(dataRecord rec)
        {
            doReadRecord(rec);
            return true;
        }

        public bool readString(string s_data)
        {
            if (!string.IsNullOrEmpty(s_data)) 
                return readRecord(createRecord(s_data));

            return false;
        }

        public string writeData()
        {
            dataRecord rec = doWriteRecord();
            return JsonUtility.ToJson(rec);
        }

        virtual internal void OnDestroy()
        {
            closeProvider();
        }
    }
}