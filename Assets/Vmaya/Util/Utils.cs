using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEditor;

namespace Vmaya.Util
{
    public class Utils
    {
        public delegate void SimpleHandle();
        public static float Null = 0.001f;

        public static double round(double v, double gridSize = 10)
        {
            return Math.Round(v * gridSize) / gridSize;
        }

        public static double round2(double v, int dec)
        {
            return Math.Round(v, dec, System.MidpointRounding.AwayFromZero);
        }

        internal static List<T> FindChildAll<T>(Transform transform, string pathName) where T : MonoBehaviour
        {
            List<T> result = new List<T>();
            T[] all = transform.GetComponentsInChildren<T>();

            foreach (T item in all)
                if (item.name.Equals(pathName)) result.Add(item);

            return result;
        }

        internal static Transform FindChild(Transform transform, string pathName)
        {
            for (int i = 0; i < transform.childCount; i++)
                if (transform.GetChild(i).name.Equals(pathName)) return transform.GetChild(i).transform;

            return null;
        }

        internal static Transform FindChildCreate(Transform transform, string pathName)
        {
            Transform pathFolder = Utils.FindChild(transform, pathName);
            if (pathFolder) return pathFolder;

            GameObject ga = new GameObject();
            ga.transform.parent = transform;
            ga.name = pathName;

            return ga.transform;
        }

        // Находит проекцию точки (point) на отрезке (p1-p2)
        // Возвращает true если проекцию точки пренадлежит отрезку
        internal static bool nearestTest(Vector3 p1, Vector3 p2, Vector3 point, out Vector3 onLine)
        {
            Vector3 direct = p1 - p2;
            float dm = direct.magnitude;

            Vector3 to_p = p1 - point;
            onLine = Vector3.zero;

            float dot = Vector3.Dot(to_p, direct);

            //Debug.DrawLine(p1, point, Color.red, 1);
            //Debug.DrawLine(point, p2, Color.green, 1);

            if (dot > 0)
            {

                Vector3 pj = Vector3.Project(-to_p, direct.normalized);
                onLine = pj + p1;
                return pj.magnitude <= dm;
            }

            return false;
        }

        public static Bounds getRBounds(Transform trans)
        {
            MeshRenderer[] rr = trans.GetComponentsInChildren<MeshRenderer>();
            MeshRenderer pr = trans.GetComponent<MeshRenderer>();
            Bounds b = new Bounds();
            if (pr) b = pr.bounds;
            else if (rr.Length > 0) b = rr[0].bounds;

            foreach (MeshRenderer r in rr)
            {
                b.Encapsulate(r.bounds);
                if (r.transform.childCount > 0)
                {
                    if (r.transform != trans)
                        b.Encapsulate(getRBounds(r.transform));
                }
            }
            return b;
        }

