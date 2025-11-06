using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Vmaya.UI
{
    public class DropdownListFrame : MonoBehaviour
    {
        [System.Serializable]
        public enum OpeningDirection { Auto, Top, Bottom };

        [SerializeField]
        private OpeningDirection _openingDirection;

        [SerializeField]
        private RectTransform _frame;
        public RectTransform Frame => _frame;

        [SerializeField]
        private TMP_Dropdown _dropdownTop;

        [SerializeField]
        private float _space;
        private Canvas _topCanvas => (transform.root != transform) ? transform.root.GetComponent<Canvas>() : null;
        private RectTransformAnim _ranim => Frame.GetComponent<RectTransformAnim>();

        protected struct BackData
        {
            public Transform lastParent;
            public bool isOpen;
        }

        protected static Dictionary<TMP_Dropdown, BackData> backData = new Dictionary<TMP_Dropdown, BackData>();

        private void Start()
        {
            BackData a_backData = default;
            a_backData.isOpen = true;
            a_backData.lastParent = !backData.ContainsKey(_dropdownTop) ? _dropdownTop.transform.parent : backData[_dropdownTop].lastParent;
            backData[_dropdownTop] = a_backData;

            if (_topCanvas)
            {
                if (_topCanvas.transform != _dropdownTop.transform.parent)
                    _dropdownTop.transform.parent = _topCanvas.transform;

                UpdateFrame();
            }
        }

        private void OnDestroy()
        {
            if (backData.ContainsKey(_dropdownTop))
            {
                BackData a_backData = backData[_dropdownTop];
                a_backData.isOpen = false;
                backData[_dropdownTop] = a_backData;

                Vmaya.Utils.setTimeout(_dropdownTop.GetComponent<MonoBehaviour>(), () =>
                {
                    if (!backData[_dropdownTop].isOpen)
                        _dropdownTop.transform.parent = backData[_dropdownTop].lastParent;
                }, _ranim ? _ranim.Duration : 0.1f);
            }
        }

        private void OnDisable()
        {
            UpdateFrame();
        }

        private void UpdateFrame()
        {
            float h;
            bool toOpen = enabled;
            RectTransform Trans = GetComponent<RectTransform>();

            float th = Trans.rect.height + _space;
            RectTransform sdtRect = _dropdownTop.GetComponent<RectTransform>();
            Rect dt_rect = UIComponent.GetScreenCoordinatesTrans(sdtRect);
            bool top = dt_rect.yMin - th < 0;

            if (_ranim)
            {
                Rect op_rect = new Rect(dt_rect.x, dt_rect.y, dt_rect.width, dt_rect.height + th);

                switch (_openingDirection)
                {
                    case OpeningDirection.Auto:
                        if (top) op_rect.y += th;
                        break;
                    case OpeningDirection.Top:
                        Vector3 lp = transform.localPosition;
                        lp.y = -_space;
                        transform.localPosition = lp;
                        op_rect.y += th;
                        break;
                }

                if (toOpen) _ranim.PlayTo(dt_rect, op_rect);
                else _ranim.PlayTo(op_rect, dt_rect);
            }
            else
            {
                h = toOpen ? th : 0;
                Vector2 pos = new Vector2(sdtRect.position.x, sdtRect.position.y);
                switch (_openingDirection)
                {
                    case OpeningDirection.Auto:
                        if (top) pos.y += th;
                        break;
                    case OpeningDirection.Top:
                        Vector3 lp = transform.localPosition;
                        lp.y = -_space;
                        transform.localPosition = lp;
                        pos.y += th;
                        break;
                }
                _frame.sizeDelta = new Vector2(0, h);
                _frame.position = pos;
            }
        }
    }
}
