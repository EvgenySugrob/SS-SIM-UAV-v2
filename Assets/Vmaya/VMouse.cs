using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Vmaya
{
    public class VMouse
    {

#if ENABLE_INPUT_SYSTEM
        public static Vector3 mousePosition => Mouse.current.position.ReadValue();

        public static bool GetMouseButton(int buttonCode)
        {
            switch (buttonCode) {
                case 0: return Mouse.current.leftButton.isPressed;
                case 1: return Mouse.current.rightButton.isPressed;
                case 2: return Mouse.current.middleButton.isPressed;
            }
            return false;
        }

        public static bool GetMouseButtonDown(int buttonCode)
        {
            switch (buttonCode)
            {
                case 0: return Mouse.current.leftButton.wasPressedThisFrame;
                case 1: return Mouse.current.rightButton.wasPressedThisFrame;
                case 2: return Mouse.current.middleButton.wasPressedThisFrame;
            }
            return false;
        }

        public static bool GetMouseButtonUp(int buttonCode)
        {
            switch (buttonCode)
            {
                case 0: return Mouse.current.leftButton.wasReleasedThisFrame;
                case 1: return Mouse.current.rightButton.wasReleasedThisFrame;
                case 2: return Mouse.current.middleButton.wasReleasedThisFrame;
            }
            return false;
        }

        public static float ScrollWheel => Mouse.current.scroll.value.y;
#else
        public static Vector3 mousePosition => Input.mousePosition;

        public static bool GetMouseButton(int buttonCode)
        {
            return Input.GetMouseButton(buttonCode);
        }

        public static bool GetMouseButtonDown(int buttonCode)
        {
            return Input.GetMouseButtonDown(buttonCode);
        }

        public static bool GetMouseButtonUp(int buttonCode)
        {
            return Input.GetMouseButtonUp(buttonCode);
        }

        public static float ScrollWheel => Input.GetAxis("Mouse ScrollWheel");
#endif
    }
}
