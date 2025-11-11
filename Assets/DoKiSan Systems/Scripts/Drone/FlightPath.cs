using System.Collections.Generic;
using UnityEngine;

public enum FlightPathType
{
    Recon,
    Attack_Drop,
    Attack_Kamikaze
}

[DisallowMultipleComponent]
public class FlightPath : MonoBehaviour
{
    [Header("Path")]
    [Tooltip("Список точек пути в порядке следования")]
    public List<Transform> waypoints = new List<Transform>();

    [Header("Meta")]
    public FlightPathType pathType = FlightPathType.Recon;
    [Tooltip("Можно задать альтернативный exit point (куда улетает дрон после завершения). Если null, spawner/ai вычисляет автоматически.")]
    public Transform exitPoint;

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count == 0) return;
        Gizmos.color = pathType == FlightPathType.Recon ? Color.cyan : Color.red;
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawSphere(waypoints[i].position, 0.25f);
            if (i + 1 < waypoints.Count && waypoints[i+1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i+1].position);
            }
        }
    }
}
