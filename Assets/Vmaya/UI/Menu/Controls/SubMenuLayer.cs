using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Vmaya.UI.Menu
{
    public class SubMenuLayer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private bool _focus;

        private static SubMenuLayer _currentSubmenu;
        public static SubMenuLayer CurrentSubmenu => _currentSubmenu;
        private Animation _animation => GetComponent<Animation>();
        private AnimationState _animationState => _animation[_animation.clip.name];
        protected bool isAnimation => _animation && (Time.timeScale > 0);

        public void OnPointerEnter(PointerEventData eventData)
        {
            _focus = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _focus = false;
            Vmaya.Utils.setTimeout(this, () =>
            {
                if (!_focus) HideCurrentSubmenu();
            }, 0.3f);
        }

        public void Show()
        {
            if (_currentSubmenu != this)
            {
                gameObject.SetActive(true);
                if (_currentSubmenu)
                {
                    _currentSubmenu.hide();

                    if (isAnimation)
                    {
                        _animationState.speed = 1;
                        _animationState.time = _animationState.length;

                        _animation.Play();
                    }
                }
                else if (isAnimation) Vmaya.Utils.AnimationPlay(_animation);

                _currentSubmenu = this;
            }
        }

        private void hide()
        {
            if (gameObject.gameObject.activeSelf)
            {
                _focus = false; 
                gameObject.SetActive(false);
            }
        }

        internal static void HideCurrentSubmenu()
        {
            if (_currentSubmenu)
            {
                if (_currentSubmenu.isAnimation)
                {
                    SubMenuLayer lastLayer = _currentSubmenu;

                    Vmaya.Utils.AnimationPlay(_currentSubmenu._animation, true, () =>
                    {
                        lastLayer.hide();
                    });
                }
                else _currentSubmenu.hide();

                _currentSubmenu = null;
            }
        }
    }
}
