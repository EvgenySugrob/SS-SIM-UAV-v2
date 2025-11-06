using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.UI;
using Vmaya.UI.Components;

namespace Vmaya.UI
{
    public class clickButton : MonoBehaviour
    {
#if ENABLE_INPUT_SYSTEM
        public Key keyCode = Key.Enter;
#else
        public KeyCode keyCode = KeyCode.Return;
#endif

        void Update()
        {
            if (VKeyboard.GetKeyDown(keyCode))
            {
                Window window = GetComponentInParent<Window>();
                WindowManager manager = GetComponentInParent<WindowManager>();
                if (window && manager)
                {
                    RectTransform trans = window.GetComponent<RectTransform>();
                    if (trans.GetSiblingIndex() == manager.maxSiblinindex())
                        GetComponent<Button>().onClick.Invoke();
                }
            }
        }
    }
}