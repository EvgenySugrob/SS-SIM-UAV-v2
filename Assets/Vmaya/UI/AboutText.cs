using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vmaya.UI
{
    public class AboutText : MonoBehaviour
    {
        private void Awake()
        {
            string source = Vmaya.Utils.getText(this);
            if (!string.IsNullOrEmpty(source))
            {
                source = source.Replace("{AppVersion}", Application.version);
                source = source.Replace("{UnityVersion}", Application.unityVersion);
                source = source.Replace("{ProductName}", Application.productName);
                Vmaya.Utils.setText(this, source);
            }
        }
    }
}
