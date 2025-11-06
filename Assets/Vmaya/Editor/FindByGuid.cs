using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FindByGuid : EditorWindow
{
    [MenuItem("Tools/Find by GUID")]
    public static void ShowMyEditor()
    {
        EditorWindow wnd = GetWindow<FindByGuid>();
        wnd.titleContent = new GUIContent("FindByGuid");
    }

    private string _guids;
    private List<string> _paths = new List<string>();

    private void OnGUI()
    {
        Rect rect = new Rect(0, 0, position.width, position.height);
        GUILayout.BeginArea(rect);

        GUILayout.Label("GUID or file name");
        _guids = GUILayout.TextField(_guids);
        if (GUILayout.Button("Begin"))
            Begin(_guids);

        if (_paths.Count > 0)
        {
            GUILayout.BeginScrollView(rect.min);
            foreach (string pathOrGUID in _paths)
            {
                Type type = AssetDatabase.GetMainAssetTypeAtPath(pathOrGUID);
                if (type != null)
                    GUILayout.Label(type.ToString());
                else
                {
                    string a_path = AssetDatabase.GUIDToAssetPath(pathOrGUID);
                    type = AssetDatabase.GetMainAssetTypeAtPath(a_path);
                    if (type != null)
                        GUILayout.Label(type.ToString() + " " + a_path);
                }

                GUILayout.TextField(pathOrGUID);
            }
            GUILayout.EndScrollView();
        } else GUILayout.Label("List is empty");

        GUILayout.EndArea();
    }

    public List<string> GuidsByType(string a_guids, string type)
    {
        string[] guids = AssetDatabase.FindAssets("t:" + type);
        List<string> result = new List<string>();

        List<string> f_guids = new List<string>(a_guids.Split(new char[] { ',', ' ', ';' }));
        foreach (string guid in guids)
        {
            if (f_guids.Contains(guid))
                result.Add(guid);
        }
        return result;
    }

    private void Begin(string a_guids)
    {
        _paths.Clear();
        List<string> f_guids = new List<string>(a_guids.Split(new char[] { ',', ' ', ';' }));
        foreach (string guidName in f_guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guidName);
            if (!string.IsNullOrEmpty(path))
                _paths.Add(path);
            else
            {
                string[] paths = AssetDatabase.GetAllAssetPaths();
                foreach (string a_path in paths)
                    if (a_path.Contains(guidName))
                        _paths.Add(AssetDatabase.GUIDFromAssetPath(a_path).ToString());
            }
        }
    }
}
