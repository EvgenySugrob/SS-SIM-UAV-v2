using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Vmaya.Language;
using Vmaya.Scene3D;

namespace Vmaya
{
    public class Utils
    {
        public static char Separator = '#';
        public static float Null = 0.001f;
        public delegate bool CheckCondition();

        internal static string WithMaxLength(string value, int maxLength = 127)
        {
            maxLength -= 3;
            if (value.Length > maxLength)
                return value?.Substring(0, Math.Min(value.Length, maxLength)) + "...";
            else return value;
        }

        public static double round2(double v, int dec)
        {
            return Math.Round(v, dec, System.MidpointRounding.AwayFromZero);
        }

        public static float roundToStep(float v, float step)
        {
            return (step > 0) ? Mathf.Round(v / step) * step : v;
        }

        public static Vector3 roundToStep(Vector3 v, float step)
        {
            if (step > 0)
                return new Vector3(roundToStep(v.x, step), roundToStep(v.y, step), roundToStep(v.z, step));
            else return v;
        }

        public static bool IsDestroyed(GameObject ga)
        {
            if (!ga || (ga.ToString() == "null")) return true;

            return IsDestroyed(ga.transform);
        }

        public static bool IsDestroyed(Component component)
        {
            if (!component || (component.ToString() == "null")) return true;

            MonoBehaviour gameObject = component.GetComponent<MonoBehaviour>();

            return gameObject == null && !ReferenceEquals(gameObject, null);
        }

        public static void Destroy(Component component)
        {
            if (!IsDestroyed(component))
            {
                if (Application.isPlaying)
                    GameObject.Destroy(component.gameObject);
                else GameObject.DestroyImmediate(component.gameObject);
            }
        }

        public static Bounds getRBounds(Transform trans, bool useBoundsComponent = true)
        {
            Vector3 prevPos = trans.position;
            trans.position = Vector3.zero;
            Bounds result = getRBoundsA(trans, useBoundsComponent);
            trans.position = prevPos;
            return result;
        }

        public static Bounds getRBoundsA(Transform trans, bool useBoundsComponent = true)
        {
            Bounds b = new Bounds();
            BoundsComponent bc = trans.GetComponent<BoundsComponent>();

            void Encapsulate(Bounds add)
            {
                if (b.size.sqrMagnitude == 0) b = add;
                else b.Encapsulate(add);
            }

            if (useBoundsComponent && bc) b = bc.bounds;
            else
            {
                MeshRenderer r = trans.GetComponent<MeshRenderer>();
                if (r) Encapsulate(r.bounds);

                for (int i = 0; i < trans.childCount; i++)
                {
                    Transform t = trans.GetChild(i);
                    if (t.name.Substring(0, 1) != "_")
                    {
                        r = t.GetComponent<MeshRenderer>();
                        if (r) Encapsulate(r.bounds);
                        if (t.childCount > 0)
                            Encapsulate(getRBoundsA(t, useBoundsComponent));
                    }
                }
            }
            return b;
        }

        internal static List<T> FindChildAll<T>(Transform transform, string pathName) where T : MonoBehaviour
        {
            List<T> result = new List<T>();
            T[] all = transform.GetComponentsInChildren<T>();

            foreach (T item in all)
                if (item.name.Equals(pathName)) result.Add(item);

            return result;
        }

        internal static List<T> getChildren<T>(Transform transform)
        {
            List<T> result = new List<T>();
            for (int i = 0; i < transform.childCount; i++)
            {
                T t = transform.GetChild(i).GetComponent<T>();
                if (t != null)
                    result.Add(t);
            }

            return result;
        }

        internal static Transform FindChild(Transform transform, string pathName)
        {
            for (int i = 0; i < transform.childCount; i++)
                if (transform.GetChild(i).name.Equals(pathName)) return transform.GetChild(i).transform;

            return null;
        }

        internal static T FindChild<T>(Transform transform, string pathName) where T : Component
        {
            for (int i = 0; i < transform.childCount; i++)
                if (transform.GetChild(i).name.Equals(pathName))
                {
                    T result = transform.GetChild(i).GetComponent<T>();
                    if (result) return result;
                }

            return default;
        }

        internal static bool checkSize(Vector3 size)
        {
            return (size.x != 0) && (size.y != 0) && (size.z != 0);
        }

