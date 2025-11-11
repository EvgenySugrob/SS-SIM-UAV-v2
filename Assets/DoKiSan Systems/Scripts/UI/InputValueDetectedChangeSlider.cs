using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputValueDetectedChangeSlider : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] TMP_InputField copterValue;
    [SerializeField] TMP_InputField planeValue;

    [Header("Slider")]
    [SerializeField] Slider copterSlider;
    [SerializeField] Slider planeSlider;

    void FixedUpdate()
    {
        if(copterSlider.gameObject.activeSelf && planeSlider.gameObject.activeSelf)
        {
            copterValue.text = copterSlider.value.ToString();
            planeValue.text = planeSlider.value.ToString();  
        }
    }
}
