using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Vmaya.UI.Controls
{
    public class SliderValue : MonoBehaviour
    {
        [SerializeField]
        private int accuracy = 0;
        private Slider _slider => GetComponentInParent<Slider>();
        private void Awake()
        {
            _slider.onValueChanged.AddListener(onValueChanged);
            updateValue();
        }

        private void onValueChanged(float value)
        {
            updateValue();
        }

        private void updateValue()
        {
            Vmaya.Utils.setText(this, Vmaya.Utils.round2(_slider.value, accuracy).ToString());
        }
    }
}
