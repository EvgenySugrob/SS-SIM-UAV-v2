using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Vmaya.Util
{
    public class InputUtils
    {
        public static string getValue(Component textComponent)
        {
            if (textComponent)
            {
                InputField text = textComponent.GetComponent<InputField>();
                if (text) return text.text;
                else
                {
                    TMP_InputField tmptext = textComponent.GetComponent<TMP_InputField>();
                    if (tmptext) return tmptext.text;
                }
            }
            return null;
        }
        public static void setValue(Component textComponent, string value)
        {
            if (textComponent)
            {
                InputField text = textComponent.GetComponent<InputField>();
                if (text) text.text = value;
                else
                {
                    TMP_InputField tmptext = textComponent.GetComponent<TMP_InputField>();
                    if (tmptext) tmptext.text = value;
                }
            }
        }
        public static void setListener(Component textComponent, UnityAction<string> onListener)
        {
            if (textComponent)
            {
                InputField text = textComponent.GetComponent<InputField>();
                if (text) text.onValueChanged.AddListener(onListener);
                else
                {
                    TMP_InputField tmptext = textComponent.GetComponent<TMP_InputField>();
                    if (tmptext) tmptext.onValueChanged.AddListener(onListener);
                }
            }
        }
    }
}