        public static T Instance<T>(ref T _component) where T : Component
        {
            if (_component == null)
                _component = GameObject.FindObjectOfType<T>();
            return _component;
        }

        public static T Find<T>(Scene scene)
        {
            GameObject[] gas = scene.GetRootGameObjects();
            T result = default;
            foreach (GameObject ga in gas)
                if ((result = ga.GetComponentInChildren<T>()) != null)
                    break;

            return result;
        }

        /*

        public static T Find<T>(string fullPath, ref T _component)
        {
            if ((_component == null) && !string.IsNullOrEmpty(fullPath))
            {
                if ((_component = Utils.Find<T>(fullPath)) == null)
                {
                    Debug.LogError("Not found on path: '" + fullPath + "'");
                }
            }
            return _component;
        }

        public static T Find<T>(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                int sid = path.LastIndexOf(Separator);
                int number = -1;
                if (sid > -1)
                {
                    number = int.Parse(path.Substring(sid + 1));
                    path = path.Substring(0, sid);
                }

                GameObject ga = GameObject.Find(path);
                if (ga && !(ga is T))
                {
                    T[] list = ga.GetComponents<T>();
                    if ((number > -1) && (number <= list.Length))
                        return list[number - 1];

                    return ga.GetComponent<T>();
                }
            }

            return default(T);
        }

        public static string fullName<T>(Component obj, Transform relative = null)
        {
            Transform trans;

            if (obj == null) return null;

            if (obj is Transform) trans = obj as Transform;
            else trans = obj.transform;

            string path = trans.name;
            while (trans.parent != null)
            {
                trans = trans.parent;
                if (trans == relative) break;
                path = trans.name + "/" + path;
            }

            T[] list = obj.GetComponents<T>();

            if (list.Length > 1)
            {
                int number = 0;
                for (int i = 0; i < list.Length; i++)
                {
                    number++;
                    if ((list[i] as Component) == obj)
                    {
                        path += Separator + number.ToString();
                        break;
                    }
                }
            }


            return path;
        }
        */

        internal static T getComponentDepth<T>(Transform cm, int v)
        {
            T t = cm.GetComponentInChildren<T>();
            if (t != null)
            {
                Transform top = (t as MonoBehaviour).transform;
                int i = 0;
                while (i < v)
                {
                    if ((cm == top) || (cm == top.transform.parent))
                        return t;

                    top = top.transform.parent;
                    i++;
                }
            }
            return default;
        }

        public delegate bool UtilFunction();

        public static bool Periodical(MonoBehaviour mbh, UtilFunction afterFunc, float sec, int countLimit)
        {
            void func(int amount = 0)
            {
                if (!afterFunc())
                {
                    if ((countLimit <= 0) || (amount < countLimit))
                        setTimeout(mbh, () => { func(amount + 1); }, sec);
                    else Debug.Log(Lang.instance["Repeat limit exceeded"]);
                }
            }
            return setTimeout(mbh, () => { func(); }, sec);
        }

        public static void PendingCondition(MonoBehaviour mbh, System.Func<bool> checkFunc, Action action)
        {
            IEnumerator proc()
            {
                yield return new WaitUntil(checkFunc);
                action();
            }

            if (checkFunc()) action();
            else mbh.StartCoroutine(proc());
        }

        public static bool setTimeout(MonoBehaviour mbh, Action afterFunc, float sec)
        {
            IEnumerator Waiting(MonoBehaviour a_mbh)
            {
                yield return new WaitForSeconds(sec);
                if (!IsDestroyed(a_mbh))
                    afterFunc();
                else Debug.Log("Destoyed");
            }

            if (!IsDestroyed(mbh) && mbh.gameObject.activeInHierarchy)
            {
                mbh.StartCoroutine(Waiting(mbh));
                return true;
            }
            return false;
        }

        public static bool afterEndOfFrame(MonoBehaviour mbh, Action afterFunc)
        {
            IEnumerator Waiting(MonoBehaviour a_mbh)
            {
                yield return new WaitForEndOfFrame();
                if (!IsDestroyed(a_mbh))
                    afterFunc();
                else Debug.Log("Destoyed");
            }

            if (!IsDestroyed(mbh) && mbh.gameObject.activeInHierarchy)
            {
                mbh.StartCoroutine(Waiting(mbh));
                return true;
            }
            return false;
        }

