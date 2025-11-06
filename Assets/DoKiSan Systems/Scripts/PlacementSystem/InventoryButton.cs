using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private MenuManager menuManager; // опционально, чтобы закрыть меню

    public void OnPointerDown(PointerEventData eventData)
    {
        if (prefab == null) return;

        if (menuManager != null) menuManager.CloseMenu();
        if (PlacementManager.Instance != null) PlacementManager.Instance.StartPlacementFromUI(prefab);
    }
}