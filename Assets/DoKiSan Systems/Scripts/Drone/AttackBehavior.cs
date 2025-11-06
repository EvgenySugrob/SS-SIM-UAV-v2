using UnityEngine;

public class AttackBehavior : IDroneBehavior
{
    private DroneBase drone;
    private Transform target;
    private Vector3 exitPoint;
    private bool kamikaze; // true = collide and explode; false = drop bomb then exit
    private float dropHeightOffset = 2f; // how high above target to drop
    private bool completed = false;
    private bool attacked = false;

    // constructor
    public AttackBehavior(Transform target, bool kamikaze = true, Vector3? exitPoint = null)
    {
        this.target = target;
        this.kamikaze = kamikaze;
        this.exitPoint = exitPoint ?? Vector3.zero;
    }

    public void Init(DroneBase drone)
    {
        this.drone = drone;
        // set altitude - randomize a bit
        drone.SetAltitudeTarget(drone.cruiseAltitude + Random.Range(-drone.altitudeRandomRange, drone.altitudeRandomRange));
        // set initial target position to be the target
        drone.SetTargetPosition(GetApproachPoint());
    }

    public void Tick(float dt)
    {
        if (completed || drone == null) return;
        if (attacked)
        {
            // if kamikaze and attacked -> drone already exploded
            // else if not kamikaze -> send to exit and finish
            if (!kamikaze)
            {
                // send to exit point, then finish when reached
                if (Vector3.Distance(drone.transform.position, exitPoint) < 1.5f)
                {
                    completed = true;
                }
            }
            return;
        }

        if (target == null)
        {
            // target lost -> abort
            completed = true;
            return;
        }

        // if close enough to target -> attack
        float dist = Vector3.Distance(drone.transform.position, target.position);
        if (kamikaze)
        {
            // threshold collision distance
            if (dist <= drone.collisionRadius + 1.0f)
            {
                // explode on impact
                drone.Explode();
                attacked = true;
                completed = true;
            }
            else
            {
                // keep heading to target
                drone.SetTargetPosition(target.position);
            }
        }
        else
        {
            // dropping bomb: move to a point above the target and then drop
            Vector3 dropPos = target.position + Vector3.up * dropHeightOffset;
            if (Vector3.Distance(drone.transform.position, dropPos) < 1.5f)
            {
                // perform drop logic
                PerformDrop();
                attacked = true;
                // send to exit
                if (exitPoint != Vector3.zero)
                {
                    drone.SetTargetPosition(exitPoint);
                }
                else
                {
                    completed = true;
                }
            }
            else
            {
                drone.SetTargetPosition(dropPos);
            }
        }
    }

    private Vector3 GetApproachPoint()
    {
        // approach at current altitude target
        return target.position + Vector3.up * drone.cruiseAltitude;
    }

    private void PerformDrop()
    {
        // spawn explosive prefab or call game logic
        GameObject bomb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bomb.transform.position = target.position + Vector3.up * 1.5f;
        bomb.transform.localScale = Vector3.one * 0.3f;
        // simple explosion after delay
        GameObject.Destroy(bomb, 2f);

        // optionally apply damage to target (you must implement)
    }

    public bool IsCompleted => completed;

    public void OnAbort()
    {
        completed = true;
        // clear steering
        drone?.ClearTarget();
    }
}

