using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class PullThatNecessary : BaseCollector
{
    protected string sourcePath;
    protected string basePath;

    protected List<MetaInfo> _metas;

    [MenuItem("Tools/PullThatNecessary")]

    public static void ShowMyEditor()
    {
        EditorWindow wnd = GetWindow<PullThatNecessary>();
        wnd.titleContent = new GUIContent("PullThatNecessary");
    }

    protected string RelativePath(string a_sourcePath)
    {
        return a_sourcePath.Substring(a_sourcePath.IndexOf(ASSETSPATH) + ASSETSPATH.Length);
    }

    protected string DistancePath(string a_sourcePath)
    {
        return Application.dataPath.Replace('/', Path.DirectorySeparatorChar) + RelativePath(a_sourcePath);
    }

    protected override string getClassPath(string className)
    {
        List<scriptInfo> list = GetByClass(className);
        return list.Count > 0 ? list[0].fullPath : null;
    }

    protected override void CreateUI()
    {
        GUILayout.Label("Source file or directory");
        sourcePath = ToPath(GUILayout.TextField(sourcePath));

        string initPath = !string.IsNullOrEmpty(sourcePath) ? sourcePath : Application.dataPath;
        string resultPath = null;

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Choose path", GUILayout.MaxWidth(100)))
            resultPath = EditorUtility.OpenFolderPanel("Choose directory", initPath, "");

        if (GUILayout.Button("Choose file", GUILayout.MaxWidth(100)))
            resultPath = EditorUtility.OpenFilePanel("Choose file", initPath, "unity,prefab");

        if (!string.IsNullOrEmpty(resultPath))
            sourcePath = ToPath(resultPath);

        EditorGUILayout.EndHorizontal();

        rewrite = GUILayout.Toggle(rewrite, "Rewrite files");

        if (GUILayout.Button("Begin"))
        {
            if (!string.IsNullOrEmpty(sourcePath))
            {
                distancePath = ToPath(Application.dataPath);
                copiedList = new List<string>();
                FillInitData();

                if (string.IsNullOrEmpty(logErrorList))
                {
                    if (File.Exists(sourcePath))
                    {
                        CopyResource(findForPath(sourcePath));
                    } else if (Directory.Exists(sourcePath))
                    {
                        CopyPrefabsAndScripts();
                        CopyScenesAndScripts();
                    } 
                    CopyIncludeClasses();
                }
                else
                {
                    CopyFromLogErrors();
                    CopyIncludeClasses();
                }
                CopyBaseClasses();

                AssetDatabase.Refresh();
            }
            else Debug.LogError("Path " + sourcePath + " not found");
        }
    }

    private void CopyIncludeGuidFromContent(string content, string pattern, int groupIdx)
    {
        MatchCollection inClasses = (new Regex(pattern)).Matches(content);

        for (int i = 0; i < inClasses.Count; i++)
        {
            Match match = inClasses[i];

            if (match.Groups.Count > groupIdx)
            {
                string guid = match.Groups[groupIdx].Value;

                MetaInfo mi = findForGuid(guid);
                if ((mi != null) && mi.path.Contains(ASSETSPATH))
                {
                    if (mi.type == MetaInfo.Type.Script)
                    {
                        scriptInfo si = GetByPath(mi.pathSource);

                        foreach (ClassInfo classInfo in si.classes)
                            if (!EXCLUDECLASSES.Contains(classInfo.className)) {
                                CopyScriptAndMeta(mi.pathSource);
                                Debug.Log("Found class: " + classInfo.className);   
                            }
                    } else CopyFileAndMeta(mi.pathSource);
                }
            }
        }
    }

    protected override void CopyPrefabsAndScripts()
    {
        CopyAndScripts(MetaInfo.Type.Prefab);
    }

    protected override void CopyScenesAndScripts()
    {
        CopyAndScripts(MetaInfo.Type.Scene);
    }

    protected void CopyAndScripts(MetaInfo.Type type)
    {
        foreach (MetaInfo mi in _metas)
        {
            if (mi.path.Contains(sourcePath) && (mi.type == type))
                CopyResource(mi);
        }
    }

    private void CopyResource(MetaInfo mi)
    {
        if (mi != null)
        {
            CopyFileAndMeta(mi.pathSource);
            CopyIncludeGuidFromContent(File.ReadAllText(mi.pathSource), "{fileID: ([-\\d]+), guid: ([-\\d\\w]+), type: [32]{1}}", 2);
        }
    }

    protected override void FillInitData()
    {
        scripts = new List<scriptInfo>();
        if (sourcePath.Contains(ASSETSPATH))
        {
            basePath = sourcePath.Substring(0, sourcePath.IndexOf(ASSETSPATH) + ASSETSPATH.Length).Replace('\\', Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            FillMetas();
            FullScripts();
        }
    }

    protected virtual void FullScripts()
    {
        foreach (MetaInfo mi in _metas)
            if (mi.type == MetaInfo.Type.Script)
            {
                if (File.Exists(DistancePath(mi.pathSource)))
                    copiedList.Add(mi.pathSource);


                scriptInfo si = new scriptInfo(mi.pathSource);
                scripts.Add(si);
            }
    }

    public static void ProcessDirectory(string targetDirectory, Action<FileInfo> ProcessFile)
    {
        string[] fileEntries = Directory.GetFiles(targetDirectory);
        foreach (string fileName in fileEntries)
            ProcessFile(new FileInfo(fileName));

        string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
        foreach (string subdirectory in subdirectoryEntries)
            ProcessDirectory(subdirectory, ProcessFile);
    }

    protected MetaInfo findForGuid(string guid)
    {
        foreach (MetaInfo mi in _metas)
            if (mi.GUID == guid)
                return mi;

        return null;
    }

    protected MetaInfo findForPath(string path)
    {
        foreach (MetaInfo mi in _metas)
            if (mi.pathSource.Equals(path))
                return mi;

        return null;
    }

    private void FillMetas()
    {
        _metas = new List<MetaInfo>();
        ProcessDirectory(basePath, (FileInfo fi) => {
            if (fi.Extension == ".meta")
            {
                MetaInfo mi = new MetaInfo(fi.FullName);
                _metas.Add(mi);
            }
        });
    }
}
