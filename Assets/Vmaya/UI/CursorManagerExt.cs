using UnityEngine;

namespace Vmaya.UI
{
    public class CursorManagerExt : CursorManager
    {
        [System.Serializable]
        public struct cursorData
        {
            public string name;
            public Texture2D cursor;
            public Vector2 hotspot;
        }

        [SerializeField]
        private cursorData[] _cursors;
        public cursorData[] Cursors { get => _cursors; }        

        [SerializeField]
        protected int _startIndex = -1;
        protected int _currentIndex = -1;

        private void Start()
        {
            if (_startIndex > -1) setCursor(_startIndex);
        }

        public override void setCursor(string name)
        {
            setCursor(indexOf(name));
        }

        public void setCursor(int idx)
        {
            if (_currentIndex != idx)
                setCursorA(idx);
        }

        private void setCursorA(int idx)
        {
            _currentIndex = idx;
            if ((idx > -1) && (idx < _cursors.Length))
                Cursor.SetCursor(_cursors[idx].cursor, _cursors[idx].hotspot, CursorMode.Auto);
            else Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        public int indexOf(string name)
        {
            for (int i = 0; i < _cursors.Length; i++)
                if (_cursors[i].name.Equals(name)) return i;
            return -1;
        }
    }
}