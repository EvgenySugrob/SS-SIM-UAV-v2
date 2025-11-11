using UnityEngine;
using Micosmo.SensorToolkit;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(SteeringSensor))]
[RequireComponent(typeof(Rigidbody))]
public class DroneAI : MonoBehaviour
{
    public enum DroneFlightBehavior { Recon, Attack }

    [Header("Refs (assign in inspector)")]
    public SteeringSensor steering;
    public Rigidbody rb;
    public TriggerSensor interactionTrigger;  // optional: child trigger for precise "in-zone" check

    [Header("Path & behavior")]
    public FlightPath assignedPath;           // assign by spawner (prefab instance of FlightPath or prefab instance already spawned)
    public DroneFlightBehavior behaviour = DroneFlightBehavior.Recon;
    public Transform overrideAttackTarget;    // optional direct target (do not change the FlightPath)
    public Vector3? exitPointOverride;        // optional exit point (from ScenarioManager)

    [Header("Movement")]
    public float arriveDistance = 2f;
    public float waypointTimeout = 15f;
    public float destroyAfterExit = 25f;

    [Header("Attack / Drop")]
    public GameObject dropPrefab;
    public float dropYOffset = -1f;

    [SerializeField] BombDrop bombDrop;
    [SerializeField] KamikazeActive kamikazeActive;

    [Header("Debug")]
    public bool verboseLogs = false;

    // runtime
    [SerializeField] int currentIdx = -1;
    bool following = false;
    bool isAttack = false;

    private void Awake()
    {
        if (steering == null) steering = GetComponent<SteeringSensor>();
        if (rb == null) rb = GetComponent<Rigidbody>();

        // config steering as flying RB locomotion (like example)
        steering.IsSpherical = true;
        steering.LocomotionMode = LocomotionMode.RigidBodyFlying;
        steering.RigidBody = rb;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        StartWithPath(assignedPath, isAttack, overrideAttackTarget, exitPointOverride);
    }

    /// <summary>Call to start following assigned path. Path must be non-null and contains waypoints.</summary>
    public void StartWithPath(FlightPath path, bool attack, Transform overrideTarget = null, Vector3? exitPoint = null)
    {
        if (path == null || path.waypoints == null || path.waypoints.Count == 0)
        {
            Debug.LogWarning("[DroneAI_Example] invalid path assigned.");
            return;
        }

        assignedPath = path;
        isAttack = attack;
        overrideAttackTarget = overrideTarget;
        exitPointOverride = exitPoint;

        currentIdx = -1;
        following = true;
        MoveToNextWaypoint();
    }

    void MoveToNextWaypoint()
    {
        if (!following || assignedPath == null) return;

        currentIdx++;
        // safety: if path finished -> OnPathComplete
        if (currentIdx >= assignedPath.waypoints.Count)
        {
            OnPathComplete();
            return;
        }

        Transform wp = assignedPath.waypoints[currentIdx];
        if (wp == null)
        {
            // skip null waypoints gracefully
            MoveToNextWaypoint();
            return;
        }

        if (verboseLogs)
        {
            Debug.Log($"[DroneAI] {name} -> moving to waypoint {currentIdx} at {wp.name}({wp.position})");
        }

        steering.SeekTo(wp.position);
        // StopAllCoroutines();
        // StartCoroutine(WatchWaypointCoroutine(wp.position, waypointTimeout));
    }

    public void OnDetectedCurrentPoint()
    {
        if(currentIdx<=assignedPath.waypoints.Count)
        {
            if(assignedPath.waypoints[currentIdx].GetComponent<ActivateEvent>())
            {
                ActivateEvent.TypeEvent eventType = assignedPath.waypoints[currentIdx].GetComponent<ActivateEvent>().GetTypeEvent();
                EventStart(eventType);
            }
           
            if (interactionTrigger.IsDetected(assignedPath.waypoints[currentIdx].gameObject))
            MoveToNextWaypoint();  
        }  
    }

    private void EventStart(ActivateEvent.TypeEvent eventType)
    {
        switch (eventType)
        {
            case ActivateEvent.TypeEvent.Attack:
                bombDrop.DropBomb();
                break;
            case ActivateEvent.TypeEvent.Kamikaze:
                kamikazeActive.Kamikaze();
                break;
        }
    }

    IEnumerator WatchWaypointCoroutine(Vector3 targetPos, float timeout)
    {
        float start = Time.time;
        while (true)
        {
            // check distance
            float dist = Vector3.Distance(transform.position, targetPos);

            // also check steering destination flag if available
            bool steeringReached = steering.IsDestinationReached;

            if (dist <= arriveDistance || steeringReached)
            {
                if (verboseLogs) Debug.Log($"[DroneAI] Reached waypoint idx {currentIdx} (dist {dist:F2})");
                MoveToNextWaypoint();
                yield break;
            }

            if (Time.time - start > timeout)
            {
                Debug.LogWarning($"[DroneAI] Waypoint timeout ({name}). Skipping.");
                MoveToNextWaypoint();
                yield break;
            }
            yield return null;
        }
    }

    void OnPathComplete()
    {
        following = false;
        if (verboseLogs) Debug.Log($"[DroneAI] Path complete for {name}. Behavior attack={isAttack}");

        // if (!isAttack)
        // {
        //     FlyToExitAndCleanup();
        //     return;
        // }

        // // attack path type determined by path.pathType
        // if (assignedPath != null)
        // {
        //     if (assignedPath.pathType == FlightPathType.Attack_Drop)
        //     {
        //         DoDrop();
        //         FlyToExitAndCleanup();
        //         return;
        //     }
        //     else if (assignedPath.pathType == FlightPathType.Attack_Kamikaze)
        //     {
        //         if (overrideAttackTarget != null)
        //         {
        //             steering.SeekTo(overrideAttackTarget.position);
        //             // destroy after some safety time to avoid infinite chase
        //             Destroy(gameObject, waypointTimeout + 10f);
        //         }
        //         else
        //         {
        //             FlyToExitAndCleanup();
        //         }
        //         return;
        //     }
        // }

        FlyToExitAndCleanup();
    }

    void DoDrop()
    {
        if (dropPrefab == null) return;
        Vector3 spawn = transform.position + Vector3.up * dropYOffset;
        Instantiate(dropPrefab, spawn, Quaternion.identity);
        if (verboseLogs) Debug.Log($"[DroneAI] Dropped prefab at {spawn}");
    }

    void FlyToExitAndCleanup()
    {
        Vector3 exit;
        if (exitPointOverride.HasValue) exit = exitPointOverride.Value;
        else exit = ComputeExitPoint();

        steering.SeekTo(exit);
        Destroy(gameObject, destroyAfterExit);
    }

    Vector3 ComputeExitPoint()
    {
        if (assignedPath == null || assignedPath.waypoints.Count == 0)
        {
            return transform.position + transform.forward * 200f;
        }

        Vector3 center = Vector3.zero;
        int c = 0;
        foreach (var w in assignedPath.waypoints)
        {
            if (w != null) { center += w.position; c++; }
        }
        if (c == 0) return transform.position + transform.forward * 200f;
        center /= c;
        Vector3 dir = (transform.position - center).normalized;
        if (dir.sqrMagnitude < 0.001f) dir = transform.forward;
        return transform.position + dir * 200f;
    }

    // private void OnCollisionEnter(Collision other)
    // {
    //     // kamikaze collision handling
    //     if (assignedPath != null && assignedPath.pathType == FlightPathType.Attack_Kamikaze)
    //     {
    //         // optionally check tag / team / target type here
    //         Destroy(gameObject);
    //     }
    // }

    
}
