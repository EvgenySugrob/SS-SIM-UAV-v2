using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace Vmaya.Scene3D.UI
{
    public class InputControl : MonoBehaviour, ISelectHandler
    {
        public static bool isFocus;

        private void Awake()
        {
            InputField inf = GetComponent<InputField>();
            if (inf)
                inf.onEndEdit.AddListener(OnEndEdit);
            else
            {
                TMP_InputField tmp_inf = GetComponent<TMP_InputField>();
                if (tmp_inf) tmp_inf.onEndEdit.AddListener(OnEndEdit);
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            isFocus = true;
        }

        private void OnEndEdit(string text)
        {
            isFocus = false;
        }
    }
}