using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Vmaya.Styles
{
    [RequireComponent(typeof(Selectable))]
    public class StyleDropdown : StyleButton
    {
        [SerializeField]
        private Image _background;

        protected TMP_Dropdown _dropdown => GetComponent<TMP_Dropdown>();

        private bool _tmplActive;
        private StyleButtonItem _bitem;

        protected override void Start()
        {
            base.Start();

            if (_dropdown)
                Utils.Periodical(this, checkShowlist, 0.2f, 0);
        }

        private bool checkShowlist()
        {
            if (_dropdown && (_dropdown.IsExpanded != _tmplActive))
            {
                _tmplActive = _dropdown.IsExpanded;

                if ((_bitem != null) && (_bitem.colors.Length >= 2))
                {
                    Color scolor = _background.color;
                    float i = 0;
                    Color ecolor = _bitem.colors[_tmplActive ? 1 : 0];

                    Utils.Periodical(this, () =>
                    {
                        _background.color = Color.Lerp(scolor, ecolor, i);
                        i += 0.1f;
                        return false;
                    }, 0.05f, 10);
                }
            }
            return false;
        }

        public override void ApplyStyle(StyleItem item)
        {
            if (item is StyleButtonItem)
            {
                _bitem = item as StyleButtonItem;

                ColorBlock colors = selectable.colors;
                colors.normalColor = colors.selectedColor = _bitem.BackgroundColor;
                colors.highlightedColor = _bitem.HighlightedColor;
                colors.pressedColor = _bitem.PressedColor;
                colors.disabledColor = _bitem.DisabledColor;
                selectable.colors = colors;

                if (item.colors.Length > 0)
                    _background.color = item.colors[0];
            } else base.ApplyStyle(item);
        }
    }
}