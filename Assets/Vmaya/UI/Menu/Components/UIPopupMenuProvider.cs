using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Vmaya.UI.Menu
{
    public class UIPopupMenuProvider : PopupMenuProvider, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            if ((int)eventData.button == MouseButtonCode) Show();
        }
    }
}
