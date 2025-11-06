using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Vmaya.UI {
    public class RectTransformAnim : UIComponent
    {

        [SerializeField]
        private float _duration;
        public float Duration => _duration;

        public bool isAnimation => (_duration > 0) && (Time.timeScale > 0);

        [SerializeField]
        private EasingFunction.Ease _easeMethod;

        private Rect _startRect;
        private Rect _toRect;
        private float _idx = -1;

        public bool isPlay => _idx > -1;

        public UnityEvent onStateChange;

        void Update()
        {
            if (isPlay)
            {
                if (_idx == 0) startAnim();

                _idx += Time.deltaTime * Time.timeScale;
                if (_idx > _duration)
                {
                    _idx = -1;
                    endAnim();
                }
                else
                {
                    float _eidx = EasingFunction.GetEasingFunction(_easeMethod)(0, 1, _idx / _duration);
                    SetScreenCoordinates(new Rect(Vector2.Lerp(_startRect.position, _toRect.position, _eidx),
                                          Vector2.Lerp(_startRect.size, _toRect.size, _eidx)));
                }
            }
        }

        private void setOverrideSorting(bool value)
        {
            Canvas canvas = GetComponentInChildren<Canvas>();
            if (canvas) canvas.overrideSorting = value;
        }

        protected virtual void startAnim()
        {
            SetScreenCoordinates(_startRect);
            setOverrideSorting(false);
            onStateChange.Invoke();
        }

        protected virtual void endAnim()
        {
            SetScreenCoordinates(_toRect);
            setOverrideSorting(true);
            onStateChange.Invoke();
        }

        public void PlayTo(Rect a_startRect, Rect a_toRect)
        {
            if (!isPlay)
                _startRect = a_startRect;

            _toRect = a_toRect;
            if (isAnimation)
                _idx = 0;
            else
            {
                _idx = -1;
                endAnim();
            }
        }
    }
}
