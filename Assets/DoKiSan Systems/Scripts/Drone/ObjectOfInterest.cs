using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ObjectOfInterest : MonoBehaviour
{
    [Header("Optional spawn anchor (informational)")]
    [Tooltip("Точка, на которой логически находится маршрут для этого объекта (не используется для сдвига путей).")]
    public Transform pathSpawnPoint;

    [Header("Exit points (куда дроны улетают после завершения)")]
    public List<Transform> exitPoints = new List<Transform>();

    [Header("Recon paths (separate lists for copter / wing)")]
    public List<FlightPath> reconPathsCopter = new List<FlightPath>();
    public List<FlightPath> reconPathsWing = new List<FlightPath>();

    [Header("Attack drop paths")]
    public List<FlightPath> attackDropPathsCopter = new List<FlightPath>();
    public List<FlightPath> attackDropPathsWing = new List<FlightPath>();

    [Header("Attack kamikaze paths")]
    public List<FlightPath> attackKamikazePathsCopter = new List<FlightPath>();
    public List<FlightPath> attackKamikazePathsWing = new List<FlightPath>();

    public Transform GetRandomExitPoint()
    {
        if (exitPoints == null || exitPoints.Count == 0) return null;
        return exitPoints[Random.Range(0, exitPoints.Count)];
    }

    public List<FlightPath> GetPathsFor(FlightPathType pathType, DroneTypeSelection droneType)
    {
        switch (pathType)
        {
            case FlightPathType.Recon:
                return droneType == DroneTypeSelection.Copter ? reconPathsCopter : reconPathsWing;
            case FlightPathType.Attack_Drop:
                return droneType == DroneTypeSelection.Copter ? attackDropPathsCopter : attackDropPathsWing;
            case FlightPathType.Attack_Kamikaze:
                return droneType == DroneTypeSelection.Copter ? attackKamikazePathsCopter : attackKamikazePathsWing;
            default:
                return null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        if (pathSpawnPoint != null) Gizmos.DrawSphere(pathSpawnPoint.position, 0.25f);
        Gizmos.color = Color.green;
        if (exitPoints != null)
        {
            foreach (var e in exitPoints) if (e != null) Gizmos.DrawCube(e.position, Vector3.one * 0.25f);
        }
    }
}

public enum DroneTypeSelection
{
    Copter,
    Wing
}
