using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

namespace Vmaya.UI.Components
{
    public class Question : ModalWindow 
    {
        public delegate void AnswerToQuestion(bool result);

        [SerializeField]
        private Button okButton;

        [SerializeField]
        private GameObject cancelButton;

        [SerializeField]
        private Component caption;

        private static Question _instance;

        public static Question instance => getInstance();

        private static AnswerToQuestion cur_answer;

        private bool okButtonClick = false;

        public delegate void okButtonClickHandle();
        private okButtonClickHandle okHandle;

        private static Question getInstance()
        {
            if (!_instance)
                _instance = FindObjectOfType<Question>(true);

            return _instance;
        }

        override public void init()
        {
            _instance = this;
        }

        private void Awake()
        {
            okButton.onClick.AddListener(onClick);
            cancelButton.GetComponent<Button>().onClick.AddListener(onCancel);
        }

        public static void Show(string text, AnswerToQuestion answer, bool showCancel, okButtonClickHandle a_handle)
        {
            if (instance)
                instance.showDialog(text, answer, showCancel, a_handle);
            else answer(true);
        }

        public static void Show(string text, AnswerToQuestion answer)
        {

            Show(text, answer, answer != null, null);
        }

        public static void Show(string text)
        {

            Show(text, null, true, null);
        }

        private void showDialog(string text, AnswerToQuestion answer, bool showCancel, okButtonClickHandle a_handle)
        {
            if (gameObject.activeSelf)
            {
                bool checkHide()
                {
                    bool result = !gameObject.activeSelf;
                    if (result)
                        showDialog(text, answer, showCancel, a_handle);
                    return result;
                }
                Vmaya.Utils.Periodical(transform.root.GetComponent<MonoBehaviour>(), checkHide, 0.5f, 10);
            }
            else
            {
                gameObject.SetActive(true);
                Vmaya.Utils.setText(caption, text);
                cur_answer = (answer != null) ? answer : null;
                cancelButton.SetActive(showCancel);
                okHandle = a_handle;
            }
        }

        private void onClick()
        {
            okButtonClick = true;
            if (cur_answer != null) cur_answer(true);
            if (okHandle != null) okHandle();
            hide();
        }

        private void onCancel()
        {
            hide();
        }

        override protected void OnDisable()
        {
            if (!okButtonClick && (cur_answer != null)) cur_answer(false);
            cur_answer = null;
            okButtonClick = false;
            base.OnDisable();
        }
    }
}