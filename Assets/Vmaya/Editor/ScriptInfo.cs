using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public struct ClassInfo
{
    public string className;
    public List<string> baseClass;
    public string nameSpace;
}

public class MetaInfo
{
    public enum Type { Folder, Prefab, Scene, Script, Material, Texture, Shader }
    public string GUID;
    public string path;
    public string data;
    public string pathSource => path.Substring(0, path.IndexOf(".meta"));
    public Type type;

    public MetaInfo(string path)
    {
        this.path = path;
        data = File.ReadAllText(path);
        Match match = new Regex("guid:\\s+([\\w]+)").Match(data);
        if (match.Success)
        {
            GUID = match.Groups[1].Value;

            if (data.Contains("folderAsset: yes")) type = Type.Folder;
            else if (data.Contains("PrefabImporter:")) type = Type.Prefab;
            else if (data.Contains("MonoImporter:")) type = Type.Script;
            else if (data.Contains("NativeFormatImporter:")) type = Type.Material;
            else if (data.Contains("TextureImporter:")) type = Type.Texture;
            else if (data.Contains("DefaultImporter:")) type = Type.Scene;
            else if (data.Contains("ShaderImporter:")) type = Type.Shader;
            else if (path.Contains(".cs.meta")) type = Type.Script;
        }
    }
}

public class scriptInfo
{
    public string fullPath;
    public string content;
    public List<ClassInfo> classes;
    public List<ClassInfo> interfaces;
    public List<string> usingList;

    public scriptInfo(string a_fullPath)
    {
        fullPath = a_fullPath;
        content = File.ReadAllText(a_fullPath);
        parseUsing();
        parseClasses();
        parseInterfaces();
    }

    private void parseInterfaces()
    {
        interfaces = new List<ClassInfo>();
        MatchCollection mcns = (new Regex("namespace[\\s]+([\\w\\.]+)[\\s$]{0,}")).Matches(content);

        if (fullPath.Contains("UI\\Interfaces"))
        {
            Debug.Log(fullPath);
        }

        MatchCollection mc = new Regex("interface[\\s]+([\\w]+)(<[\\w]+>){0,1}[\\s]{0,}(:[\\s]{0,}([\\w<>\\s,]+)){0,1}").Matches(content);
        for (int i = 0; i < mc.Count; i++) 
            addMatch(interfaces, mcns, mc[i], i);
    }

    protected void parseUsing()
    {
        MatchCollection mc = new Regex("using[\\s]+([\\w\\.]+);").Matches(content);

        usingList = new List<string>();
        classes = new List<ClassInfo>();

        foreach (Match match in mc)
            usingList.Add(match.Groups[1].Value);
    }

    void addMatch(List<ClassInfo> list, MatchCollection mcns, Match match, int i)
    {
        ClassInfo classInfo = default;
        classInfo.className = match.Groups[1].Value.Trim();

        int baseIdx = match.Groups.Count - 1;

        string implExt = match.Groups[baseIdx].Value.Trim();

        if (!string.IsNullOrEmpty(implExt))
        {
            string[] implement = implExt.Split(',');
            classInfo.baseClass = new List<string>();
            for (int x = 0; x < implement.Length; x++)
                classInfo.baseClass.Add(implement[x].Trim());
        }

        if ((mcns != null) && (mcns.Count > 0))
        {
            match = mcns[Mathf.Min(i, mcns.Count - 1)];

            if (match.Groups.Count > 1)
                classInfo.nameSpace = match.Groups[1].Value;
        }

        list.Add(classInfo);
    }

    protected void parseClasses()
    {
        MatchCollection mcns = (new Regex("namespace[\\s]+([\\w\\.]+)[\\s$]{0,}")).Matches(content);
        MatchCollection mc = (new Regex("class[\\s]+([\\w]+)(<[\\w]+>){0,1}[\\s]{0,}(:[\\s]{0,}([\\w<>\\s,]+)){0,1}")).Matches(content);

        for (int i = 0; i < mc.Count; i++) addMatch(classes, mcns, mc[i], i);
    }
}
