using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlaceablePreview : MonoBehaviour
{
    private Renderer[] renderers = new Renderer[0];
    private Collider[] colliders = new Collider[0];
    private List<Material[]> originalMaterials = new List<Material[]>();

    private void Awake()
    {
        // безопасная инициализация
        renderers = GetComponentsInChildren<Renderer>(includeInactive: true) ?? new Renderer[0];
        colliders = GetComponentsInChildren<Collider>(includeInactive: true) ?? new Collider[0];

        originalMaterials.Clear();
        foreach (var r in renderers)
        {
            originalMaterials.Add(r != null ? r.sharedMaterials : new Material[0]);
        }
    }

    public void DisableColliders()
    {
        if (colliders == null || colliders.Length == 0) return;
        foreach (var c in colliders)
        {
            if (c == null) continue;
            c.enabled = false;
        }
    }

    public void EnableColliders()
    {
        if (colliders == null || colliders.Length == 0) return;
        foreach (var c in colliders)
        {
            if (c == null) continue;
            c.enabled = true;
        }
    }

    public void SetAllMaterials(Material mat)
    {
        if (mat == null || renderers == null) return;
        foreach (var r in renderers)
        {
            if (r == null) continue;
            int slots = Mathf.Max(1, r.sharedMaterials.Length);
            Material[] mats = new Material[slots];
            for (int i = 0; i < slots; i++) mats[i] = mat;
            r.sharedMaterials = mats;
        }
    }

    public void RestoreOriginalMaterials()
    {
        if (renderers == null || originalMaterials == null) return;
        for (int i = 0; i < renderers.Length && i < originalMaterials.Count; i++)
        {
            var r = renderers[i];
            if (r == null) continue;
            r.sharedMaterials = originalMaterials[i];
        }
    }
}
