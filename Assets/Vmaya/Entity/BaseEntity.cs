using UnityEngine;
using UnityEngine.Events;
using Vmaya.RW;
using static Vmaya.RW.RWEvents;
using static Vmaya.RW.RWManager;

namespace Vmaya.Entity
{
    public class BaseEntity : MonoBehaviour, IRemovable, IJsonObject, ICopyable
    {
        private bool _active = true;

        [HideInInspector]
        public string sourceName;

        [HideInInspector]
        public UnityEvent onDocked;

        public bool Selected { get => AppManager.instance.currentSelected == this; }

        [System.Serializable]
        public class entityRecord : restoreData
        {
            public string indent;
            public string sourceName;
            public Vector3 position;
            public Quaternion rotation;
            public string json_params;
        }

        virtual protected void Awake()
        {
            if (!isInList())
            {
                if (onDocked == null) onDocked = new UnityEvent();
                AppManager.afterStart(() =>
                {
                    AppManager.instance.onCurrentSelected.AddListener(onCurrentSelected);
                });
            }
        }

        private void onCurrentSelected(BaseEntity entity)
        {
            if (entity == this) doSelected();
            else if (Selected) doUnselected();
        }

        protected virtual void doUnselected()
        {
        }

        protected virtual void doSelected()
        {
        }

        virtual protected void Start()
        {
            if (!isInList()) updateActive();
        }

        public bool Active
        {
            get => getActive();
            set => setActive(value);
        }

        public void setActive(bool value)
        {
            if (value != _active)
            {
                _active = value;
                updateActive();
            }
        }

        public bool getActive()
        {
            return _active && gameObject.activeSelf;
        }

        virtual protected void updateActive()
        {
            gameObject.SetActive(_active);
        }

        virtual public bool isSelectable()
        {
            return _active;
        }

        virtual internal void Dock()
        {
            onDocked.Invoke();
        }

        virtual public bool possibleRemove()
        {
            return true;
        }

        public virtual void Restore(restoreData data)
        {
            Active = data.active;
        }

        public virtual void Delete()
        {
            Active = false;
        }

        public virtual restoreData getRestoreData()
        {
            restoreData result = new restoreData();
            result.active = Active;
            return result;
        }

        public virtual void Destroy(restoreData data = null)
        {
            if (!Utils.IsDestroyed(this))
                Destroy(gameObject);
        }

        public virtual entityRecord GetRecord()
        {
            entityRecord r = new entityRecord();
            r.indent = name;
            r.sourceName = sourceName;
            r.json_params = getJson();
            r.position = transform.localPosition;
            r.rotation = transform.localRotation;
            r.active = Active;
            return r;
        }

        public virtual void SetRecord(entityRecord record)
        {
            name = record.indent;
            sourceName = record.sourceName;
            setJson(record.json_params);
            transform.localPosition = record.position;
            transform.localRotation = record.rotation;
            Active = record.active;
        }

        virtual public string getJson()
        {
            return "";
        }

        virtual public void setJson(string json_params)
        {
        }

        virtual public void asPreview()
        {
            Bounds bounds = Utils.getRBounds(transform);
            float scale = 1 / Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            transform.localScale = new Vector3(scale, scale, scale);

            Transform center = Utils.FindChild(transform, "center");
            if (center) transform.localPosition = -transform.InverseTransformPoint(center.position) * scale;
        }

        public virtual BaseEntity Owner()
        {
            return this;
        }

        protected virtual bool isInList()
        {
            return false;
        }

        public bool isSelected => AppManager.instance ? AppManager.instance.currentSelected == this : false;

        protected virtual void OnDisable()
        {
            if (isSelected) AppManager.instance.setSelected(null);
        }

        public virtual float dockTime()
        {
            return 0;
        }

        public virtual string getRestoreDataJson()
        {
            return JsonUtility.ToJson(GetRecord());
        }

        public virtual restoreData JsonToRestoreData(string jsonData)
        {
            return JsonUtility.FromJson<entityRecord>(jsonData);
        }

        public virtual ICloneable Clone()
        {
            BaseEntity result = Instantiate(this, transform.parent);

            string baseName = string.IsNullOrEmpty(sourceName) ? name : sourceName;

            INameOwner owner = transform.parent ? transform.parent.GetComponentInParent<INameOwner>() : null;
            if (owner != null) result.name = owner.checkNextName(baseName);
            else result.name = AppManager.checkNextName(baseName);
            return result;
        }

        public virtual ClipboardData Copy()
        {
            dataList _dataList = new dataList();
            IRW[] rws = GetComponents<IRW>();

            if (rws.Length > 0)
            {
                foreach (IRW rw in rws)
                    _dataList.children.Add(rw.writeRecord());
            }
            else
            {
                dataRecord rec = new dataRecord();
                rec.data = JsonUtility.ToJson(GetRecord());
                _dataList.children.Add(rec);
            }


            return new ClipboardData(new Indent(this), !string.IsNullOrEmpty(sourceName) ? sourceName : Indent.GetOrigin(name), JsonUtility.ToJson(_dataList));
        }
    }
}