using System;
using UnityEngine;
using UnityEngine.UI;

namespace Vmaya.UI.UIAmin
{
    public class ButtonRound : MonoBehaviour
    {
        [SerializeField]
        private float _maxScale = 2;
        [SerializeField]
        private float _maxAlpha = 0.5f;
        [SerializeField]
        private float _speed = 7;
        private Button _parent => GetComponentInParent<Button>();
        protected bool isAnimation => (Time.timeScale > 0) && (_speed > 0) && _parent.enabled;

        private float _scale;
        private bool _play;

        protected Image _image => GetComponent<Image>();
        protected float _alpha { get => _image.color.a; set
            {
                Color color = _image.color;
                color.a = value;
                _image.color = color;
            } 
        }


        private void Awake()
        {
            _parent.onClick.AddListener(onClick);
        }

        private void onClick()
        {
            transform.position = VMouse.mousePosition;
            if (isAnimation)
                beginPlay();
        }

        private void beginPlay()
        {
            _play = true;
            updateRound(_scale = 0);
        }

        private void Update()
        {
            if (_play)
            {
                if (_scale < _maxScale) updateRound(_scale + _speed * Time.deltaTime);
                else
                {
                    updateRound(0);
                    _play = false;
                }
            }
        }

        private void updateRound(float value)
        {
            _scale = value;
            transform.localScale = new Vector3(_scale, _scale, _scale);
            _alpha = Mathf.Sin(_scale / _maxScale * Mathf.PI) * _maxAlpha;
        }
    }
}
