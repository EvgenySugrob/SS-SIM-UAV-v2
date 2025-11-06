using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Vmaya.UI.Components
{
    public class WindowManager : UIComponent
    {
        [SerializeField]
        private Component curtain;
        private ICurtain _curtain => curtain ? curtain.GetComponent<ICurtain>() : null;

        [SerializeField]
        private Window[] Initialize;

        public UnityEvent onModalModeChange;

        private Vector3 _screenSizePrev;
        public UnityEvent onScreenResize;
        public bool Restrict = false;

        private void OnValidate()
        {
            curtain = _curtain as Component;
        }

        private void Awake()
        {
            for (int i = 0; i < Initialize.Length; i++) Initialize[i].init();

            if (!EventSystem.current)
            {
                GameObject go = new GameObject();
                go.AddComponent<EventSystem>();
                go.AddComponent<StandaloneInputModule>();
                go.name = "EventSystem";
            }
        }

        private Object ModalDialog()
        {
            Object md = GetComponentInChildren<ModalWindow>();
            if (md == null) md = GetComponentInChildren<IDialog>() as Object;
            return md;
        }

        public void showCurtain()
        {
            if (ModalDialog())
            {
                if (_curtain != null) _curtain.Show();
                onModalModeChange.Invoke();
            }
        }

        public void requireHideCurtain()
        {
            if (!ModalDialog())
            {
                if (_curtain != null) _curtain.Hide();
                onModalModeChange.Invoke();
            }
        }

        public int maxSiblinindex()
        {
            int max = 0;
            Window[] windows = GetComponentsInChildren<Window>();
            foreach (Window window in windows)
                max = Mathf.Max(window.GetComponent<RectTransform>().GetSiblingIndex(), max);
            return max;
        }

        private void Update()
        {
            Vector2 newSSize = new Vector2(Screen.width, Screen.height);
            if (!newSSize.Equals(_screenSizePrev))
            {
                _screenSizePrev = newSSize;
                onScreenResize.Invoke();
            }
        }

        public void CloseWindow(Window window)
        {
            if (window)
                Vmaya.Utils.afterEndOfFrame(this, window.hide);
        }

    }
}