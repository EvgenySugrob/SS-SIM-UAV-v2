using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public abstract class BaseCollector : EditorWindow
{
    public static string ASSETSPATH = "Assets";
    protected static List<string> EXCLUDECLASSES = new List<string> { "void", "bool", "int", "float", "double", 
            "string", "char", "RectTransform", "Transform", "Material", "virtual", "Camera", "Camera", "Renderer", "Shader",
            "return", "using"};

    protected List<scriptInfo> scripts;
    protected List<string> copiedList;
    protected string logErrorList;
    protected bool rewrite;
    protected string distancePath;
    protected bool debugOnly;

    protected abstract void CreateUI();

    protected virtual void OnGUI()
    {
        Rect rect = new Rect(20, 20, 500, 800);
        GUILayout.BeginArea(rect);

        CreateUI();

        GUILayout.Label("Log error list or list classes or interfaces");
        logErrorList = GUILayout.TextArea(logErrorList);
        debugOnly = GUILayout.Toggle(debugOnly, "Debug only");

        GUILayout.EndArea();
    }

    protected abstract void FillInitData();

    protected abstract void CopyPrefabsAndScripts();
    protected abstract void CopyScenesAndScripts();

    protected void CopyIncludeClasses()
    {
        int cCount = 0;
        do
        {
            cCount = copiedList.Count;
            //CopyIncludeClasses("(\\w+)\\s+\\w+\\s+(=|;)", 1);
            //CopyIncludeClasses("(GetComponent|\\.[\\w]+)<([\\w\\.]+)>\\(", 2);

            CopyIncludeClasses("<(\\w+)>", 1);
            CopyIncludeClasses("(private|public|protected)\\s+(readonly\\s|static\\s){0,}([\\w\\.]+)(.+);", 3);
            CopyIncludeClasses("new\\s+(\\w+)\\(", 1);
            CopyIncludeClasses("([\\w]+)\\.(instance | Instance)", 1);
            CopyIncludeClasses("(private|public|protected)[\\s\\w]+\\([\\s]{0,}([\\w]+)", 1);
        } while (cCount != copiedList.Count);
    }

    private void CopyIncludeClasses(string pattern, int groupIdx)
    {
        foreach (scriptInfo si in scripts)
            if (copiedList.Contains(si.fullPath))
                CopyIncludeClassesFromContent(pattern, groupIdx, si.content);
    }

    protected void CopyFromLogErrors()
    {
        CopyIncludeClassesFromContent("(CS0117|CS0103|CS0246):[\\s\\w]{0,}\'([\\w]+)(<[\\w]{0,}>){0,}\'", 2, logErrorList);
        CopyIncludeClassesFromContent("(^|,|\\s+)(\\w{" + Mathf.Min(logErrorList.Length, 5) + ",30})", 2, logErrorList);
    }

    protected void CopyBaseClasses()
    {
        for (int i = 0; i < 3; i++)
        {
            foreach (scriptInfo si in scripts)
                if (copiedList.Contains(si.fullPath))
                {
                    foreach (ClassInfo ci in si.classes)
                    {
                        if (ci.baseClass != null)
                        {
                            List<scriptInfo> bsiList = GetByClass(ci.baseClass[0]);
                            foreach (scriptInfo bsi in bsiList)
                                if ((bsi != null) && !copiedList.Contains(bsi.fullPath))
                                    CopyScriptAndMeta(bsi.fullPath);

                            if (ci.baseClass.Count > 1)
                                foreach (string inf in ci.baseClass)
                                {
                                    scriptInfo sii = FindForInterface(inf);
                                    if (sii != null)
                                        CopyScriptAndMeta(sii.fullPath);
                                }
                        }
                    }
                }
        }
    }

    public static string ToPath(string path)
    {
        return string.IsNullOrEmpty(path) ? "" : path.Replace('/', Path.DirectorySeparatorChar);
    }

    protected List<scriptInfo> GetByClass(string className)
    {
        List<scriptInfo> result = new List<scriptInfo>();
        foreach (scriptInfo si in scripts)
        {
            for (int i = 0; i < si.classes.Count; i++)
                if (className.Equals(si.classes[i].className) && !copiedList.Contains(si.fullPath))
                    result.Add(si);

            if (si.interfaces != null)
                for (int i = 0; i < si.interfaces.Count; i++)
                    if (className.Equals(si.interfaces[i].className) && !copiedList.Contains(si.fullPath))
                        result.Add(si);
        }

        return result;
    }

    protected scriptInfo GetByPath(string filePath)
    {
        foreach (scriptInfo si in scripts)
            if (filePath.Equals(si.fullPath))
                return si;

        return default;
    }

    protected virtual string getClassPath(string className)
    {
        return AssetDatabase.GUIDToAssetPath(className);
    }

    protected void CopyIncludeClassesFromContent(string pattern, int groupIdx, string content)
    {
        MatchCollection inClasses = (new Regex(pattern)).Matches(content);

        for (int i = 0; i < inClasses.Count; i++)
        {
            Match match = inClasses[i];

            if (match.Groups.Count > groupIdx)
            {
                string[] classPath = match.Groups[groupIdx].Value.Split('.');

                foreach (string className in classPath)
                {
                    string path = getClassPath(className);
                    if (!string.IsNullOrEmpty(path))
                    {
                        if (path.Contains(ASSETSPATH))
                            CopyScriptAndMeta(path);

                    }
                    else if (!EXCLUDECLASSES.Contains(className))
                    {
                        List<scriptInfo> bsiList = GetByClass(className);
                        foreach (scriptInfo classScriptInfo in bsiList)
                            if (classScriptInfo != null)
                            {
                                CopyScriptAndMeta(classScriptInfo.fullPath);
                                Debug.Log("Found class: " + className);
                            }
                    }
                }
            }
        }
    }

    public scriptInfo FindForInterface(string interfaceName)
    {
        foreach (scriptInfo si in scripts)
            if (si.interfaces != null)
                foreach (ClassInfo inf in si.interfaces)
                    if (inf.className.Contains(interfaceName))
                        return si;

        return null;
    }

    protected bool CopyScriptAndMeta(string scriptPath)
    {
        bool result = CopyFileAndMeta(scriptPath);
        if (result) ParseScripFile(scriptPath);
        return result;
    }

    protected string reSep(string path)
    {
        if (path[path.Length - 1] == Path.DirectorySeparatorChar)
            return path.Substring(0, path.Length - 1);

        return path;
    }

    protected bool CopyFileAndMeta(string srcPath)
    {

        if (!string.IsNullOrEmpty(srcPath) && !copiedList.Contains(srcPath))
        {

            FileInfo fi = new FileInfo(srcPath);
            string relPath = srcPath.Replace(fi.Name, null);

            string newDir;
            if (relPath.Contains(ASSETSPATH))
                newDir = distancePath + relPath.Substring(relPath.IndexOf(ASSETSPATH) + ASSETSPATH.Length);
            else newDir = distancePath + Path.DirectorySeparatorChar + relPath;

            if (!debugOnly)
            {
                if (!Directory.Exists(newDir))
                {
                    Directory.CreateDirectory(newDir);
                    string pathMeta = reSep(relPath) + ".meta";
                    if (File.Exists(pathMeta))
                        File.Copy(pathMeta, reSep(newDir) + ".meta", true);

                    Debug.Log("Create directory: " + newDir);
                }

                string newFilePath = newDir + fi.Name;
                if (rewrite || !File.Exists(newFilePath))
                {
                    File.Copy(fi.FullName, newFilePath, true);

                    FileInfo meta = new FileInfo(srcPath + ".meta");

                    if (File.Exists(meta.FullName))
                        File.Copy(meta.FullName, newDir + meta.Name, true);
                }
            }

            copiedList.Add(srcPath);
            return true;
        }
        return false;
    }

    protected void ParseScripFile(string scriptPath)
    {
        scriptInfo info = GetByPath(scriptPath);
        if ((info != null) && copiedList.Contains(scriptPath))
        {

            foreach (string usingStr in info.usingList)
            {
                string nsPath = ASSETSPATH + Path.DirectorySeparatorChar + usingStr.Replace('.', Path.DirectorySeparatorChar);
                if (Directory.Exists(nsPath))
                {
                    string interfaceFile = nsPath + Path.DirectorySeparatorChar + "Interfaces.cs";
                    if (File.Exists(interfaceFile))
                        CopyScriptAndMeta(interfaceFile);
                }
            }
        }
    }

}
