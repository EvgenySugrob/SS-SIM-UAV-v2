using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using Vmaya.Scene3D;
using Vmaya.UI.Components;

namespace Vmaya
{
    public class CameraManager : MonoBehaviour
    {
        [SerializeField]
        private List<Camera> cameras;
        [SerializeField]
        private int currentCameraIndex;
        [SerializeField]
        private int playerCamera;
        [SerializeField]
        private bool transmitPosition;

#if ENABLE_INPUT_SYSTEM
        [SerializeField]
        private InputActionReference _nextCamera;
#else
        [SerializeField]
        private KeyCode nextCameraKey;
        [SerializeField]
        private KeyCode backCameraKey;
#endif

        private int _currentCameraIndex = -1;
        public UnityEvent onChangeCamera;
        public UnityEvent onGamemode;
        public UnityEvent offGamemode;

        private static CameraManager _instance;
        public static CameraManager instance => getInstance();

        private bool _nextCameraDown = false;
        public bool mode2d => getCurrent().orthographic;
        public bool gameMode => currentCameraIndex == playerCamera;
        public List<Camera> Cameras => cameras;

        public bool isCurrent(Camera camera)
        {
            return camera == cameras[currentCameraIndex];
        }

        private static CameraManager getInstance()
        {
            if (_instance == null)
            {
                if ((_instance = FindObjectOfType<CameraManager>()) == null)
                {
                    GameObject ga = new GameObject();
                    ga.name = "CameraManager";
                    _instance = ga.AddComponent<CameraManager>();

                    _instance.onChangeCamera = new UnityEvent();
                    _instance.onGamemode = new UnityEvent();
                    _instance.offGamemode = new UnityEvent();
                }
            }
            return _instance;
        }

        // Use this for initialization
        virtual protected void Awake()
        {
            if ((cameras == null) || (cameras.Count == 0))
            {

                Camera[] objs = Resources.FindObjectsOfTypeAll<Camera>();
                cameras = new List<Camera>();
                Camera acurrent = Camera.main ? Camera.main : Camera.current;

                for (int i=0; i<objs.Length; i++)
                {
                    if (objs[i].targetTexture == null)
                    {
                        cameras.Add(objs[i]);
                        if (acurrent == objs[i]) currentCameraIndex = cameras.Count - 1; 
                    }
                }
            }
            _instance = this;
        }

        public static Camera getCurrent()
        {
            if (instance)
                return (instance.currentCameraIndex > -1) ? instance.cameras[instance.currentCameraIndex] : Camera.main;
            else return Camera.current;
        }

        public static Camera getLastCurrent()
        {
            if (instance)
                return (instance._currentCameraIndex > -1) ? instance.cameras[instance._currentCameraIndex] : Camera.main;
            else return Camera.current;
        }

        void updateCurrentCamera()
        {
            for (int i = 0; i < cameras.Count; i++)
            {
                cameras[i].enabled = (i == currentCameraIndex);
            }
            afterUpdate();
        }

        void afterUpdate()
        {
            onChangeCamera.Invoke();
            _currentCameraIndex = currentCameraIndex;
            if (gameMode) onGamemode.Invoke();
            else offGamemode.Invoke();

            //Selectable
        }

        public virtual void nextCamera(int inc = 1)
        {
            setCameraIndex((cameras.Count + currentCameraIndex + inc) % cameras.Count);
        }

        void Update()
        {
            if (currentCameraIndex != _currentCameraIndex) updateCurrentCamera();

            if (cameras.Count > 1)
            {
#if ENABLE_INPUT_SYSTEM
                int incnc = _nextCamera ? Mathf.FloorToInt(_nextCamera.action.ReadValue<float>()) : 0;
#else
                int incnc = VKeyboard.GetKeyDown(nextCameraKey) ? 1 : (VKeyboard.GetKeyDown(backCameraKey) ? -1 : 0);
#endif
                if (!Curtain.isModal && (incnc != 0))
                {
                    if (!_nextCameraDown)
                    {
                        _nextCameraDown = true;
                        nextCamera(incnc);
                    }
                }
                else _nextCameraDown = false;
            }
        }

        public void setCameraIndex(int index)
        {
            if (currentCameraIndex != index)
            {
                Cursor.visible = true;

                Camera lastCurrent = getCurrent();
                Vector3 pp = default;

                if (lastCurrent)
                     pp = lastCurrent.transform.position;

                clearOutlines();

                currentCameraIndex = index;

                if (lastCurrent && transmitPosition)
                {
                    Vector3 np = getCurrent().transform.position;
                    np.x = pp.x;
                    np.z = pp.z;
                    getCurrent().transform.position = np;
                }
            }
        }

        private void clearOutlines()
        {
            if (SwitchableList.instance != null) SwitchableList.instance.Clear<BaseSelectableOutline>();
        }

        public void Orthographic(bool isOrth)
        {
            for (int i = 0; i < cameras.Count; i++)
            {
                if (cameras[i].orthographic == isOrth)
                {
                    setCameraIndex(i);
                    break;
                }
            }
        }

        public Camera playCamera
        {
            get
            {
                return cameras[playerCamera];
            }
        }

        public void Player()
        {
            setCameraIndex(playerCamera);
        }

        internal static Vector3 lookPlane()
        {
            Vector3 look = getCurrent().transform.forward;

            if (Mathf.Abs(look.y) > Mathf.Max(Mathf.Abs(look.x), Mathf.Abs(look.z)))
            {
                look.y = look.y > 0 ? 1 : -1;
                look.x = look.z = 0;
            }
            else if (Mathf.Abs(look.x) > Mathf.Abs(look.z))
            {
                look.x = look.x > 0 ? 1 : -1;
                look.y = look.z = 0;
            }
            else
            {
                look.z = look.z > 0 ? 1 : -1;
                look.x = look.y = 0;
            }

            return look;
        }
    }
}
