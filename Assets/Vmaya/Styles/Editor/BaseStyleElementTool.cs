using UnityEditor;
using UnityEngine;

namespace Vmaya.Styles
{
    [CustomEditor(typeof(BaseStyleElement)), CanEditMultipleObjects]
    public class BaseStyleElementTool : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (StyleList.Instance)
            {
                if (GUILayout.Button("Send back to style list"))
                {
                    (target as BaseStyleElement).ApplyFromEditor();
                }
            }
        }
    }
}

