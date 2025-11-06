using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using static UnityEngine.EventSystems.PointerEventData;

namespace Vmaya.UI.Components
{
    public class DragPanel : UIComponent, IPointerDownHandler, IPointerClickHandler
    {
        public RectTransform draggable;
        public InputButton mouseButton;
        public RectTransform Draggable => draggable ? draggable : draggable = GetComponent<RectTransform>();
        protected RectTransform clampRectTransform;
        private RectTransform panelRectTransform;
        private int drag = 0;
        public bool isDrag => drag == 2;
        protected Vector2 pointerOffset;

        public UnityEvent onBeginDrag;
        public UnityEvent onDrag;
        public UnityEvent onEndDrag;

        private WindowManager windowsManager => GetComponentInParent<WindowManager>();

        virtual protected void Awake()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                if (Draggable != null) panelRectTransform = Draggable;
                else panelRectTransform = transform.parent as RectTransform;

                clampRectTransform = windowsManager ? windowsManager.Trans : panelRectTransform.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            }
        }

        virtual public void OnPointerDown(PointerEventData data)
        {
            if (isAllowDrag() && (data.button == mouseButton))
            {
                panelRectTransform.SetAsLastSibling();
                pointerOffset = getPointerOffset(data);
                drag = 1;
            }
        }

        virtual public bool isAllowDrag()
        {
            return true;
        }

        virtual protected Vector2 getPointerOffset(PointerEventData data)
        {
            Vector2 result;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRectTransform, data.position, data.pressEventCamera, out result);

            return result;
        }

        virtual protected void doBeginDrag()
        {
            onBeginDrag.Invoke();
        }

        virtual protected void doDrag()
        {
            onDrag.Invoke();
        }

        virtual protected void doEndDrag()
        {
            onEndDrag.Invoke();
        }

        virtual public void Update()
        {
            if (drag > 0) OnDrag();
        }

        virtual public void OnDrag()
        {
            int mb = 0;
            switch (mouseButton)
            {
                case InputButton.Left:
                    mb = 0;
                    break;
                case InputButton.Right:
                    mb = 2;
                    break;
                case InputButton.Middle:
                    mb = 1;
                    break;
            }

            if (panelRectTransform == null)
                return;

            if (!VMouse.GetMouseButtonUp(mb))
            {
                if (drag == 1)
                {
                    drag = 2;
                    doBeginDrag();
                }
                Vector2 localPointerPosition;

                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(clampRectTransform, VMouse.mousePosition, null, out localPointerPosition))
                {
                    Vector3 newPos = localPointerPosition - pointerOffset;
                    if (windowsManager && windowsManager.Restrict)
                    {
                        Vector3 delta = newPos - panelRectTransform.localPosition;
                        panelRectTransform.localPosition += Utils.ClampDelta(delta, panelRectTransform, clampRectTransform);
                    }
                    else panelRectTransform.localPosition = newPos;
                    doDrag();
                }
            }
            else OnPointerUp();
        }

        public void OnPointerUp()
        {
            if (drag == 2)
            {
                drag = 0;
                doEndDrag();
            }
            else drag = 0;
        }

        virtual public void OnPointerClick(PointerEventData eventData)
        {

        }
    }
}