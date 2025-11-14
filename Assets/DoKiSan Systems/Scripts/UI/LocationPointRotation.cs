using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LocationPointRotation : MonoBehaviour
{
    [SerializeField] float duration = 2f;

    [SerializeField] Transform cameraPointLocation;

    void Start()
    {
        transform.DORotate(new Vector3(0,360,0), duration)
        .SetRelative(true)
        .SetEase(Ease.Linear)
        .SetLoops(-1)
        .Play();
    }

    public Transform GetCameraPoint()=>cameraPointLocation;
}
