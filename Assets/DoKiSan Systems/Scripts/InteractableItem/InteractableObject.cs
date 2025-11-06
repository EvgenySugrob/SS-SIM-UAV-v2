using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public abstract class InteractableObject : MonoBehaviour
{
    [SerializeField] private string baseName = "Device";
    [SerializeField] private int id = -1;

    public string BaseName => baseName;
    public int ID => id;
    public string FullName => $"{baseName}-{id}";

    public void InitializeAsPlaced(string baseName, int assignedId)
    {
        this.baseName = baseName;
        this.id = assignedId;
        gameObject.name = FullName;
    }

    public virtual void Remove()
    {
        Destroy(gameObject);
    }

    public abstract List<ContextMenuItem> GetContextMenuItems();

    public virtual string GetDisplayName()
    {
        return FullName;
    }
}