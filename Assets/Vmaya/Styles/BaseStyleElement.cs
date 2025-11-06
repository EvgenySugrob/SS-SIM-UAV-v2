using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Vmaya.Styles
{
    public abstract class BaseStyleElement : MonoBehaviour, IStyleElement
    {
        [SerializeField]
        private string _class;

        public static float DISABLEALPHA = 0.4f;

        private static List<Selectable> _trackElements = new List<Selectable>();

        public static void ApplyTextStyleA(Component a_textComponent, StyleItem item, Color textColor)
        {
            Text[] texts = a_textComponent.GetComponentsInChildren<Text>();
            foreach (Text text in texts)
            {
                text.fontSize = item.TextSize;
                text.fontStyle = item.fontStyle;
                text.color = textColor;
            }

            TextMesh[] textMeshs = a_textComponent.GetComponentsInChildren<TextMesh>();
            foreach (TextMesh textMesh in textMeshs)
            {
                textMesh.fontSize = item.TextSize;
                textMesh.fontStyle = item.fontStyle;
                textMesh.color = textColor;
            }

            TextMeshPro[] textMpros = a_textComponent.GetComponentsInChildren<TextMeshPro>();
            foreach (TextMeshPro textMpro in textMpros)
            {
                textMpro.fontSize = item.TextSize;
                textMpro.fontStyle = (FontStyles)item.fontStyle;
                textMpro.color = textColor;
            }

            TextMeshProUGUI[] textMproUGUIs = a_textComponent.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (TextMeshProUGUI textMproUGUI in textMproUGUIs)
            {
                textMproUGUI.fontSize = item.TextSize;
                textMproUGUI.fontStyle = (FontStyles)item.fontStyle;
                textMproUGUI.color = textColor;
            }
        }

        protected static void trackElement(Selectable selectable, StyleItem item)
        {
            bool curInter = selectable.interactable;
            _trackElements.Add(selectable);

            Utils.Periodical(selectable.transform.root.GetComponent<MonoBehaviour>(), () =>
            {
                if (!Vmaya.Utils.IsDestroyed(selectable))
                {
                    if (curInter != selectable.interactable)
                    {
                        curInter = selectable.interactable;
                        ApplyTextStyleA(selectable, item, Utils.setAlpha(item.TextColor, selectable.interactable ? item.TextColor.a : item.TextColor.a * DISABLEALPHA));
                    }
                    return false;
                }
                else
                {
                    _trackElements.Remove(selectable);
                    return true;
                }
            }, 0.2f, 0);
        }

        public static void ApplyTextStyle(Component a_textComponent, StyleItem item)
        {
            if (a_textComponent)
            {
                Color TextColor = item.TextColor;
                Selectable selectable = a_textComponent.GetComponentInParent<UnityEngine.UI.Selectable>();
                if (selectable)
                {
                    if (_trackElements.IndexOf(selectable) == -1)
                        trackElement(selectable, item);

                    TextColor = Utils.setAlpha(item.TextColor, selectable.interactable ? item.TextColor.a : item.TextColor.a * DISABLEALPHA);
                }
                ApplyTextStyleA(a_textComponent, item, TextColor);
            }
        }

        public static void GetTextStyle(Component a_textComponent, ref StyleItem item)
        {
            if (a_textComponent)
            {
                Text[] texts = a_textComponent.GetComponentsInChildren<Text>();
                if (texts.Length > 0)
                {
                    foreach (Text text in texts)
                    {
                        item.TextSize = text.fontSize;
                        item.fontStyle = text.fontStyle;
                        item.TextColor = text.color;
                    }
                }
                else
                {
                    TextMesh[] textMeshs = a_textComponent.GetComponentsInChildren<TextMesh>();
                    if (textMeshs.Length > 0)
                    {
                        foreach (TextMesh textMesh in textMeshs)
                        {
                            item.TextSize = textMesh.fontSize;
                            item.fontStyle = textMesh.fontStyle;
                            item.TextColor = textMesh.color;
                        }
                    }
                    else
                    {
                        TextMeshPro[] textMpros = a_textComponent.GetComponentsInChildren<TextMeshPro>();
                        if (textMpros.Length > 0)
                        {
                            foreach (TextMeshPro textMpro in textMpros)
                            {
                                item.TextSize = (int)textMpro.fontSize;
                                item.fontStyle = (FontStyle)textMpro.fontStyle;
                                item.TextColor = textMpro.color;
                            }
                        }
                        else
                        {
                            TextMeshProUGUI[] textMproUGUIs = a_textComponent.GetComponentsInChildren<TextMeshProUGUI>();
                            if (textMproUGUIs.Length > 0)
                            {
                                foreach (TextMeshProUGUI textMproUGUI in textMproUGUIs)
                                {
                                    item.TextSize = (int)textMproUGUI.fontSize;
                                    item.fontStyle = (FontStyle)textMproUGUI.fontStyle;
                                    item.TextColor = textMproUGUI.color;
                                }
                            }
                        }
                    }
                }
            }
        }

        protected virtual void Start()
        {
            if (StyleList.Instance) 
                StyleList.Instance.Apply(this);
        }

        public abstract void ApplyFromEditor();

        public abstract void ApplyStyle(StyleItem item);

        public string Class()
        {
            return _class;
        }

        public void OnChangeGroup()
        {
            if (StyleList.Instance) StyleList.Instance.Apply(this);
        }

        private void OnEnable()
        {
            if (StyleList.Instance) StyleList.Instance.Apply(this);
        }
    }
}
