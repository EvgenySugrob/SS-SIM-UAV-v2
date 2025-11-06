using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Vmaya.Styles
{
    [RequireComponent(typeof(Selectable))]
    public class StyleButton : BaseStyleElement
    {
        protected Selectable selectable => GetComponent<Selectable>();

        public override void ApplyStyle(StyleItem item)
        {
            if (item is StyleButtonItem)
            {
                StyleButtonItem bitem = item as StyleButtonItem;

                ColorBlock colors = selectable.colors;
                colors.normalColor = bitem.BackgroundColor;
                colors.highlightedColor = bitem.HighlightedColor;
                colors.pressedColor = bitem.PressedColor;
                colors.selectedColor = bitem.SelectedColor;
                colors.disabledColor = bitem.DisabledColor;
                selectable.colors = colors;
            }
            else
            {
                Image background = selectable.GetComponent<Image>();
                if (background)
                {
                    if (item.BackgroundSprite)
                        background.sprite = item.BackgroundSprite;

                    background.color = item.BackgroundColor;
                }
            }

            ApplyTextStyle(selectable, item);

        }

        public override void ApplyFromEditor()
        {
            StyleButtonItem item = StyleList.Instance.GetItemButton(Class());

            item.BackgroundColor = selectable.colors.normalColor;
            item.HighlightedColor = selectable.colors.highlightedColor;
            item.PressedColor = selectable.colors.pressedColor;
            item.SelectedColor = selectable.colors.selectedColor;
            item.DisabledColor = selectable.colors.disabledColor;

            StyleItem sitem = default;
            GetTextStyle(selectable, ref sitem);

            item.TextSize = sitem.TextSize;
            item.fontStyle = sitem.fontStyle;
            item.TextColor = sitem.TextColor;

            StyleList.Instance.SetItemButton(Class(), item);
        }
    }
}
