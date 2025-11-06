using System.Collections.Generic;
using UnityEngine;

public class DroneSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject copterPrefab; // prefab must have DroneCopter component + SteeringSensor child
    public GameObject wingPrefab;   // prefab must have DroneWing component + SteeringSensor child

    [Header("Spawn")]
    public int totalDrones = 10;
    public int numCopters = 5;
    public int numWings = 5;

    [Header("Behavior settings")]
    public float reconRatio = 0.5f; // portion of drones assigned recon (rest attack)
    public Transform[] reconTargets;
    public Transform[] attackTargets;

    [Header("Spawn area")]
    public Transform spawnCenter;
    public float spawnRadius = 20f;
    public float spawnAltitude = 10f;

    private List<DroneBase> spawned = new List<DroneBase>();

    public void SpawnScenario()
    {
        // clamp counts
        numCopters = Mathf.Clamp(numCopters, 0, totalDrones);
        numWings = Mathf.Clamp(numWings, 0, totalDrones - numCopters);

        int spawnedCount = 0;

        // spawn copters
        for (int i = 0; i < numCopters; i++)
        {
            if (spawnedCount >= totalDrones) break;
            var go = Instantiate(copterPrefab, RandomSpawnPos(), Quaternion.identity);
            var drone = go.GetComponent<DroneBase>();
            if (drone == null) drone = go.AddComponent<DroneCopter>();
            SetupAndStart(drone);
            spawned.Add(drone);
            spawnedCount++;
        }

        // spawn wings
        for (int i = 0; i < numWings; i++)
        {
            if (spawnedCount >= totalDrones) break;
            var go = Instantiate(wingPrefab, RandomSpawnPos(), Quaternion.identity);
            var drone = go.GetComponent<DroneBase>();
            if (drone == null) drone = go.AddComponent<DroneWing>();
            SetupAndStart(drone);
            spawned.Add(drone);
            spawnedCount++;
        }

        // if still need spawn (totalDrones > sum), spawn default copters
        while (spawnedCount < totalDrones)
        {
            var go = Instantiate(copterPrefab, RandomSpawnPos(), Quaternion.identity);
            var drone = go.GetComponent<DroneBase>();
            if (drone == null) drone = go.AddComponent<DroneCopter>();
            SetupAndStart(drone);
            spawned.Add(drone);
            spawnedCount++;
        }
    }

    private Vector3 RandomSpawnPos()
    {
        Vector2 r = Random.insideUnitCircle * spawnRadius;
        Vector3 pos = spawnCenter != null ? spawnCenter.position : transform.position;
        pos += new Vector3(r.x, spawnAltitude, r.y);
        return pos;
    }

    private void SetupAndStart(DroneBase drone)
    {
        // try to set steeringSensor component automatically (child named "SteeringSensor")
        var ss = ComponentFinder.FindInChildren(drone.transform, "SteeringSensor");
        if (ss != null) drone.steeringSensorComponent = ss;

        // pick behavior randomly by reconRatio
        bool isRecon = Random.value < reconRatio;
        IDroneBehavior behavior = null;
        if (isRecon && reconTargets != null && reconTargets.Length > 0)
        {
            Transform t = reconTargets[Random.Range(0, reconTargets.Length)];
            behavior = new ReconBehavior(t, radius: Random.Range(6f, 20f), altitude: Random.Range(8f, 18f), angularSpeedDegPerSec: Random.Range(20f, 45f));
        }
        else
        {
            if (attackTargets != null && attackTargets.Length > 0)
           {
                Transform t = attackTargets[Random.Range(0, attackTargets.Length)];
                bool kamikaze = Random.value < 0.6f;
                Vector3 exit = (transform.position + (transform.forward * 50f)); // simple exit
                behavior = new AttackBehavior(t, kamikaze, exit);
            }
            else
            {
                // fallback to recon on no attack targets
                Transform t = reconTargets != null && reconTargets.Length > 0 ? reconTargets[0] : null;
                behavior = new ReconBehavior(t, radius: Random.Range(6f, 20f), altitude: Random.Range(8f, 18f), angularSpeedDegPerSec: Random.Range(20f, 45f));
            }
        }

       drone.StartMission(behavior);
    }
}

/// <summary>
/// Small helper to find Component by type name (reflection). Useful when SensorToolkit types are in different namespace.
/// </summary>
public static class ComponentFinder
{
    /// <summary>
    /// Находит первый компонент в иерархии потомков (включая сам root), у которого GetType().Name == typeName.
    /// </summary>
    public static Component FindInChildren(Transform root, string typeName)
    {
        if (root == null || string.IsNullOrEmpty(typeName)) return null;

        // сначала проверим сам root
        var compsRoot = root.GetComponents<Component>();
        foreach (var c in compsRoot)
        {
            if (c == null) continue;
            if (c.GetType().Name == typeName) return c;
        }

        // затем рекурсивно по всем детям (GetComponentsInChildren включает root, но мы уже проверили его — однако это проще)
        var comps = root.GetComponentsInChildren<Component>(true);
        foreach (var c in comps)
        {
            if (c == null) continue;
            if (c.GetType().Name == typeName) return c;
        }

        return null;
    }

    /// <summary>
    /// Поиск везде по сцене (весь проект) по имени типа.
    /// </summary>
    public static Component ByName(string typeName)
    {
        if (string.IsNullOrEmpty(typeName)) return null;
        var all = GameObject.FindObjectsOfType<Component>();
        foreach (var c in all)
        {
            if (c == null) continue;
            if (c.GetType().Name == typeName) return c;
        }
        return null;
    }
}

