using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Vmaya.UI.Controls
{
    public class SliderHandle : SliderValue, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private Graphic _shadow;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_shadow) _shadow.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_shadow) _shadow.gameObject.SetActive(false);
        }
    }
}
