using UnityEngine;
using UnityEngine.Events;
using Vmaya.Collections;
using static Vmaya.UI.Components.ListViewItem;

namespace Vmaya.UI.Components
{
    public class ListView : UIComponent
    {

        [SerializeField]
        private ListViewItem _itemTemplate;

        private IListSource _source;
        [SerializeField]
        private Component source;

        private string _lastSelectId = "";

        public IListSource Source { get { return _source; } }

        public OnSelectItem onSelectItem;

        public UnityEvent onDblClick;

        public int selectedIndex => Source.IndexOf(_lastSelectId);
        public string selectedId => _lastSelectId;

        void OnValidate() { validateSource(); }

        private void validateSource()
        {
            _source = checkSource(source);
            source = _source as Component;
        }

        protected virtual IListSource checkSource(Component value)
        {
            return (value != null) ? value.GetComponent<IListSource>() : null;
        }

        private void Awake()
        {
            _itemTemplate.gameObject.SetActive(false);
            validateSource();
        }

        private void Start()
        {
            if (Source != null) Source.onAfterChange(onAfterSource);
            else Debug.LogError(Source);
        }

        private void onAfterSource()
        {
            Refresh();
        }

        private void ClearA()
        {
            ListViewItem[] list = GetComponentsInChildren<ListViewItem>();
            foreach (ListViewItem b in list) Destroy(b.gameObject);
        }

        public void Clear()
        {
            ClearA();
            _lastSelectId = "";
        }

        public void Refresh()
        {
            if (gameObject.activeInHierarchy)
            {
                ClearA();

                for (int i = 0; i < Source.getCount(); i++)
                {
                    ListViewItem item = Instantiate(_itemTemplate, transform);
                    item.setData(Source.getId(i), Source.getName(i));
                    item.gameObject.SetActive(true);
                    item.onSelect.AddListener(OnSelectItem);
                    item.onDblClick.AddListener(OnDblClickItem);
                }
            }
        }

        protected virtual void OnSelectItem(string id)
        {
            _lastSelectId = id;
            onSelectItem.Invoke(id);
        }

        protected virtual void OnDblClickItem()
        {
            onDblClick.Invoke();
        }

        private void OnEnable()
        {
            if (Source != null)
            {
                Source.Refresh();
                Refresh();
            }
        }
    }
}