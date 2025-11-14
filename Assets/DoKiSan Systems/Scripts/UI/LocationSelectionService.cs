using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LocationSelectionService
{
    public enum Location
    {
        Camp = 0,
        Factory = 1,
        City = 2
    }

    // default
    private static Location? selected = null;

    public static void SetSelected(Location loc)
    {
        selected = loc;
    }

    public static Location GetSelectedOrDefault(Location def = Location.Camp)
    {
        return selected ?? def;
    }

    public static bool HasSelection => selected.HasValue;

    public static void ClearSelection() => selected = null;
}
