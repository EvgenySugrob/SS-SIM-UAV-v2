using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using System.Collections;

namespace Vmaya.UI.Components
{
    public class ListViewItem : UIComponent, ISelectHandler, IPointerClickHandler
    {
        [System.Serializable]
        public class OnSelectItem : UnityEvent<string> { }

        private string _id;
        private string _caption;
        private int _clickCount;

        public string id { get { return _id; } }

        public OnSelectItem onSelect;
        [HideInInspector]
        public UnityEvent onDblClick;

        virtual public void setData(string a_id, string a_caption)
        {
            _id = a_id;
            _caption = a_caption;

            Text text = GetComponentInChildren<Text>();
            if (text) text.text = _caption;

            if (selected) setVisibleAsSelect(true);
        }

        virtual protected void setVisibleAsSelect(bool selected) {}

        public void OnSelect(BaseEventData eventData)
        {
            onSelect.Invoke(_id);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _clickCount++;
            if (_clickCount == 2) OnDblClick();
            else if (_clickCount == 1) StartCoroutine(clearClicks());
        }

        private IEnumerator clearClicks()
        {
            yield return new WaitForSeconds(0.4f);
            _clickCount = 0;
        }

        private void OnDblClick()
        {
            onDblClick.Invoke();
        }

        protected ListView listView
        {
            get
            {
                return transform.parent.GetComponentInParent<ListView>();
            }
        }

        protected bool selected
        {
            get
            {
                return listView ? listView.selectedId == id : false;
            }
        }
    }
}