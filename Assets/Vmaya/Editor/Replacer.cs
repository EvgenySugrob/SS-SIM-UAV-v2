using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

//[CustomEditor(typeof(Transform))]
public class Replacer 
{
    [MenuItem("CONTEXT/Transform/Replace with similar name")]
    static void Replace(MenuCommand command)
    {
        GameObject select = Selection.activeObject as GameObject;
        Regex re = new Regex("[\\d\\s]+");


        if (select)
        {
            string nameTmp = re.Replace(select.name, "");

            Match m = re.Match(select.name);
            int number = int.Parse(m.Value);

            List<Transform> list = new List<Transform>();
            for (int i = 0; i < select.transform.parent.childCount; i++)
            {
                Transform child = select.transform.parent.GetChild(i);
                if (child.gameObject != select)
                {
                    string cname = re.Replace(child.name, "");
                    if (nameTmp.Equals(cname)) list.Add(child);
                }
            }


            for (int i = 0; i < list.Count; i++)
            {
                Transform child = list[i];
                GameObject clone = GameObject.Instantiate(select);

                clone.transform.parent = select.transform.parent;
                clone.transform.position = child.position;
                clone.transform.rotation = child.rotation;
                clone.transform.localScale = child.localScale;

                number++;
                clone.name = nameTmp + " c" + number;

                //GameObject.DestroyImmediate(child);

                //break;
            }
        }
    }
    /*
    [MenuItem("Examples/Instantiate Selected")]
    static void InstantiatePrefab()
    {
        Object prefab = PrefabUtility.GetPrefabInstanceHandle(Selection.activeObject);
        Object clone = PrefabUtility.InstantiatePrefab(Selection.activeObject);

        Debug.Log(clone);
    }

    [MenuItem("Examples/Instantiate Selected", true)]
    static bool ValidateInstantiatePrefab()
    {
        GameObject go = Selection.activeObject as GameObject;
        return (go != null) && PrefabUtility.IsPartOfPrefabAsset(go);
    }*/
}
