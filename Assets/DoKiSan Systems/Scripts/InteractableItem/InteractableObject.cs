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

    // ---------------- Context actions ----------------
    [Serializable]
    public class ContextAction
    {
        public string label;
        public UnityEvent onSelected;
    }

    [Header("Context Menu (optional)")]
    [Tooltip("Если оставить пустым — по умолчанию будет одна кнопка 'Inspect' (или None).")]
    [SerializeField] private List<ContextAction> inspectorActions = new List<ContextAction>();

    /// <summary>
    /// Возвращает список действий, которые будет показывать контекстное меню для этого объекта.
    /// Можно переопределить в наследниках, чтобы динамически формировать набор.
    /// </summary>
    public virtual List<ContextAction> GetContextActions()
    {
        return new List<ContextAction>(inspectorActions);
    }

    /// <summary>
    /// Удобный метод для добавления действия программно.
    /// </summary>
    public void AddContextAction(string label, UnityAction callback)
    {
        var a = new ContextAction { label = label, onSelected = new UnityEvent() };
        a.onSelected.AddListener(callback);
        inspectorActions.Add(a);
    }

    public virtual string GetDisplayName()
    {
        return FullName;
    }
}