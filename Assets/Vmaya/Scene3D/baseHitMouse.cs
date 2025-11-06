using UnityEngine;

namespace Vmaya.Scene3D
{
    public class baseHitMouse : MonoBehaviour, ISwitchable
    {
        public int[] MouseButtonsCodes = new int[] { 0 };
        private Vector2 downPosition;

        public Selectable selectable;
        public int priority = 0;

        private void OnValidate()
        {
            baseHitMouse[] list = GetComponents<baseHitMouse>();
            if (list.Length > 1)
                Debug.LogError("Hit mouse component must be alone [" + new Indent(this) + "]");
        }

        private void Awake()
        {
            if (!selectable) selectable = GetComponent<Selectable>();
        }

        public Vector3 calcNewPosition(Vector3 pNormal, Vector3 pPos)
        {
            float enter;

            Ray ray = mRay();
            Plane plane = new Plane(pNormal, pPos);
            plane.Raycast(ray, out enter);

            return ray.GetPoint(enter);
        }

        protected Camera getCamera()
        {
            return CameraManager.getCurrent();
        }

        public Vector2 screenPosition(Canvas canvas)
        {
            if (!canvas)
                canvas = GetComponentInParent<Canvas>();
            if (canvas)
            {
                var viewport_position = getCamera().WorldToViewportPoint(transform.position);
                var canvas_rect = canvas.GetComponent<RectTransform>();

                return new Vector2((viewport_position.x * canvas_rect.sizeDelta.x) - (canvas_rect.sizeDelta.x * 0.5f),
                                   (viewport_position.y * canvas_rect.sizeDelta.y) - (canvas_rect.sizeDelta.y * 0.5f));
            }

            return Vector2.zero;
        }

        public Ray mRay()
        {
            return getCamera().ScreenPointToRay(VMouse.mousePosition);
        }

        virtual protected void doMouseDrag()
        {

        }

        public bool isDown => hitDetector.Down == this;
        virtual protected void Update()
        {
            if (hitDetector.Down == this) doMouseDrag();
        }

        virtual public void doMouseDown()
        {
            downPosition = VMouse.mousePosition;
        }


        virtual public void doMouseUp()
        {
            if (hitDetector.Down == this)
            {
                Vector2 up = VMouse.mousePosition;

                if ((up - downPosition).magnitude < 3)
                    doClick();
            }
        }

        virtual public void doOver()
        {
        }

        virtual public void doOut()
        {
        }

        virtual public void doClick()
        {

        }

        virtual public bool isDrag()
        {
            return false;
        }

        protected void setSelectableEnabled(bool value)
        {
            if (selectable) selectable.enabled = value;
        }

        private void OnEnable()
        {
            setSelectableEnabled(true);
        }

        private void OnDisable()
        {
            setSelectableEnabled(false);
        }

        public void setOn(bool value)
        {
            enabled = value;
        }

        public virtual bool isMouseDown()
        {
            foreach (int code in MouseButtonsCodes)
                if (VMouse.GetMouseButtonDown(code)) 
                    return true;

            return false;
        }

        public virtual bool isMouseUp()
        {
            foreach (int code in MouseButtonsCodes)
                if (VMouse.GetMouseButtonUp(code)) return true;

            return false;
        }
    }
}