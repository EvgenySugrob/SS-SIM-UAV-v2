using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Vmaya.Styles
{
    public interface IStyleElement
    {
        public string Class();
        public void ApplyStyle(StyleItem item);
        public void OnChangeGroup();
    }

    [System.Serializable]
    public class StyleItem
    {
        public string Class;
        public Color[] colors;
        public Color BackgroundColor;
        public Sprite BackgroundSprite;
        public Color TextColor;
        public int TextSize;
        public FontStyle fontStyle;
    }

    [System.Serializable]
    public class StyleButtonItem: StyleItem
    {
        public Color HighlightedColor;
        public Color PressedColor;
        public Color SelectedColor;
        public Color DisabledColor;
    }

    [System.Serializable]
    public class StyleGroup
    {
        public string NameGroup;
        public List<StyleItem> Items;
        public List<StyleButtonItem> Buttons;
    }
}
