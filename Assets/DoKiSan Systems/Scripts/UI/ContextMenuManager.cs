using UnityEngine;
using UnityEngine.InputSystem;

public class ContextMenuManager : MonoBehaviour
{
    public static ContextMenuManager Instance;

    [Header("Prefab & Canvas")]
    [SerializeField] private GameObject menuPrefab; // PrefabContextMenu (root должен иметь ContextMenuInstance)
    [SerializeField] private Canvas parentCanvas;   // если не задан — будет найден автоматически

    [Header("Player")]
    [SerializeField] private FirstPersonController fpsController; // для блокировки/разблокировки ввода

    private GameObject currentMenuInstance;
    private ContextMenuInstance currentInstance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (parentCanvas == null)
            parentCanvas = GetComponentInParent<Canvas>() ?? FindObjectOfType<Canvas>();
    }

    /// <summary>
    /// Показать меню для target. Меню будет центрировано на экране (если нужно - можно вызвать Show(target, screenPos)).
    /// </summary>
    public void Show(InteractableObject target)
    {
        // центр экрана (в пикселях)
        Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Show(target, center);
    }

    /// <summary>
    /// Показать меню для target в экранной позиции screenPos (пиксели). Если wantCenter == true, будет центрировано.
    /// </summary>
    public void Show(InteractableObject target, Vector2 screenPos)
    {
        Hide(); // закрыть текущее, если есть

        if (menuPrefab == null || parentCanvas == null)
        {
            Debug.LogWarning("[ContextMenuManager] menuPrefab или parentCanvas не назначены.");
            return;
        }

        currentMenuInstance = Instantiate(menuPrefab, parentCanvas.transform);
        currentInstance = currentMenuInstance.GetComponent<ContextMenuInstance>();
        if (currentInstance == null)
        {
            Debug.LogError("[ContextMenuManager] Префаб меню должен содержать компонент ContextMenuInstance на корне.");
            Destroy(currentMenuInstance);
            currentMenuInstance = null;
            currentInstance = null;
            return;
        }

        // инициализируем, менеджер передаёт ссылку на себя и fpsController
        currentInstance.Initialize(target, screenPos, parentCanvas, this, fpsController);
    }

    /// <summary>Закрыть текущее меню (вызывается из ContextMenuInstance например по Close button).</summary>
    public void Hide()
    {
        if (currentInstance != null)
        {
            currentInstance.OnForcedClose();
        }

        if (currentMenuInstance != null)
           Destroy(currentMenuInstance);

        currentMenuInstance = null;
        currentInstance = null;
    }

    public bool IsOpen => currentInstance != null;
}
