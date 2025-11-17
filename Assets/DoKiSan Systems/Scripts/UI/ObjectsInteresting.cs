using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsInteresting : MonoBehaviour
{
    [Header("AllObjectsInteresting")]
    [SerializeField] List<ObjectOfInterest> objectOfInterests;

    public List<ObjectOfInterest> GetObjectOfInterests() => objectOfInterests;
}
