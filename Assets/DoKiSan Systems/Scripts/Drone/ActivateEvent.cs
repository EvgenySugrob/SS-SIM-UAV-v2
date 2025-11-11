using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateEvent : MonoBehaviour
{
    public enum TypeEvent
    {
        Attack,
        Kamikaze
    }

    [SerializeField] TypeEvent typeEvent;

    public TypeEvent GetTypeEvent() => typeEvent;
    
}
