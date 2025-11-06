using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems; // Required when using Event data.
using Vmaya.Scene3D;
using static UnityEngine.EventSystems.PointerEventData;

namespace Vmaya.UI.Components
{
    public class ResizePanel : UIComponent, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
    {
        [SerializeField]
        private Vector2 _minSize;
        [SerializeField]
        private InputButton mouseButton;
        [SerializeField]
        [Tooltip("The size of the active area on the window border")]
        private float _border = 6;
        public Vector2 MinSize => getMinSize();

        private WindowManager windowsManager => GetComponentInParent<WindowManager>();

        [System.Serializable]
        public class RectEvent : UnityEvent<Rect>
        {
        }

        public RectEvent onResize;

        private bool _focus;
        private bool _down;
        private Vector2 _prevPos;

        private bool left;
        private bool right;
        private bool top;
        private bool bottom;

        private void OnValidate()
        {
            if (enabled && _minSize.sqrMagnitude == 0)
                _minSize = GetComponent<RectTransform>().sizeDelta;
        }

        protected virtual Vector2 getMinSize()
        {
            return _minSize;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (enabled && !_down && !VKeyboard.anyKey)
            {
                _focus = true;
                checkEdge();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_down)
            {
                _focus = false;
                setCursor(null);
            }
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == mouseButton)
            {
                _down = true;
                _prevPos = VMouse.mousePosition;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _down = false;
        }

        private void checkEdge()
        {
            RectTransform mrect = GetComponent<RectTransform>();

            Rect mRect = mrect.rect;

            Vector2 mpos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(mrect, VMouse.mousePosition, null, out mpos);

            left   = Utils.Between(mRect.x, mRect.x + _border, mpos.x);
            right  = Utils.Between(mRect.xMax - _border, mRect.xMax, mpos.x);
            bottom = Utils.Between(mRect.yMax - _border, mRect.yMax, mpos.y);
            top    = Utils.Between(mRect.y, mRect.y + _border, mpos.y);

            if (left && top) setCursor("ht_lbrt");
            else if (right && bottom) setCursor("ht_lbrt");
            else if (right && top) setCursor("ht_ltrb");
            else if (left && bottom) setCursor("ht_ltrb");
            else if (left || right) setCursor("ht_hory");
            else if (top || bottom) setCursor("ht_vert");
            else setCursor(null);
        }

        private void resize(float leftDelta, float topDelta, float rightDelta, float bottomDelta)
        {
            RectTransform mrect = GetComponent<RectTransform>();

            Rect inc = new Rect(leftDelta, topDelta, rightDelta - leftDelta, bottomDelta - topDelta);
            
            if (mrect.rect.width + inc.width < MinSize.x) {
                inc.width = 0;
                inc.x = 0;
            }

            if (mrect.rect.height + inc.height < MinSize.y)
            {
                inc.height = 0;
                inc.y = 0;
            }

            doResizeAddRect(inc);

            if (inc.size.sqrMagnitude > 0) onResize.Invoke(inc);
        }

        protected virtual void doResizeAddRect(Rect inc)
        {
            Rect rect = GetScreenCoordinates();

            rect.width += inc.width;
            rect.x += inc.x;
            rect.height += inc.height;
            rect.y += inc.y;

            if (windowsManager && windowsManager.Restrict)
            {
                Rect wrect = windowsManager.GetScreenCoordinates();
                if (rect.min.x < wrect.min.x)
                {
                    float dif = wrect.min.x - rect.min.x;
                    rect.x      += dif;
                    rect.width  -= dif;
                }
                else if (rect.max.x > wrect.max.x)
                    rect.width -= rect.max.x - wrect.max.x;

                if (rect.min.y < wrect.min.y)
                {
                    float dif = wrect.min.y - rect.min.y;
                    rect.y += dif;
                    rect.height -= dif;
                }
                else if (rect.max.y > wrect.max.y)
                    rect.height -= rect.max.y - wrect.max.y;
            }

            SetWorldRect(rect);
        }

        private void Update()
        {
            if (_down)
            {
                Vector2 pos = VMouse.mousePosition;
                if (!pos.Equals(_prevPos))
                {
                    Vector2 delta = pos - _prevPos;

                    resize(left ? delta.x : 0, top ? delta.y : 0, right ? delta.x : 0, bottom ? delta.y : 0);

                    _prevPos = pos;
                }
            } else if (_focus) checkEdge();
        }
    }
}