using System;
using UnityEngine;
using UnityEngine.UI;

namespace Vmaya.UI
{
    public class UIComponent : MonoBehaviour
    {
        public RectTransform Trans => GetComponent<RectTransform>();

        //protected ICursorManager cursorManager => getCursorManager();

        public Vector2 localTo(Vector2 pos, RectTransform rect)
        {
            return rect.InverseTransformPoint(Trans.TransformPoint(pos));
        }

        public static void allPlace(RectTransform cTrans)
        {
            cTrans.anchorMin        = Vector2.zero;
            cTrans.anchorMax        = new Vector2(1, 1);
            cTrans.anchoredPosition    = Vector2.zero;
            cTrans.sizeDelta        = Vector2.zero;
            //cTrans.localPosition = new Vector3(cTrans.rect.width * cTrans.pivot.x, cTrans.rect.height * cTrans.pivot.y);
        }

        public void setTimeout(Action action, float sec)
        {
            Vmaya.Utils.setTimeout(this, action, sec);
        }

        public void fullSpace()
        {
            allPlace(Trans);
        }

        public static float GetScaleFactor()
        {
            float scaleFactor = 1;
            CanvasScaler cs = GameObject.FindAnyObjectByType<CanvasScaler>();

            if (cs)
            {
                switch (cs.uiScaleMode)
                {
                    case CanvasScaler.ScaleMode.ConstantPixelSize:
                        scaleFactor = cs.scaleFactor;
                        break;
                    case CanvasScaler.ScaleMode.ScaleWithScreenSize:
                        scaleFactor = cs.transform.localScale.x;
                        break;
                    case CanvasScaler.ScaleMode.ConstantPhysicalSize:
                        scaleFactor = cs.transform.localScale.x;
                        break;
                }
            }

            return scaleFactor;
        }

        public void SetWorldRect(Rect rect)
        {
            Trans.position = rect.position + rect.size * Trans.pivot;
            Trans.sizeDelta = rect.size / GetScaleFactor();
        }


        public static Rect GetScreenCoordinatesTrans(RectTransform Trans)
        {
            Vector3[] corners = new Vector3[4];
            Trans.GetWorldCorners(corners);
            return new Rect(corners[0].x, corners[0].y, corners[2].x - corners[0].x, corners[2].y - corners[0].y);
        }

        public static void SetScreenCoordinatesTrans(RectTransform Trans, Rect rect)
        {
            /*
            float scaleFactor = 1;
            CanvasScaler cs = Trans.GetComponentInParent<CanvasScaler>();

            if (cs)
            {
                switch (cs.uiScaleMode)
                {
                    case CanvasScaler.ScaleMode.ConstantPixelSize:
                        scaleFactor = cs.scaleFactor;
                        break;
                    case CanvasScaler.ScaleMode.ScaleWithScreenSize:
                        scaleFactor = cs.transform.localScale.x;
                        break;
                    case CanvasScaler.ScaleMode.ConstantPhysicalSize:
                        scaleFactor = cs.transform.localScale.x;
                        break;
                }
            }
            */

            Vector2 size = rect.size;// / scaleFactor;

            Rect pRect = GetScreenCoordinatesTrans(Trans.parent.GetComponent<RectTransform>());

            Vector2 diffSize = pRect.size - size;

            Trans.position  = rect.position + rect.size * Trans.pivot + diffSize;
            Trans.sizeDelta = -diffSize * Trans.anchorMax 
                                + size * new Vector2(1 - Trans.anchorMax.x, 1 - Trans.anchorMax.y)
                                + new Vector2(size.x * Trans.anchorMin.x, pRect.height * Trans.anchorMin.y);
        }

        public Rect GetScreenCoordinates()
        {
            return GetScreenCoordinatesTrans(Trans);
        }

        public void SetScreenCoordinates(Rect rect)
        {
            SetScreenCoordinatesTrans(Trans, rect);
        }

        public void setCursor(string nameCursor)
        {
            if (CursorManager.instance != null) CursorManager.instance.setCursor(nameCursor);
        }
    }
}
