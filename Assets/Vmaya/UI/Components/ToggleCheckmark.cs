using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Vmaya.UI
{
    public class ToggleCheckmark : Graphic
    {
        [SerializeField]
        private Animation _animation;

        private bool _isUpdate = false;

        protected override void OnEnable()
        {
            base.OnEnable();
            Vmaya.Utils.setTimeout(this, () => { _isUpdate = true; }, 0.5f);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _isUpdate = false;
        }

        public override void CrossFadeAlpha(float alpha, float duration, bool ignoreTimeScale)
        {
            base.CrossFadeAlpha(alpha, duration, ignoreTimeScale);

            if (_animation)
            {
                bool invert = alpha == 0;
                AnimationState animationState = _animation[_animation.clip.name];

                if (!_isUpdate) animationState.time = invert ? 0 : animationState.length;
                else animationState.time = invert ? animationState.length : 0;

                animationState.speed = invert ? -1 : 1;
                _animation.Play();
            }
        }
    }
}
