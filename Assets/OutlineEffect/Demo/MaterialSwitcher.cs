using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using Vmaya;

namespace cakeslice
{
    public class MaterialSwitcher : MonoBehaviour
    {
        public Material target;
        public int index;

        public void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if(VKeyboard.GetKeyDown(Key.M))
#else
            if (VKeyboard.GetKeyDown(KeyCode.M))
#endif
            {
                Material[] materials = GetComponent<Renderer>().materials;
                materials[index] = target;
                GetComponent<Renderer>().materials = materials;
            }
        }
    }
}