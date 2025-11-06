using UnityEngine;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.UI;
using Vmaya.UI.Components;

namespace Vmaya.UI
{
    public class ShortcutManager : MonoBehaviour
    {
        [System.Serializable]
        public struct ShortcutData
        {
            public Text chortcutLabel;
            public UnityEvent action;
            [HideInInspector]
            public bool Ctrl;
            [HideInInspector]
            public bool Alt;
            [HideInInspector]
            public bool Shift;
            [HideInInspector]
#if ENABLE_INPUT_SYSTEM
            public Key keyCode;
#else
            public KeyCode keyCode;
#endif
        }

        [SerializeField]
        private ShortcutData[] shortcutData;

        private void Awake()
        {
            parseShortcut();
        }

        public static void parseShortcut(string shortCutStr, ref ShortcutData result)
        {
            string[] a = shortCutStr.Split('+');
            string KeyStr;
            if (a.Length > 1)
            {
                result.Ctrl = a[0].Trim().ToLower().Equals("ctrl");
                result.Alt = a[0].Trim().ToLower().Equals("alt");
                result.Shift = a[0].Trim().ToLower().Equals("shift");
                KeyStr = a[1].ToUpper();
            }
            else KeyStr = shortCutStr;
#if ENABLE_INPUT_SYSTEM
            result.keyCode = (Key)System.Enum.Parse(typeof(Key), KeyStr);
#else
            result.keyCode = (KeyCode)System.Enum.Parse(typeof(KeyCode), KeyStr);
#endif
        }


        private void parseShortcut()
        {
            for (int i=0; i< shortcutData.Length; i++)
                parseShortcut(shortcutData[i].chortcutLabel.text, ref shortcutData[i]);
        }

        private void Update()
        {
            if (!Curtain.isModal && !Vmaya.Scene3D.UI.InputControl.isFocus)
            {
                for (int i = 0; i < shortcutData.Length; i++)
                {
                    if (VKeyboard.GetKeyDown(shortcutData[i].keyCode))
                    {
#if ENABLE_INPUT_SYSTEM
                        if ((!shortcutData[i].Ctrl || VKeyboard.GetKey(Key.LeftCtrl)) &&
                            (!shortcutData[i].Alt || VKeyboard.GetKey(Key.LeftAlt)) &&
                            (!shortcutData[i].Shift || VKeyboard.GetKey(Key.LeftShift)))
#else
                        if ((!shortcutData[i].Ctrl || VKeyboard.GetKey(KeyCode.LeftControl)) &&
                            (!shortcutData[i].Alt || VKeyboard.GetKey(KeyCode.LeftAlt)) &&
                            (!shortcutData[i].Shift || VKeyboard.GetKey(KeyCode.LeftShift)))
#endif
                        {
                            shortcutData[i].action.Invoke();
                        }
                    }
                }
            }
        }
    }
}
