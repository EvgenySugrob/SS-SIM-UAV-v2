using System;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider))]
public class DroneBase : MonoBehaviour
{
    [Header("Common")]
    public float maxSpeed = 8f;
    public float turnSpeed = 60f; // deg/sec for visual rotation/tilt
    public float cruiseAltitude = 10f;
    public float altitudeRandomRange = 2f; // randomize altitude +/- range in meters
    public float collisionRadius = 0.5f;

    [Header("References")]
    public Component steeringSensorComponent; // assign SteeringSensor in inspector (or left null -> auto find)
    // We use dynamic typing to avoid hard compile dependency if namespace differs.
    protected object steeringSensor; // actual SteeringSensor component (from SensorToolkit)

    protected IDroneBehavior behavior;

    protected bool missionActive = false;
    protected bool destroyed = false;

    protected float currentAltitudeTarget;
    protected float lastAltitude;

    protected virtual void Awake()
    {
        // try to auto-assign steeringSensorComponent if not set
        if (steeringSensorComponent == null)
        {
            // try to find a child SteeringSensor by type name (safe if namespace unknown)
            var comp = GetComponentInChildren(typeof(UnityEngine.Component), true); // fallback
        }

        if (steeringSensorComponent == null)
        {
            // try by reflection to find a component named "SteeringSensor"
            var comps = GetComponentsInChildren<Component>(true);
            foreach (var c in comps)
            {
                if (c == null) continue;
                if (c.GetType().Name == "SteeringSensor")
                {
                    steeringSensorComponent = c;
                    break;
                }
            }
        }

        steeringSensor = steeringSensorComponent;
        lastAltitude = transform.position.y;
        currentAltitudeTarget = cruiseAltitude + UnityEngine.Random.Range(-altitudeRandomRange, altitudeRandomRange);
    }

    protected virtual void Update()
    {
        if (destroyed) return;

        // Tick behavior
        if (missionActive && behavior != null && !behavior.IsCompleted)
        {
            behavior.Tick(Time.deltaTime);
        }

        // alt variation: gently move to currentAltitudeTarget via DOTween (visual). Use small smoothing to avoid sudden jumps.
        // We'll lerp altitude using DOVirtual.Float to avoid conflicts.
        float y = transform.position.y;
        if (Mathf.Abs(y - currentAltitudeTarget) > 0.05f)
        {
            // smooth adjust - small tween to move Y (not controlling steeringSensor position directly)
            transform.DOMoveY(Mathf.Lerp(y, currentAltitudeTarget, Time.deltaTime * 1.5f), 0.5f).SetUpdate(true);
        }
    }

    public virtual void StartMission(IDroneBehavior b)
    {
        if (b == null) return;
        behavior = b;
        behavior.Init(this);
        missionActive = true;
        destroyed = false;
        currentAltitudeTarget = cruiseAltitude + UnityEngine.Random.Range(-altitudeRandomRange, altitudeRandomRange);
    }

    public virtual void AbortMission()
    {
        behavior?.OnAbort();
        behavior = null;
        missionActive = false;
    }

    /// <summary>Set scalar target altitude (with optional random jitter)</summary>
    public void SetAltitudeTarget(float altitude, bool randomJitter = true)
    {
        currentAltitudeTarget = altitude + (randomJitter ? UnityEngine.Random.Range(-altitudeRandomRange, altitudeRandomRange) : 0f);
    }

    /// <summary>Set world-space target position. Uses SteeringSensor if present, otherwise fallback.</summary>
    public virtual void SetTargetPosition(Vector3 worldPos)
    {
        // if steeringSensor available -> call its SetSeekTarget API
        if (steeringSensor != null)
        {
            TryCallSteeringSetSeek(worldPos);
            return;
        }

        // fallback simple movement: move transform towards
        StopAllCoroutines();
        // simple immediate movement toward; better to implement Rigidbody movement in concrete classes
        transform.position = Vector3.MoveTowards(transform.position, worldPos, maxSpeed * Time.deltaTime);
    }

    /// <summary>Clear any steering target</summary>
    public virtual void ClearTarget()
    {
        if (steeringSensor != null)
        {
            TryCallSteeringClear();
        }
    }

    /// <summary>Explode and destroy drone (for attack behaviour)</summary>
    public virtual void Explode()
    {
        if (destroyed) return;
        destroyed = true;
        missionActive = false;

        // TODO: spawn VFX, sound, damage logic here
        // Example: instantiate explosive prefab, apply area damage
        var fx = new GameObject("ExplosionFX");
        fx.transform.position = transform.position;
        Destroy(fx, 2f);

        // destroy self
        Destroy(gameObject);
    }

    // Helpers: reflection-based invocation for SteeringSensor API calls
    protected void TryCallSteeringSetSeek(Vector3 pos)
    {
        try
        {
            var type = steeringSensor.GetType();
            // try SetSeekTarget(Vector3) or SetSeekTarget(object) or SetSeekTarget(GameObject)
            var m = type.GetMethod("SetSeekTarget", new Type[] { typeof(Vector3) });
            if (m != null)
            {
                m.Invoke(steeringSensor, new object[] { pos });
                return;
            }

            // fallback: method named "SetSeekTarget" with object param
            m = type.GetMethod("SetSeekTarget", new Type[] { typeof(object) });
            if (m != null)
            {
                m.Invoke(steeringSensor, new object[] { pos });
                return;
            }

            // try "SetSeekTarget" with Vector3 as boxed object
            m = type.GetMethod("SetSeekTarget", new Type[] { });
            if (m != null)
            {
                // no-arg version exists -> not useful
            }

            // try method "SetTarget" or "SetDestination"
            m = type.GetMethod("SetDestination", new Type[] { typeof(Vector3) });
            if (m != null)
            {
                m.Invoke(steeringSensor, new object[] { pos });
                return;
            }

            m = type.GetMethod("SetTarget", new Type[] { typeof(Vector3) });
            if (m != null)
            {
                m.Invoke(steeringSensor, new object[] { pos });
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[DroneBase] Steering SetSeek call failed: {e.Message}");
        }
    }

    protected void TryCallSteeringSetSeek(GameObject go)
    {
        try
        {
            var type = steeringSensor.GetType();
            var m = type.GetMethod("SetSeekTarget", new Type[] { typeof(GameObject) });
            if (m != null)
            {
                m.Invoke(steeringSensor, new object[] { go });
                return;
            }
            m = type.GetMethod("SetSeekTarget", new Type[] { typeof(object) });
            if (m != null)
            {
                m.Invoke(steeringSensor, new object[] { go });
                return;
            }
            m = type.GetMethod("SetDestination", new Type[] { typeof(GameObject) });
            if (m != null)
            {
                m.Invoke(steeringSensor, new object[] { go });
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[DroneBase] Steering SetSeek(GameObject) failed: {e.Message}");
        }
    }

    protected void TryCallSteeringClear()
    {
        try
        {
            var type = steeringSensor.GetType();
            var m = type.GetMethod("ClearSeekTarget", Type.EmptyTypes);
            if (m != null) { m.Invoke(steeringSensor, null); return; }

            m = type.GetMethod("ClearTarget", Type.EmptyTypes);
            if (m != null) { m.Invoke(steeringSensor, null); return; }

            m = type.GetMethod("SetSeekTarget", new Type[] { typeof(UnityEngine.Vector3) });
            // if only SetSeekTarget exists, maybe pass current position? ignore
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[DroneBase] Steering Clear call failed: {e.Message}");
        }
    }
}
