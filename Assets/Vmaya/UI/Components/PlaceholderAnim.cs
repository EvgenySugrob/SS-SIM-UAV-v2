using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

namespace Vmaya.UI.Components
{
    [RequireComponent(typeof(TMP_InputField), typeof(Animator))]
    public class PlaceholderAnim : MonoBehaviour
    {
        [SerializeField]
        private string selectClip;

        [SerializeField]
        private string deselectClip;

        protected Animator animator => GetComponent<Animator>();
        protected TMP_InputField inputField => GetComponent<TMP_InputField>();

        private Graphic _placeholder;

        private void Start()
        {
            if (_placeholder = inputField.placeholder)
            {
                inputField.placeholder = null;

                if (!_placeholder.enabled)
                {
                    _placeholder.enabled = true;
                    animator.Play(selectClip, -1, 1);
                }
            }

            inputField.onSelect.AddListener(onSelect);
            inputField.onDeselect.AddListener(onDeselect);
        }

        private void onSelect(string s)
        {
            if (!string.IsNullOrEmpty(selectClip)) animator.Play(selectClip);
        }

        private void onDeselect(string s)
        {
            if (!string.IsNullOrEmpty(deselectClip) && string.IsNullOrEmpty(inputField.text)) animator.Play(deselectClip);
        }
    }
}