        public static bool waitUntil(MonoBehaviour mbh, Func<bool> checkFunc,  Action afterFunc)
        {
            IEnumerator Waiting(MonoBehaviour a_mbh)
            {
                yield return new WaitUntil(checkFunc);
                if (!IsDestroyed(a_mbh))
                    afterFunc();
                else Debug.Log("Destoyed");
            }

            if (!IsDestroyed(mbh) && mbh.gameObject.activeInHierarchy)
            {
                mbh.StartCoroutine(Waiting(mbh));
                return true;
            }
            return false;
        }

        internal static T getParent<T>(Transform child)
        {
            if (child.transform.parent)
            {

                T p = child.transform.parent.GetComponent<T>();
                if (p == null) return getParent<T>(child.transform.parent);
                else return p;

            }
            else return default;
        }

        public static Texture2D toTexture2D(RenderTexture rTex)
        {
            Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.ARGB32, false);
            RenderTexture.active = rTex;
            tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            tex.Apply();
            return tex;
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

        public static float AngleAroundAxis(Vector3 dirA, Vector3 dirB, Vector3 axis)
        {
            dirA = dirA - Vector3.Project(dirA, axis);
            dirB = dirB - Vector3.Project(dirB, axis);
            float angle = Vector3.Angle(dirA, dirB);
            return angle * (Vector3.Dot(axis, Vector3.Cross(dirA, dirB)) < 0 ? -1 : 1);
        }

        internal static bool EmptyList(ICollection coll)
        {
            return (coll == null) || (coll.Count == 0);
        }

        public static Color setAlpha(Color inColor, float alpha)
        {
            return new Color(inColor.r, inColor.g, inColor.b, alpha);
        }

        internal static void setInteractable(Component textComponent, bool value)
        {
            if (textComponent)
            {

                float alpha = value ? 1 : 0.5f;

                Text text = textComponent.GetComponentInChildren<Text>();
                if (text) text.color = setAlpha(text.color, alpha);
                else
                {
                    TMP_Text tmptext = textComponent.GetComponentInChildren<TMP_Text>();
                    if (tmptext) tmptext.color = setAlpha(tmptext.color, alpha);
                    else
                    {
                        TextMesh textMesh = textComponent.GetComponentInChildren<TextMesh>();
                        if (textMesh)
                            textMesh.color = setAlpha(textMesh.color, alpha);
                        else
                        {
                            TextMeshPro textMpro = textComponent.GetComponentInChildren<TextMeshPro>();
                            if (textMpro)
                                textMpro.color = setAlpha(textMpro.color, alpha);
                            else
                            {
                                TextMeshProUGUI textMproUGUI = textComponent.GetComponentInChildren<TextMeshProUGUI>();
                                if (textMproUGUI)
                                    textMproUGUI.color = setAlpha(textMproUGUI.color, alpha);
                            }
                        }
                    }
                }
            }
        }

        public static void setText(Component textComponent, string value)
        {
            if (textComponent)
            {
                InputField ifield = textComponent.GetComponentInChildren<InputField>();
                if (ifield) ifield.text = value;
                else
                {
                    TMP_InputField tmpif = textComponent.GetComponentInChildren<TMP_InputField>();
                    if (tmpif) tmpif.text = value;
                    else
                    {
                        Text text = textComponent.GetComponentInChildren<Text>();
                        if (text) text.text = value;
                        else
                        {
                            TextMesh textMesh = textComponent.GetComponentInChildren<TextMesh>();
                            if (textMesh) textMesh.text = value;
                            else
                            {
                                TMP_Text tmptext = textComponent.GetComponentInChildren<TMP_Text>();
                                if (tmptext) tmptext.text = value;
                            }
                        }
                    }
                }
            }
        }

        public static string getText(Component textComponent)
        {
            if (textComponent)
            {
                InputField ifield = textComponent.GetComponentInChildren<InputField>();
                if (ifield) return ifield.text;
                else
                {
                    TMP_InputField tmpif = textComponent.GetComponentInChildren<TMP_InputField>();
                    if (tmpif) return tmpif.text;
                    else
                    {
                        Text text = textComponent.GetComponent<Text>();
                        if (text) return text.text;
                        else
                        {
                            TMP_Text tmptext = textComponent.GetComponent<TMP_Text>();
                            if (tmptext) return tmptext.text;
                        }
                    }
                }
            }
            return null;
        }

