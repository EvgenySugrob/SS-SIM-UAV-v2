using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Vmaya
{
    public class VKeyboard
    {

#if ENABLE_INPUT_SYSTEM
        internal static bool anyKey => Keyboard.current.anyKey.isPressed;
        public static bool GetKey(Key value)
        {
            return Keyboard.current[value].isPressed;
        }

        public static bool GetKeyDown(Key value)
        {
            return Keyboard.current[value].wasPressedThisFrame;
        }

        public static bool GetKeyUp(Key value)
        {
            return Keyboard.current[value].wasReleasedThisFrame;
        }
#else
        internal static bool anyKey => Input.anyKey;
        public static bool GetKey(KeyCode value)
        {
            return Input.GetKey(value);
        }

        public static bool GetKeyDown(KeyCode value)
        {
            return Input.GetKeyDown(value);
        }

        public static bool GetKeyUp(KeyCode value)
        {
            return Input.GetKeyUp(value);
        }
#endif
    }
}