        public static void childToCenter(Transform transform)
        {
            Vector3 pos = transform.position;
            transform.position = Vector3.zero;
            Bounds b = Utils.getRBounds(transform);
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).transform.localPosition = -b.center;
            }
            transform.position = pos;
        }

        public static void createColliders(Transform trans, List<MeshCollider> list)
        {
            if (trans.name[0] != '_')
            {
                MeshFilter filter = trans.GetComponent<MeshFilter>();
                if (filter)
                {
                    if (trans.GetComponent<MeshCollider>() == null)
                    {
                        list.Add(trans.gameObject.AddComponent<MeshCollider>());
                    }
                }
                for (int i = 0; i < trans.childCount; i++)
                {
                    createColliders(trans.GetChild(i), list);
                };
            }
        }

        public static List<string> intersection(List<string> l1, List<string> l2)
        {
            List<string> result = new List<string>();
            foreach (string i1 in l1)
                if (l2.Contains(i1)) result.Add(i1);
            return result;
        }

        public static List<string> union(List<string> l1, List<string> l2)
        {
            List<string> result = new List<string>();
            foreach (string i1 in l1) if (!result.Contains(i1)) result.Add(i1);
            foreach (string i2 in l2) if (!result.Contains(i2)) result.Add(i2);
            return result;
        }

        public static string currentTime()
        {
            return System.DateTime.Now.ToString();//"dd.MM.yyyy HH:mm:ss"
        }

        public static long toTime(string dateTime)
        {
            return System.DateTime.Parse(dateTime).ToFileTime();
        }

        public static string timeToString(int sa)
        {
            int s = sa % 60;
            int m = (int)(sa / 60) % 60;
            int h = (int)(sa / 60 / 60) % 24;
            System.DateTime dt = new System.DateTime(2018, 1, 1, h, m, s);
            return dt.ToString("HH:mm:ss");
        }

        /*
            public static RW.RWEvents.dataRecord readDataRecord(string fileName)
            {
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs, true);
                string jsonString = "";
                while (sr.EndOfStream != true)
                {
                    jsonString += sr.ReadLine();
                }
                sr.Close();

                return JsonUtility.FromJson<RW.RWEvents.dataRecord>(jsonString);
            }
        */
        public static string join(List<string> list)
        {
            string result = "";
            foreach (string item in list) result += (result.Length > 0 ? "," : "") + item;
            return result;
        }

        internal static float LineRendererLength(LineRenderer lr)
        {
            Vector3[] pl = new Vector3[lr.positionCount];
            lr.GetPositions(pl);
            float length = 0;

            for (int i = 1; i < pl.Length; i++)
                length += (pl[i - 1] - pl[i]).magnitude;

            return length;
        }

        public enum BlendMode
        {
            Opaque,
            Cutout,
            Fade,
            Transparent
        }

        public static GameObject copyComponent(GameObject ga, Component comp)
        {
            Component new_component = ga.AddComponent(comp.GetType());
            foreach (var prop in new_component.GetType().GetProperties())
            {
                if (prop.CanRead && prop.CanWrite)
                {
                    prop.SetValue(comp, prop.GetValue(comp, null), null);
                }
            }
            return ga;
        }

        public static void clearScripts(GameObject ga)
        {
            MonoBehaviour[] scripts = ga.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                if (script.GetType() != typeof(BoxCollider))
                {
                    try
                    {
                        MonoBehaviour.Destroy(script);
                        Debug.Log(script.GetType());
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);
                    }
                }
            }
            scripts = ga.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts) MonoBehaviour.Destroy(script);
        }

        public static void ChangeRenderMode(Material standardShaderMaterial, BlendMode blendMode)
        {
            switch (blendMode)
            {
                case BlendMode.Opaque:
                    standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    standardShaderMaterial.SetInt("_ZWrite", 1);
                    standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                    standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                    standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    standardShaderMaterial.renderQueue = -1;
                    break;
                case BlendMode.Cutout:
                    standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    standardShaderMaterial.SetInt("_ZWrite", 1);
                    standardShaderMaterial.EnableKeyword("_ALPHATEST_ON");
                    standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                    standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    standardShaderMaterial.renderQueue = 2450;
                    break;
                case BlendMode.Fade:
                    standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    standardShaderMaterial.SetInt("_ZWrite", 0);
                    standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                    standardShaderMaterial.EnableKeyword("_ALPHABLEND_ON");
                    standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    standardShaderMaterial.renderQueue = 3000;
                    break;
                case BlendMode.Transparent:
                    standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    standardShaderMaterial.SetInt("_ZWrite", 0);
                    standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                    standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                    standardShaderMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    standardShaderMaterial.renderQueue = 3000;
                    break;
            }

        }

        public static Texture2D toTexture2D(RenderTexture rTex)
        {
            Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.ARGB32, false);
            RenderTexture.active = rTex;
            tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            tex.Apply();
            return tex;
        }

        public static Vector3 convertCoordinates(Transform from, Transform to, Vector3 v)
        {
            return to.InverseTransformPoint(from.TransformPoint(v));
        }

        public static Vector3[] convertCoordinates(Transform from, Transform to, Vector3[] v)
        {
            Vector3[] result = new Vector3[v.Length];
            for (int i = 0; i < v.Length; i++)
                result[i] = convertCoordinates(from, to, v[i]);
            return result;
        }

        public static Vector3[] findEdges(Mesh mesh, Vector3 hitPoint, Vector3 hitNormal, int hitTriangleIndex, Vector3 cuttingPlaneNormal)
        {
            float thresholdHearest = 0.01f;

            Plane cutting = new Plane(cuttingPlaneNormal, hitPoint);
            List<Vector3[]> sections = new List<Vector3[]>();
            int hitIndex = -1;


            Vector3[] points = null;

            for (int i = 0; i < mesh.triangles.Length; i += 3)
                if (mesh.normals.Length > i)
                {

                    Vector3 normal = mesh.normals[i];

                    if (Vector3.Dot(normal, hitNormal) > 0.8)
                    {

                        Vector3[] section = new Vector3[2];
                        int si = 0;

                        for (int n = 0; n < 3; n++)
                        {
                            Vector3 p1 = mesh.vertices[mesh.triangles[i + n]];
                            Vector3 p2 = mesh.vertices[mesh.triangles[i + (n + 1) % 3]];
                            Vector3 d = p2 - p1;
                            Ray ray = new Ray(p1, d);
                            float enter;
                            if (cutting.Raycast(ray, out enter))
                            {
                                if (enter <= d.magnitude)
                                {
                                    section[si] = ray.GetPoint(enter);
                                    si++;
                                }
                            }
                        }

                        if (si > 0)
                        {
                            if (hitTriangleIndex * 3 == i) hitIndex = sections.Count;
                            sections.Add(section);
                        }
                    }
                }

            if (hitIndex > -1)
            {
                Vector3 left = sections[hitIndex][0];
                Vector3 right = sections[hitIndex][1];
                sections.RemoveAt(hitIndex);
                int i = -1;
                while (i < sections.Count - 1)
                {
                    i++;
                    for (int n = 0; n < 2; n++)
                    {
                        int ivn = (n + 1) % 2;
                        if ((sections[i][n] - left).magnitude < thresholdHearest)
                        {
                            left = sections[i][ivn];
                            sections.RemoveAt(i);
                            i = -1;
                            break;
                        }

                        if ((sections[i][n] - right).magnitude < thresholdHearest)
                        {
                            right = sections[i][ivn];
                            sections.RemoveAt(i);
                            i = -1;
                            break;
                        }
                    }
                }

                points = new Vector3[2];
                points[0] = left;
                points[1] = right;
            }

            return points;
        }

        public static float distanceToRay(Ray ray, Vector3 point)
        {
            //Рассчитываем расстояние до луча
            Vector3 to_p = point - ray.origin;
            Vector3 pj = Vector3.Project(to_p, ray.direction);
            return Mathf.Sqrt(Mathf.Pow(to_p.magnitude, 2) - Mathf.Pow(pj.magnitude, 2));
        }

        public static Vector3 distancePointToSegment(Vector3 p, Vector3 s1, Vector3 s2) // Еще не тестировал
        {
            Vector3 v = s2 - s1;
            Vector3 w = p - s1;

            float c1 = Vector3.Dot(w, v);
            if (c1 <= 0) return p - s1;
            float c2 = Vector3.Dot(v, v);
            if (c2 <= c1) return p - s2;
            float b = c1 / c2;
            Vector3 pb = p + b * v;
            return p - pb;
        }

        public static Vector3 VectorInverse(Vector3 v)
        {
            return new Vector3(-v.x, -v.y, -v.z);
        }

        public static float AngleAroundAxis(Vector3 dirA, Vector3 dirB, Vector3 axis)
        {
            dirA = dirA - Vector3.Project(dirA, axis);
            dirB = dirB - Vector3.Project(dirB, axis);
            float angle = Vector3.Angle(dirA, dirB);
            return angle * (Vector3.Dot(axis, Vector3.Cross(dirA, dirB)) < 0 ? -1 : 1);
        }

        public static void debugPoint(Vector3 pd, float width)
        {
            Vector3 up = Vector3.up * width * 0.5f;
            Vector3 hp = Vector3.left * width * 0.5f;

            Debug.DrawLine(pd - up, pd + up);
            Debug.DrawLine(pd - hp, pd + hp);
        }

        public static string fullName(Transform trans, Transform relative)
        {
            string path = trans.name;
            while (trans.parent != null)
            {
                trans = trans.parent;
                if (trans == relative) break;
                path = trans.name + "/" + path;
            }

            return path;
        }

        public static Vector3 nearestWithTerrain(Vector3 p, Vector3 direct)
        {
            RaycastHit hit;
            Terrain t;
            Vector3 result = p;

            Ray ray = new Ray(p, direct);

            RaycastHit[] hits = Physics.RaycastAll(ray);

            float minDistance = float.MaxValue;
            for (int i = 0; i < hits.Length; i++)
            {
                hit = hits[i];
                t = hit.collider.GetComponent<Terrain>();
                if (t && (minDistance > hit.distance))
                {
                    result = hit.point;
                    minDistance = hit.distance;
                }
            }

            return result;
        }

        public static bool IsDestroyed(Component component)
        {
            if (!component || (component.ToString() == "null")) return true;

            MonoBehaviour gameObject = component.GetComponent<MonoBehaviour>();

            return gameObject == null && !ReferenceEquals(gameObject, null);
        }

        public static Bounds getTerrainBounds(Transform trans)
        {

            Terrain[] ts = trans.GetComponentsInChildren<Terrain>();

            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach (Terrain t in ts)
            {
                Vector3 leftTop = t.GetPosition();
                Vector3 size = t.terrainData.bounds.size;
                Vector3 rightBottom = leftTop + size;

                if (min.x > leftTop.x) min.x = leftTop.x;
                if (min.y > leftTop.y) min.y = leftTop.y;
                if (min.z > leftTop.z) min.z = leftTop.z;

                if (max.x < rightBottom.x) max.x = rightBottom.x;
                if (max.y < rightBottom.y) max.y = rightBottom.y;
                if (max.z < rightBottom.z) max.z = rightBottom.z;
            }
            return new Bounds((min + max) / 2, max - min);
        }

        public static string Distance(float value)
        {
            if (value < 1000) return Mathf.Round(value) + "м.";
            else return round2(value / 1000, 1) + "км.";
        }

        public static List<FileInfo> getFiles(string relativePath)
        {
            FileInfo[] list = new DirectoryInfo(relativePath).GetFiles();
            List<FileInfo> result = new List<FileInfo>();
            foreach (FileInfo info in list)
            {
                if (info.Name.EndsWith(".json")) result.Add(info);
            }
            return result;
        }
        public static string join(List<string> list, string separate = ",")
        {
            string result = "";
            foreach (string item in list) result += (result.Length > 0 ? separate : "") + item;
            return result;
        }

        public static string join(string[] list, string separate = ",")
        {
            string result = "";
            foreach (string item in list) result += (result.Length > 0 ? separate : "") + item;
            return result;
        }

        internal static List<Vector3> bytesToVectors(byte[] values)
        {
            List<Vector3> result = new List<Vector3>();
            for (int i = 0; i < values.Length; i += 3 * 4)
                result.Add(new Vector3(BitConverter.ToSingle(values, i), BitConverter.ToSingle(values, i + 4), BitConverter.ToSingle(values, i + 8)));
            return result;
        }

        //source: https://stackoverflow.com/questions/1230303/bitconverter-tostring-in-reverse
        internal static byte[] stringToBytes(string values)
        {
            String[] arr = values.Split('-');
            byte[] array = null;
            if (arr.Length > 1)
            {
                array = new byte[arr.Length];
                for (int i = 0; i < arr.Length; i++) array[i] = Convert.ToByte(arr[i], 16);
            }
            else
            {
                int count = values.Length / 2;
                array = new byte[count];
                for (int i = 0; i < count; i++)
                    array[i] = Convert.ToByte(values.Substring(i * 2, 2), 16);
            }
            return array;
        }

        internal static byte[] vectorsToBytes(List<Vector3> values)
        {
            byte[] result = new byte[values.Count * 3 * 4];

            void pushComponents(int n, byte[] x, byte[] y, byte[] z)
            {
                for (int i = 0; i < 4; i++) result[n + i] = x[i];
                for (int i = 0; i < 4; i++) result[n + i + 4] = y[i];
                for (int i = 0; i < 4; i++) result[n + i + 8] = z[i];
            }

            for (int i = 0; i < values.Count; i++)
                pushComponents(i * 3 * 4, BitConverter.GetBytes(values[i].x), BitConverter.GetBytes(values[i].y), BitConverter.GetBytes(values[i].z));


            return result;
        }

        internal static string bytesToString(byte[] arr)
        {
            string result = "";
            for (int i = 0; i < arr.Length; i++)
                result += (arr[i] < 16 ? "0" : "") + Convert.ToString(arr[i], 16);

            return result;
        }

        internal static void LogError(Component cp, string v)
        {
            Debug.LogError(v + " (" + cp.name + " " + cp.GetType().Name + ")");
        }

        internal static Quaternion QuaternionBetween(Vector3 v1, Vector3 v2)
        {
            return Quaternion.AngleAxis(Vector3.Angle(v1, v2), Vector3.Cross(v1, v2));
        }
    }
}