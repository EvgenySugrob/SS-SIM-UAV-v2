using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vmaya.Util;

namespace Vmaya.UI.Components
{
    public class SliderInputValue : MonoBehaviour
    {
        [SerializeField]
        private Slider _slider;

        [SerializeField]
        private int _accuracy = 2;

        private void OnValidate()
        {
            if (!_slider) _slider = GetComponentInParent<Slider>();
        }

        private void Awake()
        {
            if (_slider)
            {
                _slider.onValueChanged.AddListener(onValueChangedFromSlider);
                InputUtils.setListener(this, onValueChangeFromInput);
                UpdateText(_slider.value);
            }
        }
        private void onValueChangeFromInput(string v)
        {
            float value;
            if (float.TryParse(v, out value) && (value != _slider.value))
                _slider.value = value;
        }

        private void onValueChangedFromSlider(float v)
        {
            UpdateText(v);
        }

        private void UpdateText(float v)
        {
            InputUtils.setValue(this, Vmaya.Utils.round2(v, _accuracy).ToString());
        }
    }
}