        public static void debugPoint(Vector3 pd, float width, Color color = default, float duration = 0)
        {
            if (color == default) color = Color.white;

            Vector3 up = Vector3.up * width * 0.5f;
            Vector3 hp = Vector3.left * width * 0.5f;

            Debug.DrawLine(pd - up, pd + up, color, duration);
            Debug.DrawLine(pd - hp, pd + hp, color, duration);
        }

        internal static void debugBox(Transform transform, Vector3 size, Color color = default, float duration = 0)
        {
            Vector3 size2 = size / 2;
            Vector3 p1 = transform.TransformPoint(new Vector3(-size2.x, -size2.y , -size2.z));
            Vector3 p2 = transform.TransformPoint(new Vector3(size2.x, -size2.y, -size2.z));
            Vector3 p3 = transform.TransformPoint(new Vector3(-size2.x, size2.y, -size2.z));
            Vector3 p4 = transform.TransformPoint(new Vector3(size2.x, size2.y, -size2.z));
            Vector3 p5 = transform.TransformPoint(new Vector3(-size2.x, -size2.y, size2.z));
            Vector3 p6 = transform.TransformPoint(new Vector3(size2.x, -size2.y, size2.z));
            Vector3 p7 = transform.TransformPoint(new Vector3(-size2.x, size2.y, size2.z));
            Vector3 p8 = transform.TransformPoint(new Vector3(size2.x, size2.y, size2.z));

            Debug.DrawLine(p1, p2, color, duration);
            Debug.DrawLine(p2, p4, color, duration);
            Debug.DrawLine(p4, p3, color, duration);
            Debug.DrawLine(p3, p1, color, duration);

            Debug.DrawLine(p5, p6, color, duration);
            Debug.DrawLine(p6, p8, color, duration);
            Debug.DrawLine(p8, p7, color, duration);
            Debug.DrawLine(p7, p5, color, duration);

            Debug.DrawLine(p1, p5, color, duration);
            Debug.DrawLine(p2, p6, color, duration);
            Debug.DrawLine(p3, p7, color, duration);
            Debug.DrawLine(p4, p8, color, duration);
        }

        internal static bool EmptyArray(Array array)
        {
            return (array == null) || (array.Length == 0);
        }

        public static void createColliders(Transform trans, List<MeshCollider> list)
        {
            if (trans.name[0] != '_')
            {
                MeshFilter filter = trans.GetComponent<MeshFilter>();
                if (filter)
                {
                    if (trans.GetComponent<MeshCollider>() == null)
                        list.Add(trans.gameObject.AddComponent<MeshCollider>());
                }
                for (int i = 0; i < trans.childCount; i++)
                    createColliders(trans.GetChild(i), list);
            }
        }

        public static bool findFutureCollisions(Collider myCol, ref Vector3 a_delta, Transform root = null, Quaternion newRotation = default, Transform topParent = null)
        {
            if (newRotation.Equals(default))
                newRotation = myCol.transform.rotation;

            if (root == null) root = myCol.transform.root;
            if (topParent == null) topParent = myCol.transform;

            bool result = false;
            if (myCol != null)
            {
                Collider[] cols = root.GetComponentsInChildren<Collider>();
                foreach (Collider col in cols)
                {
                    if (!col.transform.IsChildOf(topParent))
                    {
                        Vector3 direct;
                        float distance;
                        if (Physics.ComputePenetration(myCol, myCol.transform.position + a_delta, newRotation,
                                                    col, col.transform.position, col.transform.rotation, out direct, out distance))
                        {
                            a_delta += direct * distance;
                            result = true;
                        }
                    }
                }
            }
            return result;
        }

        internal static void AnimationPlay(Animation animation, bool invert = false, Action afterPlay = null, float speed = 1)
        {
            if (animation.clip)
            {
                AnimationState animationState = animation[animation.clip.name];
                animationState.time = invert ? animationState.length : 0;
                animationState.speed = invert ? -speed : speed;
                animation.Play();

                if (afterPlay != null)
                    setTimeout(animation.GetComponent<MonoBehaviour>(), afterPlay, animationState.length);
            }
            else Debug.Log("You must set the value of the \"clip\" parameter");
        }

        internal static AnimationClip GetAnimationClip(Animator animator, string name)
        {
            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
                if (clip.name == name) return clip;

            return null;
        }

#if UNITY_EDITOR
        internal static bool PrefabModeIsActive()
        {
            return UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null;
        }
#endif
    }
}
