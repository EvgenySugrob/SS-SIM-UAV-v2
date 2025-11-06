using UnityEngine;

public class DroneCopter : DroneBase
{
    [Header("Copter")]
    public float hoverStability = 2f;
    public float tiltAmplitude = 10f;

    protected override void Awake()
    {
        base.Awake();
        // copter-specific defaults
        maxSpeed = Mathf.Max(4f, maxSpeed);
        turnSpeed = Mathf.Max(80f, turnSpeed);
    }

    protected override void Update()
    {
        base.Update();

        // simple visual tilt towards movement direction if using SteeringSensor (optional)
        // If you want advanced physics, tie into Rigidbody forces.
    }
}

