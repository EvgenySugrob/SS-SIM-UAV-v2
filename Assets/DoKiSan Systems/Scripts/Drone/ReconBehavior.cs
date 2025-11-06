using UnityEngine;

public class ReconBehavior : IDroneBehavior
{
    private DroneBase drone;
    private Transform target;
    private float radius;
    private float altitude;
    private float angularSpeedDegPerSec;
    private float currentAngle; // degrees
    private bool completed = false;

    // configuration
    public ReconBehavior(Transform target, float radius = 10f, float altitude = 15f, float angularSpeedDegPerSec = 30f)
    {
        this.target = target;
        this.radius = Mathf.Max(1f, radius);
        this.altitude = altitude;
        this.angularSpeedDegPerSec = angularSpeedDegPerSec;
        this.currentAngle = Random.Range(0f, 360f);
    }

    public void Init(DroneBase drone)
    {
        this.drone = drone;
        // set altitude target with jitter
        drone.SetAltitudeTarget(altitude, true);
        // initial target pos on orbit
        var pos = ComputeOrbitPosition(currentAngle);
        drone.SetTargetPosition(pos);
    }

    public void Tick(float dt)
    {
        if (completed || drone == null || target == null) return;

        // advance angle
        currentAngle += angularSpeedDegPerSec * dt;
        if (currentAngle >= 360f) currentAngle -= 360f;

        // compute next orbit point (can be per-frame)
        var pos = ComputeOrbitPosition(currentAngle);
        drone.SetTargetPosition(pos);

        // small random altitude jitter occasionally
        if (Random.value < 0.01f)
        {
            drone.SetAltitudeTarget(altitude + Random.Range(-drone.altitudeRandomRange, drone.altitudeRandomRange), false);
        }
    }

    public bool IsCompleted => completed;

    public void OnAbort()
    {
        completed = true;
        // clear steering
        drone?.ClearTarget();
    }

    private Vector3 ComputeOrbitPosition(float angleDeg)
    {
        var center = target.position;
        float rad = angleDeg * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * radius;
        Vector3 pos = center + offset;
        pos.y = altitude + Random.Range(-drone.altitudeRandomRange, drone.altitudeRandomRange);
        return pos;
    }
}
