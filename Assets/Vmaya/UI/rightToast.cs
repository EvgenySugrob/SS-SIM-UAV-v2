using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Vmaya.Language;

namespace Vmaya.UI
{
    [RequireComponent(typeof(Text))]
    public class rightToast : MonoBehaviour
    {
        [SerializeField]
        private int waitTime = 10;
        private static rightToast _instance;
        private float _prevTime;
        private bool _show;

        private void Awake()
        {
            if (_instance && (_instance != this))
                Debug.LogError(Lang.instance["There must be one instance"]);

            _instance = this;
        }

        public static void setText(string text)
        {
            if (_instance)
                _instance.startText(text);
        }

        public void startText(string text)
        {
            GetComponent<Text>().text = text;
            _prevTime = Time.fixedTime;
            if (!_show)
            {
                StartCoroutine(checkWait());
                _show = true;
            }
        }

        private IEnumerator checkWait()
        {
            yield return new WaitForSeconds(1);
            if (Time.fixedTime - _prevTime < waitTime) StartCoroutine(checkWait());
            else
            {
                _show = false;
                GetComponent<Text>().text = "";
            }
        }
    }
}