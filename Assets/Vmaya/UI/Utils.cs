using System.Collections;
using UnityEngine;
using System;
using UnityEngine.UI;

namespace Vmaya.UI
{
    public class Utils
    {

        public static void Destroy(Component component)
        {
            if (!Vmaya.Utils.IsDestroyed(component))
            {
                if (Application.isPlaying)
                    GameObject.Destroy(component.gameObject);
                else GameObject.DestroyImmediate(component.gameObject);
            }
        }

        internal static string UniqueName(string v, int inc = 1)
        {
            GameObject ga = GameObject.Find(v);
            if (ga)
            {
                string[] a = ga.name.Split('-');
                int id;
                if (int.TryParse(a[a.Length - 1], out id))
                {
                    inc = id + 1;
                    Array.Resize(ref a, a.Length - 1);
                    v = join(a, "-");
                }
                return UniqueName(v + "-" + inc.ToString(), inc + 1);
            }
            else return v;
        }

        public static string join(string[] list, string separate = ",")
        {
            string result = "";
            foreach (string item in list) result += (result.Length > 0 ? separate : "") + item;
            return result;
        }

        internal static bool Between(int minValue, int maxValue, int index)
        {
            return (index >= minValue) && (index <= maxValue);
        }

        internal static bool Between(float minValue, float maxValue, float value)
        {
            return (value >= minValue) && (value <= maxValue);
        }

        public static Vector3 ClampDelta(Vector3 delta, RectTransform window, RectTransform clampLayer)
        {

            //Bounds bound = RectTransformUtility.CalculateRelativeRectTransformBounds(clampLayer, window);

            Bounds bound = new Bounds(clampLayer.InverseTransformPoint(window.parent.TransformPoint(window.localPosition)), window.rect.size);
            Vector3 csize = clampLayer.rect.size;

            bound.center += delta;

            Vector3 correction = window.rect.size * 0.5f - window.rect.size * window.pivot;


            Vector3 minEdge = bound.min + csize / 2 + correction;
            Vector3 maxEdge = bound.max - csize / 2 + correction;

            if (minEdge.x < 0) delta.x -= minEdge.x;
            if (minEdge.y < 0) delta.y -= minEdge.y;
            if (maxEdge.x > 0) delta.x -= maxEdge.x;
            if (maxEdge.y > 0) delta.y -= maxEdge.y;

            return delta;
        }

        public static void EnsureVisibility(ScrollRect scrollRect, RectTransform child, float padding = 0)
        {
            Debug.Assert(child.parent == scrollRect.content,
                "EnsureVisibility assumes that 'child' is directly nested in the content of 'scrollRect'");

            float viewportHeight = scrollRect.viewport.rect.height;
            Vector2 scrollPosition = scrollRect.content.anchoredPosition;

            float elementTop = child.anchoredPosition.y;
            float elementBottom = elementTop - child.rect.height;

            float visibleContentTop = -scrollPosition.y - padding;
            float visibleContentBottom = -scrollPosition.y - viewportHeight + padding;

            float scrollDelta =
                elementTop > visibleContentTop ? visibleContentTop - elementTop :
                elementBottom < visibleContentBottom ? visibleContentBottom - elementBottom:
                0f;

            scrollPosition.y += scrollDelta;
            scrollRect.content.anchoredPosition = scrollPosition;
        }
    }
}