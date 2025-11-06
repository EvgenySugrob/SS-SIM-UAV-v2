using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vmaya.UI.Components
{
    public class ModalWindow : Window
    {
        override protected void doShow()
        {
            base.doShow();
            doShowModal();
        }

        virtual protected void doShowModal()
        {
            if (manager) manager.showCurtain();
            GetComponent<RectTransform>().SetAsLastSibling();
        }

        virtual protected void OnDisable()
        {
            if (manager) manager.requireHideCurtain();
        }
    }
}