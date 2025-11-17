using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointPlacer : MonoBehaviour
{
    [Header("Container to move (SpawnPointDrone)")]
    [Tooltip("Объект, который содержит реальные spawn-points (тот, который нужно перемещать).")]
    public Transform spawnPointsContainer;

    [Header("Preferred: list of spawn anchors (in inspector)")]
    [Tooltip("Список точек SpawnPointDronePosition. Индексы соответствуют Location enum (0 = Camp, 1 = Factory, 2 = City и т.д.).")]
    public List<Transform> spawnPointPositions;

    [Header("Behavior")]
    [Tooltip("Если true — применяем и поворот anchor к контейнеру.")]
    public bool applyRotationFromAnchor = true;

    [Header("ScenarioManager and Player")]
    [SerializeField] ScenarioManager scenarioManager;
    [SerializeField] List<ObjectsInteresting> objectsInteresting;
    [SerializeField] List<Transform> playerLocationPosition;
    [SerializeField] Transform player;

    [Serializable]
    public struct LocationAnchor
    {
        public LocationSelectionService.Location location;
        public Transform anchor;
    }

    private void Awake()
    {
         ApplySelectedLocation();
    }

    public void ApplySelectedLocation()
    {
        if (spawnPointsContainer == null)
        {
            Debug.LogError("[SpawnPointPlacer] spawnPointsContainer not assigned.");
            return;
        }

        var chosen = LocationSelectionService.GetSelectedOrDefault();

        if (spawnPointPositions != null && spawnPointPositions.Count > 0)
        {
            int idx = (int)chosen;
            if (idx >= 0 && idx < spawnPointPositions.Count && spawnPointPositions[idx] != null)
            {
                scenarioManager.SetNewObjectsOfInterestInLocation(objectsInteresting[idx].GetObjectOfInterests());
                player.position = playerLocationPosition[idx].position;
                player.rotation = playerLocationPosition[idx].rotation;

                Transform anchor = spawnPointPositions[idx];
                MoveContainerTo(anchor);
                return;
            }
            else
            {
                Debug.LogWarning($"[SpawnPointPlacer] spawnPointPositions does not contain index {(int)chosen} ({chosen}). Falling back to anchors.");
            }
        }

        Debug.LogWarning($"[SpawnPointPlacer] No spawn anchor found for selected location {chosen} (list and anchors empty).");
    }

    private void MoveContainerTo(Transform anchor)
    {
        if (anchor == null) return;
        spawnPointsContainer.position = anchor.position;
        if (applyRotationFromAnchor) spawnPointsContainer.rotation = anchor.rotation;
    }

    // для тестирования в редакторе
#if UNITY_EDITOR
    [ContextMenu("Apply Selected Location (Editor)")]
    private void EditorApply() => ApplySelectedLocation();
#endif
}
