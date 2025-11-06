using MyBox;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Vmaya.Scene3D
{
    public class DragDrop3d : HitMouse, IHandle, IDragControl
    {
        [System.Serializable]
        public enum DragDropPlane { X, Y, Z, User, Adaptive, Surface };
        public DragDropPlane plane = DragDropPlane.Y;
        [ConditionalField("plane", false, DragDropPlane.User)] public Vector3 userPlaneNormal;
        [SerializeField] private bool _findCollision = true;


        [SerializeField]
        private float _rigidBodyForce;
        public ILimiter limiter;

        [SerializeField]
        private Transform dragable;
        public Transform Dragable => dragable ? dragable : transform;
        public float step = 0;
        public Collider boundsCollider;

        public Vector3 delta { get { return _delta; } }
        private Vector3 prevPos;
        private Vector3 _prevMousePos;
        private Vector3 pos;
        private Vector3 boffset;
        private Vector3 doffset;
        private int drag = 0;
        private Vector3 _delta;

        public HMEvent onDrag;
        public HMEvent onBeginDrag;
        public HMEvent onEndDrag;

        private static DragDrop3d _currentDrag = null;
        public static DragDrop3d currentDrag { get { return _currentDrag; } }

        private Rigidbody _rb;
        private bool _useRbGravity;

        [HideInInspector]
        public bool freeze;

        protected UnityEvent onSetPosition = new UnityEvent();
        private RaycastHit _surface;
        protected RaycastHit surface => _surface;

        virtual protected void Awake()
        {
            if (onDrag == null) onDrag = new HMEvent();
            if (onBeginDrag == null) onBeginDrag = new HMEvent();
            if (onEndDrag == null) onEndDrag = new HMEvent();
            _rb = Dragable.GetComponent<Rigidbody>();
            if (_rb) _useRbGravity = _rb.useGravity;
        }

        virtual protected bool isAllowedDrag()
        {
            return !VKeyboard.GetKey(
#if ENABLE_INPUT_SYSTEM
                Key.LeftAlt
#else
                KeyCode.LeftAlt
#endif
                ) && !EventSystem.current.IsPointerOverGameObject() && !freeze && Cursor.visible;
        }

        public override void doOver()
        {
            if (!VKeyboard.GetKey(
#if ENABLE_INPUT_SYSTEM
                Key.LeftAlt
#else
                KeyCode.LeftAlt
#endif
                ) && !EventSystem.current.IsPointerOverGameObject()) base.doOver();
        }

        override public void doMouseDown()
        {
            if (isAllowedDrag())
            {
                pos = prevPos = getPosition();
                drag = 1;
                if (dragAsBounds())
                {
                    Bounds bpos = boundsCollider.bounds;
                    boffset = pos - bpos.center;
                }

                if (plane == DragDropPlane.Surface)
                {
                    RaycastHit hit;
                    if (nearestSurface(getPosition(), out hit))
                    {
                        _surface = hit;
                        pos = prevPos = _surface.point;
                    }
                }

                doffset = pos - calcNewPos();

                _prevMousePos = VMouse.mousePosition;
            }
        }

        private void prepareRiginBodyForDrag(bool a_enabled)
        {
            if (_rb)
            {
                if (a_enabled) _useRbGravity = _rb.useGravity;

                _rb.useGravity = a_enabled ? false : _useRbGravity;
                if (_rigidBodyForce == 0)
                {
                    _rb.constraints = a_enabled ? (RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ) : 0;
                }
            }
        }

        override public bool isDrag()
        {
            return drag == 2;
        }

        override protected void doMouseDrag()
        {
            if (drag > 0)
            {
                Vector3 v = sroundV(calcNewPos(doffset));
                _delta = v - prevPos;
                if ((_delta.sqrMagnitude > 0) && _findCollision && boundsCollider)
                {
                    Vector3 _newDelta = _delta;
                    Utils.findFutureCollisions(boundsCollider, ref _newDelta);
                    _delta = Vector3.ProjectOnPlane(_newDelta, Normal);
                }

                if (delta.sqrMagnitude > 0)
                {
                    if (isDrag())
                    {
                        if (_rb && (_rigidBodyForce > 0))
                        {
                            _rb.AddForceAtPosition(delta * _rigidBodyForce, transform.TransformPoint(doffset), ForceMode.VelocityChange);
                        }
                        else setFromLimits();
                        doDrag();
                    }
                    prevPos = getPosition();
                }

                float deltaPixs = (_prevMousePos - VMouse.mousePosition).magnitude;
                _prevMousePos = VMouse.mousePosition;

                if ((drag == 1) && (deltaPixs > hitDetector.instance.STARTDRAGTHRESHOLDPIXS))
                {
                    doBeginDrag();
                    drag = 2;
                    _currentDrag = this;
                }
            }
            else doMouseDown(); // Эта строка для того что бы выбирался объект который начали тащить
        }

        override public void doMouseUp()
        {
            if (hitDetector.Down == this)
            {
                if (drag <= 1)
                {
                    doClick();
                    drag = 0;
                }
                else Drop();
            }
        }

        public void Drop()
        {
            if (drag > 1)
            {
                doEndDrag();
                drag = 0;
                _currentDrag = null;
            }
        }

        virtual protected void doDrag()
        {
            if (onDrag != null) onDrag.Invoke(this);
        }

        virtual protected void doBeginDrag()
        {
            if (onBeginDrag != null) onBeginDrag.Invoke(this);
            prepareRiginBodyForDrag(true);
        }

        virtual protected void doEndDrag()
        {
            prepareRiginBodyForDrag(false);
            if (onEndDrag != null) onEndDrag.Invoke(this);
        }

        protected virtual Vector3 calcNewPos(Vector3 offset = default)
        {
            Vector3 result;
            if (plane == DragDropPlane.Surface)
            {
                RaycastHit hitResult;
                if (nearestSurface(getPosition(), out hitResult))
                {

                    result = calcNewPosition(hitResult.normal, hitResult.point);

                    if (nearestSurface(result + offset, out hitResult)) {

                        if (!_surface.normal.Equals(hitResult.normal))
                        {
                            doffset = offset = Vector3.zero;// Quaternion.FromToRotation(_surface.normal, hitResult.normal) * doffset;
                            prevPos = pos = hitResult.point;
                        }
                    
                        _surface = hitResult;
                    } else
                    {
                        result = prevPos - offset;
                    }
                } else
                {
                    result = prevPos - offset;
                }
            }
            else
            {
                Vector3 a_pos = getPosition() - offset;
                result = a_pos;

                DragDropPlane cur_plane = plane;
                if (plane == DragDropPlane.Adaptive)
                {
                    result = calcNewPosition(Normal, a_pos);
                } else if (getCamera().orthographic)
                {
                    if (cur_plane == DragDropPlane.X)
                    {
                        result = calcNewPosition(Vector3.left, new Vector3(a_pos.x, 0, 0));
                        result.x = 0;
                    }
                    else if (cur_plane == DragDropPlane.Z)
                    {
                        result = calcNewPosition(Vector3.back, new Vector3(0, a_pos.y, 0));
                        result.z = 0;
                    }
                    else if (cur_plane == DragDropPlane.Y)
                    {
                        result = calcNewPosition(Vector3.up, new Vector3(a_pos.x, 0, a_pos.z));
                    }
                    result.y = 0;
                }
                else
                {
                    if ((plane == DragDropPlane.User) && (userPlaneNormal.sqrMagnitude != 0))
                    {
                        result = calcNewPosition(userPlaneNormal.normalized, a_pos);
                    }
                    else
                    {
                        if (cur_plane == DragDropPlane.X) result = calcNewPosition(Vector3.left, new Vector3(a_pos.x, 0, 0));
                        else if (cur_plane == DragDropPlane.Y) result = calcNewPosition(Vector3.up, new Vector3(0, a_pos.y, 0));
                        else result = calcNewPosition(Vector3.back, new Vector3(0, 0, a_pos.z));
                    }
                }
            }
            return result + offset;
        }

        private float sround(float v)
        {
            return (step == 0) ? v : (Mathf.Round(v / step) * step);
        }

        virtual protected Vector3 sroundV(Vector3 pos)
        {
            if (plane == DragDropPlane.Surface)
                return pos; // Реализовать потом
            else if (plane == DragDropPlane.User)
            {
                Vector3 lp = transform.parent.InverseTransformPoint(pos);
                lp = new Vector3(sround(lp.x), lp.y, sround(lp.z));
                return transform.parent.TransformPoint(lp);
            }
            return new Vector3((plane == DragDropPlane.Z) || (plane == DragDropPlane.Y) ? sround(pos.x) : pos.x,
                                sround(pos.y),
                                (plane == DragDropPlane.X) || (plane == DragDropPlane.Y) ? sround(pos.z) : pos.z);
        }

        private bool dragAsBounds()
        {
            return (getLimiter() != null) && boundsCollider;
        }

        private ILimiter getLimiter()
        {
            ILimiter l = limiter;
            if (l == null) l = GetComponentInParent<ILimiter>();

            if ((l != null) && (l != GetComponent<ILimiter>())) return l;

            return null;
        }

        public void setFromLimits()
        {
            if (plane == DragDropPlane.Surface)
            {
                pos += delta;
                setPositionOnSurface(sroundV(pos));
            }
            else
            {
                ILimiter l = getLimiter();

                if (l != null)
                {
                    if (boundsCollider)
                    {
                        Bounds bounds = boundsCollider.bounds;
                        bounds.center = pos - boffset;
                        pos = l.checkLimits(bounds, delta) + boffset;
                    }
                    else
                    {
                        Bounds bounds = new Bounds(pos, Vector3.zero);
                        pos = l.checkLimits(bounds, delta);
                    }
                    setPosition(sroundV(pos));
                }
                else
                {
                    pos += delta;
                    setPosition(sroundV(pos));
                }
            }
        }

        virtual public void setPositionOnSurface(Vector3 value)
        {
            setPosition(value, Quaternion.LookRotation(_surface.normal));
        }

        virtual public void setPosition(Vector3 value)
        {
            IPositioned[] iposList = Dragable.GetComponents<IPositioned>();

            foreach (IPositioned ipos in iposList) {
                //if ((ipos as MonoBehaviour).transform != transform) {
                if (ipos as Component != this) {
                    ipos.setPosition(value);
                    onSetPosition.Invoke();
                    return;
                }
            }

            Dragable.position = value;
            onSetPosition.Invoke();
        }

        public virtual void setPosition(Vector3 value, Quaternion rotate)
        {
            transform.rotation = rotate;
            setPosition(value);
        }

        public virtual Vector3 getPosition()
        {
            IPositioned[] iposList = Dragable.GetComponents<IPositioned>();

            foreach (IPositioned ipos in iposList)
                if (ipos as Object != this) return ipos.getPosition();

            return Dragable.position;
        }

        public Quaternion getRotate()
        {
            IPositioned[] iposList = Dragable.GetComponents<IPositioned>();

            foreach (IPositioned ipos in iposList)
                if (ipos as Object != this) return ipos.getRotate();

            return Dragable.rotation;
        }

        public void addListener(UnityAction action)
        {
            onSetPosition.AddListener(action);
        }

        public void removeListener(UnityAction action)
        {
            onSetPosition.RemoveListener(action);
        }

        bool IHandle.isFocus()
        {
            return focus == this;
        }

        public IDraggable getDraggable()
        {
            return Dragable.GetComponent<IDraggable>();
        }

        protected virtual bool isSuitableSurface(RaycastHit hit)
        {
            return true;
        }

        protected bool nearestSurface(Vector3 target, out RaycastHit hitResult)
        {
            float minDistance = float.MaxValue;
            hitResult   = default;
            bool result = false;
            Camera cam = CameraManager.getCurrent();

            Vector3 camPos = cam.transform.position;
            Ray ray = new Ray(camPos, (target - camPos).normalized);
            RaycastHit[] hits = Physics.RaycastAll(ray, float.MaxValue, cam.cullingMask);
            foreach (RaycastHit hit in hits)
                if (!hit.transform.IsChildOf(transform)) {
                    float distance = (cam.transform.position - hit.point).magnitude;
                    if ((minDistance > distance) && isSuitableSurface(hit))
                    {
                        minDistance = distance;
                        hitResult = hit;
                        result = true;
                    }
                }

            return result;
        }

        public Vector3 Normal
        {
            get
            {
                if (plane == DragDropPlane.Surface)
                {
                    RaycastHit hit;
                    if (nearestSurface(getPosition(), out hit)) return hit.normal;
                    else return Vector3.up;

                } else if ((plane == DragDropPlane.User) && (userPlaneNormal.sqrMagnitude != 0)) 
                    return userPlaneNormal.normalized;
                else {
                    if (plane == DragDropPlane.Adaptive) return (Camera.main.transform.position - transform.position).normalized;
                    if (plane == DragDropPlane.X) return Vector3.left;
                    else if (plane == DragDropPlane.Y) return Vector3.up;
                    else return Vector3.back;
                }
            }
        }
    }
}
