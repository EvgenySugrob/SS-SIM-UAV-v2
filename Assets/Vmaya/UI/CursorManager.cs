using UnityEngine;

namespace Vmaya.UI
{
    public abstract class CursorManager : MonoBehaviour, ICursorManager
    {
        private static ICursorManager _instance;
        public static ICursorManager instance => _instance;

        public static bool isInit => _instance != null;

        private void Awake()
        {
            _instance = this;
        }

        public abstract void setCursor(string nameCursor);
    }
}
