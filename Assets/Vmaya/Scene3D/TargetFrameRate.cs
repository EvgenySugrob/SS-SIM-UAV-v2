using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vmaya.Scene3D
{
    public class TargetFrameRate : MonoBehaviour
    {
        [Header("The restriction only works in the editor")]
        [SerializeField]
        private int _targetFrameRate = 100;
        [SerializeField]
        [Range(0, 1)]
        private float _timeScale = 1f;

        private int _lastTragetFrameRate;
        private float _lastTimeScale;

#if (UNITY_EDITOR)
        private void Update()
        {
            if (_targetFrameRate != _lastTragetFrameRate)
                Application.targetFrameRate = _lastTragetFrameRate = _targetFrameRate;

            if (_lastTimeScale != _timeScale)
                Time.timeScale = _lastTimeScale = _timeScale;
        }
#endif
    }
}
