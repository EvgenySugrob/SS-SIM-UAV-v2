using UnityEngine;
using UnityEngine.UI;

namespace Vmaya.Styles
{
    [ExecuteInEditMode]
    public class StyleElement : BaseStyleElement
    {
        [SerializeField]
        private Image _backgroundComponent;
        [SerializeField]
        private Component _textComponent;

        private void OnValidate()
        {
            if (!_backgroundComponent) _backgroundComponent = GetComponent<Image>();
            if (!_textComponent) _textComponent = GetComponent<Text>();
        }

        public override void ApplyStyle(StyleItem item)
        {
            if (_backgroundComponent)
            {
                if (item.BackgroundSprite)
                    _backgroundComponent.sprite = item.BackgroundSprite;

                _backgroundComponent.color = item.BackgroundColor;
            }

            ApplyTextStyle(_textComponent, item);
        }

        public override void ApplyFromEditor()
        {
            StyleItem item = StyleList.Instance.GetItem(Class());

            if (_backgroundComponent)
            {
                item.BackgroundColor = _backgroundComponent.color;
                item.BackgroundSprite = _backgroundComponent.sprite;
            }
            
            GetTextStyle(_textComponent, ref item);

            StyleList.Instance.SetItem(Class(), item);
        }
    }
}
