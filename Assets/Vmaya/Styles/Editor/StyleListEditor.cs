using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Vmaya.Styles
{
    [CustomEditor(typeof(StyleList)), CanEditMultipleObjects]
    public class StyleListEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Apply"))
            {
                (target as StyleList).Apply();
            }
        }
    }
}
