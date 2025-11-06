using UnityEngine;
using DG.Tweening;

public class DroneWing : DroneBase
{
    [Header("Wing (airplane)")]
    public float bankAngleMax = 30f;
    public float bankSpeed = 2f;

    protected override void Awake()
    {
        base.Awake();
        // wing-specific defaults
        maxSpeed = Mathf.Max(12f, maxSpeed);
        turnSpeed = Mathf.Max(45f, turnSpeed);
    }

    protected override void Update()
    {
        base.Update();

        // For wings we can calculate desired bank based on lateral acceleration / turning
        // Example: bank proportional to yaw change - implement per visuals if needed
    }
}

