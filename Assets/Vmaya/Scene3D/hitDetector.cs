using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.SceneManagement;

namespace Vmaya.Scene3D
{
    public class hitDetector : MonoBehaviour
    {
        private baseHitMouse focus;
        private baseHitMouse down;
        private RaycastHit rhit;
        private baseHitMouse mhit;
        public HitMouse.HMEvent onFocus;
        public HitMouse.HMEvent onUnFocus;
        public HitMouse.HMEvent onClick;

        public int STARTDRAGTHRESHOLDPIXS = 2;

        [HideInInspector]
        public HitMouse.HMEvent onDown = new HitMouse.HMEvent();

        [HideInInspector]
        public HitMouse.HMEvent onUp = new HitMouse.HMEvent();

        private static hitDetector _instance;
        public static hitDetector instance => getInstance();

        public static Ray DropRay;
        public static RaycastHit[] hits;
        private Vector3 _downPositon;

        public static baseHitMouse Down => instance ? instance.down : null;
        public static baseHitMouse Focus => instance ? instance.focus : null;

        public static Camera Camera => Camera.main ? Camera.main : CameraManager.getCurrent();
        public static Vector3 CamPosition => Camera.transform.position;

        public RaycastHit Hit
        {
            get
            {
                return rhit;
            }
        }

        public baseHitMouse Target
        {
            get
            {
                return mhit;
            }
        }

        private void Awake()
        {
            if (_instance) Debug.LogError("There must be one copy [" + name + "]");
            if (onFocus == null) onFocus     = new HitMouse.HMEvent();
            if (onUnFocus == null) onUnFocus = new HitMouse.HMEvent();
            if (onClick == null) onClick = new HitMouse.HMEvent();
        }

        private static hitDetector getInstance()
        {
            if (!_instance)
            {
                _instance = FindObjectOfType<hitDetector>();
                if (!_instance)
                    _instance = SceneManager.GetActiveScene().GetRootGameObjects()[0].AddComponent<hitDetector>();
            }
            return _instance;
        }

        private void checkHitMouse()
        {
            RaycastHit hit;

            Vector2 mp = VMouse.mousePosition;

            float distance = 0;


            if (((isAllowed() && !VKeyboard.GetKey(
#if ENABLE_INPUT_SYSTEM
                Key.LeftAlt
#else
                KeyCode.LeftAlt
#endif
                ))) &&
                (!Cursor.visible || ((mp.x > 0) && (mp.x < Screen.width) && (mp.y > 0) && (mp.y < Screen.height))))
            {

                Ray ray = mRay();

                hits = Physics.RaycastAll(ray, float.MaxValue, Camera.cullingMask);
                baseHitMouse result = null;
                baseHitMouse[] bhms;

                float minDistance = float.MaxValue;
                int priory = 0;
                for (int i = 0; i < hits.Length; i++)
                {
                    bhms = hits[i].transform.GetComponentsInParent<baseHitMouse>();
                    foreach (baseHitMouse bhm in bhms)
                    {
                        if (bhm && bhm.enabled && (priory < bhm.priority)) priory = bhm.priority;
                    }
                }

                for (int i = 0; i < hits.Length; i++)
                {
                    hit = hits[i];
                    bhms = hit.transform.GetComponentsInParent<baseHitMouse>();
                    foreach (baseHitMouse bhm in bhms)
                    {

                        if (((distance == 0) || (hit.distance <= distance)) &&
                            bhm.enabled && (priory == bhm.priority) && (minDistance > hit.distance))
                        {
                            rhit = hit;
                            result = bhm;
                            minDistance = hit.distance;
                        }
                    }
                }
                if (result != null)
                {
                    mhit = result;
                    return;
                }
            }

            if (!(Down && (Down == mhit)))
                mhit = null;
        }

        public static Ray mRay()
        {
            if (Cursor.visible)
                return mRay(VMouse.mousePosition);
            else return new Ray(Camera.transform.position, Camera.transform.forward);
        }

        public static Ray mRay(Vector2 screenPosition)
        {
            return Camera.ScreenPointToRay(screenPosition);
        }

        public baseHitMouse getFocus()
        {
            return focus;
        }

        public static RaycastHit getNearest<T>()
        {
            return getNearest<T>(mRay());
        }

        public static RaycastHit getNearest<T>(Ray ray, int layerMask = -1)
        {
            RaycastHit[] hits;

            if (layerMask != -1)
                hits = Physics.RaycastAll(ray, float.MaxValue, layerMask);
            else hits = Physics.RaycastAll(ray);

            RaycastHit result = new RaycastHit();

            float minDistance = float.MaxValue;
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                if ((hit.transform.GetComponent<T>() != null) && (minDistance > hit.distance))
                {
                    result = hit;
                    minDistance = hit.distance;
                }
            }

            return result;
        }

        public static Vector3 getTerrainPoint()
        {

            Ray ray = mRay();

            RaycastHit[] hits = Physics.RaycastAll(ray);

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].transform.GetComponent<Terrain>())
                    return hits[i].point;
            }

            return Vector3.zero;
        }

        public static bool isOverGUI()
        {
            return EventSystem.current && EventSystem.current.IsPointerOverGameObject();
        }

        public static bool isAllowed()
        {
            return instance && instance.isAllowedA();
        }

        protected virtual bool isAllowedA()
        {
            return !isOverGUI();
        }

        private void Update()
        {
            checkHitMouse();
            if (focus != mhit)
            {
                if (focus)
                {
                    focus.doOut();
                    onUnFocus.Invoke(focus);
                }
                //Debug.Log("Change focus " + focus + " to " + mhit);
                focus = mhit;
                if (focus)
                {
                    focus.doOver();
                    onFocus.Invoke(focus);
                }
            }


            if (focus && focus.isMouseDown() && isAllowed())
            {
                _downPositon = VMouse.mousePosition;
                down = focus;
                down.doMouseDown();
                onDown.Invoke(down);
                //if (VKeyboard.GetKey(KeyCode.LeftControl)) Debug.Log(down);
            }
            else if (down && down.isMouseUp())
            {
                onUp.Invoke(down);
                down.doMouseUp();

                if ((VMouse.mousePosition - _downPositon).sqrMagnitude < 4)
                    onClick.Invoke(down);

                down = null;
            }
        }
    }
}

