using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using Vmaya;

namespace cakeslice
{
    public class Toggle : MonoBehaviour
    {
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if(VKeyboard.GetKeyDown(
#if ENABLE_INPUT_SYSTEM
                Key.K
#else
                KeyCode.K
#endif
            ))
            {
                GetComponent<Outline>().enabled = !GetComponent<Outline>().enabled;
            }
        }
    }
}