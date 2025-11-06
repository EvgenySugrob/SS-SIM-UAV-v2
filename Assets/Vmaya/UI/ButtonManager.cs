using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Vmaya.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Button))]
    public class ButtonManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private Image _shadowImage;
        [SerializeField]
        private Sprite _defaultSprite;
        [SerializeField]
        private Sprite _disableSprite;
        [SerializeField]
        private string _enterClip;
        [SerializeField]
        private string _exitClip;

        private Button _button => GetComponent<Button>();
        private Animator _animator => GetComponent<Animator>();
        protected bool isAnimation => _animator && (Time.timeScale > 0) && _button.interactable;

        private bool _lastInteractable;

        void Start()
        {
            if (_shadowImage)
                _shadowImage.gameObject.SetActive(false);

            UpdareImage();
        }

        private void UpdareImage()
        {
            Sprite image = _button.interactable ? _defaultSprite : _disableSprite;
            if (image)
                (_button.targetGraphic as Image).sprite = image;

            _lastInteractable = _button.interactable;
        }

        private void Update()
        {
            if (_lastInteractable != _button.interactable) UpdareImage();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_button.interactable)
            {
                if (_shadowImage)
                    _shadowImage.gameObject.SetActive(true);

                if (_animator && !string.IsNullOrEmpty(_enterClip) && isAnimation)
                    _animator.Play(_enterClip);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_shadowImage) _shadowImage.gameObject.SetActive(false);

            if (_animator && !string.IsNullOrEmpty(_exitClip) && isAnimation)
                _animator.Play(_exitClip);
        }
    }
}
