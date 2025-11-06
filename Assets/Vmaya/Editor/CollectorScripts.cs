using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CollectorScripts : BaseCollector
{

    protected string relativePath;

    [MenuItem("Tools/CollectorScripts")]
    public static void ShowMyEditor()
    {
        EditorWindow wnd = GetWindow<CollectorScripts>();
        wnd.titleContent = new GUIContent("CollectorScripts");
    }

    protected override void FillInitData()
    {
        scripts = new List<scriptInfo>();
        string[] guids = AssetDatabase.FindAssets("t:Script");
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains(ASSETSPATH))
            {
                if (path.Contains("FileSource"))
                {
                    //Debug.Log(path);  
                }
                scripts.Add(new scriptInfo(path));
            }
        }
    }

    private void CopyMaterials(Material mat)
    {
        var materialPath = AssetDatabase.GetAssetPath(mat);
        if (!string.IsNullOrEmpty(materialPath) && materialPath.Contains(ASSETSPATH))
            CopyFileAndMeta(materialPath);

        string[] names = mat.GetTexturePropertyNames();
        foreach (string name in names)
        {
            Texture texture = mat.GetTexture(name);
            var texturePath = AssetDatabase.GetAssetPath(texture);
            if (!string.IsNullOrEmpty(texturePath) && texturePath.Contains(ASSETSPATH))
                CopyFileAndMeta(texturePath);
        }
    }

    private void CopyFromGA(GameObject ga)
    {

        Renderer rend = ga.GetComponentInChildren<Renderer>(true);
        if (rend)
        {
            for (int i=0; i < rend.materials.Length; i++)
                CopyMaterials(rend.materials[i]);
        }

        MonoBehaviour[] mbList = ga.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour mb in mbList)
        {
            Graphic gr = mb as Graphic;
            if (gr)
                CopyImage(gr);
            else
            {
                MonoScript script = MonoScript.FromMonoBehaviour(mb);
                var scripPath = AssetDatabase.GetAssetPath(script);
                if (scripPath.Contains(ASSETSPATH))
                    CopyScriptAndMeta(scripPath);
            }
        }
    }

    private void CopyImage(Graphic gr)
    {
        Image img = gr as Image;
        if (img)
        {
            string spritePath = AssetDatabase.GetAssetPath(img.sprite);
            if (spritePath.Contains(ASSETSPATH))
                CopyScriptAndMeta(spritePath);

        }
        
        CopyMaterials(gr.material);
    }
    
    protected override void CopyScenesAndScripts()
    {
        string[] guids = AssetDatabase.FindAssets("t:Scene");
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains(relativePath))
            {
                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                if (sceneAsset != null)
                {
                    Scene scene = SceneManager.GetSceneByName(sceneAsset.name);
                    try
                    {
                        CopyFileAndMeta(path);

                        GameObject[] gaList = scene.GetRootGameObjects();
                        foreach (GameObject ga in gaList)
                            if (ga)
                                CopyFromGA(ga);
                    }
                    catch (Exception)
                    {
                        Debug.Log("Scene: " + sceneAsset.name + " parse as text");
                        ParseSceneAsText(path);
                    }
                }
            }
        }
    }

    private void ParseSceneAsText(string pathScene)
    {
        string sceneText = File.ReadAllText(pathScene);
        CopyIncludeClassesFromContent("m_Script: {fileID: (\\d+), guid: ([\\d\\w]+), type: 3}", 2, sceneText);
    }

    protected override void CopyPrefabsAndScripts()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains(relativePath))
            {
                GameObject ga = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (ga)
                {
                    CopyFromGA(ga);
                    CopyFileAndMeta(path);
                }
            }
        }
    }

    protected override void CreateUI()
    {

        if (string.IsNullOrEmpty(logErrorList))
        {
            GUILayout.Label("Relative path to package");
            relativePath = GUILayout.TextField(relativePath);

            GUILayout.Label("Distance project path");
            distancePath = GUILayout.TextField(distancePath);
        }

        rewrite = GUILayout.Toggle(rewrite, "Rewrite files"); if (GUILayout.Button("Begin"))
        {
            if (!string.IsNullOrEmpty(relativePath) && Directory.Exists(distancePath))
            {
                copiedList = new List<string>();
                FillInitData();

                if (string.IsNullOrEmpty(logErrorList))
                {
                    CopyPrefabsAndScripts();
                    CopyScenesAndScripts();
                    CopyIncludeClasses();
                }
                else
                {
                    CopyFromLogErrors();
                    CopyIncludeClasses();
                }
                CopyBaseClasses();
            }
        }
    }
}
