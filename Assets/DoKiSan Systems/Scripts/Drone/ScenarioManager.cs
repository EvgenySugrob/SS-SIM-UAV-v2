using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ScenarioManager : MonoBehaviour
{
    public enum BehaviorMode { Recon = 0, Attack = 1, Mixed = 2 }

    [Header("Prefabs")]
    public GameObject copterPrefab;    // prefab with DroneAI + SteeringSensor + Rigidbody
    public GameObject wingPrefab;

    [Header("Spawn points")]
    [Tooltip("Spawn points on location. 1 point -> 1 drone. Use up to 10.")]
    public Transform[] spawnPoints;

    [Header("Objects of interest in location")]
    [SerializeField] List<ObjectOfInterest> objectsOfInterest;

    [Header("Optional attack targets (explicit)")]
    public Transform[] attackTargets;

    [Header("Scene organization (optional)")]
    [Tooltip("Parent under which instantiated FlightPath instances will be placed.")]
    public Transform pathsParent;

    // runtime lists to clean up
    private List<GameObject> spawnedDrones = new List<GameObject>();
    private List<GameObject> spawnedPaths = new List<GameObject>();

    public void ApplySettings(int totalDrones, int copterCount, int wingCount, BehaviorMode behaviorMode)
    {
        ClearSpawned();

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[ScenarioManager] No spawn points assigned.");
            return;
        }
        if (objectsOfInterest == null || objectsOfInterest.Count == 0)
        {
            Debug.LogWarning("[ScenarioManager] No objectsOfInterest assigned.");
            return;
        }

        totalDrones = Mathf.Clamp(totalDrones, 0, spawnPoints.Length);
        copterCount = Mathf.Clamp(copterCount, 0, totalDrones);
        wingCount = Mathf.Clamp(wingCount, 0, totalDrones - copterCount);

        List<DroneTypeSelection> spawnTypes = BuildSpawnTypeList(totalDrones, copterCount, wingCount);

        // behavior alternation bookkeeping for Mixed:
        bool lastWasRecon = false;

        int used = 0;
        // spawnPoints are used sequentially (1 spawn point -> 1 drone)
        for (int i = 0; i < spawnPoints.Length && used < totalDrones; i++)
        {
            var sp = spawnPoints[i];
            if (sp == null) continue;

            DroneTypeSelection dt = spawnTypes[used];
            BehaviorMode assignedBehavior = ResolveBehaviorForIndex(behaviorMode, ref lastWasRecon);

            // choose random object of interest
            ObjectOfInterest oi = objectsOfInterest[Random.Range(0, objectsOfInterest.Count)];
            if (oi == null)
            {
                // do not consume a spawn slot if chosen oi is null — skip this spawn point
                continue;
            }

            // determine path type to request (for Attack we randomly take drop or kamikaze)
            FlightPathType reqPathType = FlightPathType.Recon;
            if (assignedBehavior == BehaviorMode.Recon) reqPathType = FlightPathType.Recon;
            else if (assignedBehavior == BehaviorMode.Attack)
            {
                // randomly choose attack variant
                reqPathType = Random.value < 0.5f ? FlightPathType.Attack_Drop : FlightPathType.Attack_Kamikaze;
            }

            List<FlightPath> candidatePaths = oi.GetPathsFor(reqPathType, dt);
            if (candidatePaths == null || candidatePaths.Count == 0)
            {
                Debug.LogWarning($"[ScenarioManager] No paths for object '{oi.name}', type {reqPathType}, drone {dt}. Skipping spawn at {sp.name}.");
                // consume spawn slot (we attempted spawn here)
                used++;
                continue;
            }

            // pick random path prefab (we will instantiate it into the scene)
            FlightPath chosenPrefab = candidatePaths[Random.Range(0, candidatePaths.Count)];
            if (chosenPrefab == null)
            {
                Debug.LogWarning("[ScenarioManager] Chosen path prefab invalid - skip.");
                used++;
                continue;
            }

            // compute spawn position for the path: use ObjectOfInterest.pathSpawnPoint if assigned,
            // otherwise fallback to prefab's own position.
            Vector3 pathPos = (oi.pathSpawnPoint != null) ? oi.pathSpawnPoint.position : chosenPrefab.transform.position;

            // IMPORTANT: use the SPAWN POINT'S ROTATION (sp.rotation) as you requested:
            // spawn path at object-of-interest position, but orient it to the drone spawn point rotation.
            Quaternion pathRot = sp.rotation;

            // Instantiate the path prefab at pathPos with rotation equal to the drone spawn point rotation
            GameObject pathInstGo = Instantiate(chosenPrefab.gameObject, pathPos, pathRot);

            // parent it if required
            if (pathsParent != null) pathInstGo.transform.SetParent(pathsParent, true);

            pathInstGo.name = chosenPrefab.gameObject.name + "_Inst";
            spawnedPaths.Add(pathInstGo);
            
            FlightPath pathInstance = pathInstGo.GetComponent<FlightPath>();
            if (pathInstance == null || pathInstance.waypoints == null || pathInstance.waypoints.Count == 0)
            {
                Debug.LogWarning($"[ScenarioManager] Instantiated path has no valid waypoints ({chosenPrefab.name}). Destroying path and skipping spawn.");
                spawnedPaths.Remove(pathInstGo);
                Destroy(pathInstGo);
                used++;
                continue;
            }

            GameObject prefab = dt == DroneTypeSelection.Copter ? copterPrefab : wingPrefab;
            if (prefab == null)
            {
                Debug.LogWarning("[ScenarioManager] Drone prefab not assigned for type " + dt);
                // cleanup path instance we created
                spawnedPaths.Remove(pathInstGo);
                Destroy(pathInstGo);
                used++;
                continue;
            }

            // spawn drone at spawn point
            GameObject inst = Instantiate(prefab, sp.position, sp.rotation);
            spawnedDrones.Add(inst);

            DroneAI ai = inst.GetComponent<DroneAI>();
            if (ai != null)
            {
                // If attack and explicit attackTargets provided, optionally set override target (but do NOT change FlightPath)
                Transform overrideAttackTarget = null;
                if (assignedBehavior == BehaviorMode.Attack && attackTargets != null && attackTargets.Length > 0)
                {
                    overrideAttackTarget = attackTargets[Random.Range(0, attackTargets.Length)];
                }

                // exit point from object of interest (optional)
                Transform exit = oi.GetRandomExitPoint();

                // assign instantiated path to drone
                ai.StartWithPath(pathInstance, assignedBehavior == BehaviorMode.Attack, overrideAttackTarget, exit != null ? exit.position : (Vector3?)null);

                Debug.Log($"[ScenarioManager] Spawned {inst.name} at {sp.name} with path {pathInstance.name} (type {pathInstance.pathType})");
            }

            used++;
        }

        Debug.Log($"[ScenarioManager] Spawned {spawnedDrones.Count}/{totalDrones} drones.");
    }

    private List<DroneTypeSelection> BuildSpawnTypeList(int total, int copterCount, int wingCount)
    {
        List<DroneTypeSelection> list = new List<DroneTypeSelection>(total);

        if (copterCount == 0 && wingCount > 0)
        {
            for (int i = 0; i < total; i++) list.Add(DroneTypeSelection.Wing);
            return list;
        }
        if (wingCount == 0 && copterCount > 0)
        {
            for (int i = 0; i < total; i++) list.Add(DroneTypeSelection.Copter);
            return list;
        }

        // both: build pool and shuffle
        List<DroneTypeSelection> pool = new List<DroneTypeSelection>();
        for (int i = 0; i < copterCount; i++) pool.Add(DroneTypeSelection.Copter);
        for (int i = 0; i < wingCount; i++) pool.Add(DroneTypeSelection.Wing);

        // shuffle pool
        for (int i = 0; i < pool.Count; i++)
        {
            int j = Random.Range(i, pool.Count);
            var tmp = pool[i];
            pool[i] = pool[j];
            pool[j] = tmp;
        }

        // ensure length == total
        while (pool.Count < total) pool.Add(DroneTypeSelection.Copter);
        for (int i = 0; i < total; i++) list.Add(pool[i]);

        return list;
    }

    private BehaviorMode ResolveBehaviorForIndex(BehaviorMode globalMode, ref bool lastWasRecon)
    {
        if (globalMode == BehaviorMode.Recon) return BehaviorMode.Recon;
        if (globalMode == BehaviorMode.Attack) return BehaviorMode.Attack;

        // Mixed: alternate Recon / Attack
        bool giveRecon = !lastWasRecon;
        lastWasRecon = giveRecon;
        return giveRecon ? BehaviorMode.Recon : BehaviorMode.Attack;
    }

    public void ClearSpawned()
    {
        // destroy drones
        foreach (var g in spawnedDrones) if (g != null) Destroy(g);
        spawnedDrones.Clear();

        // destroy instantiated path instances
        foreach (var p in spawnedPaths) if (p != null) Destroy(p);
        spawnedPaths.Clear();
    }

    public void SetNewObjectsOfInterestInLocation(List<ObjectOfInterest> newObjects)
    {
        if(objectsOfInterest.Count > 0)
        {
            objectsOfInterest.Clear();
            objectsOfInterest = newObjects;
        }
    }
}
