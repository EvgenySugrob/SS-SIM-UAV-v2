using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;            // assign (Player)
    [SerializeField] private Camera playerCamera;               // assign or leave null -> Camera.main
    [SerializeField] private float maxInteractDistance = 6f;
    [SerializeField] private LayerMask interactableLayerMask = ~0;

    private InputAction leftClickAction;

    private void Awake()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (playerInput == null) playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        if (playerInput == null || playerInput.actions == null) return;
        leftClickAction = playerInput.actions.FindAction("LeftClick", throwIfNotFound: false);
        if (leftClickAction != null) leftClickAction.performed += OnLeftClick;
    }

    private void OnDisable()
    {
        if (leftClickAction != null) leftClickAction.performed -= OnLeftClick;
    }

    private void OnLeftClick(InputAction.CallbackContext ctx)
    {
        // 1) Если меню открыто — игнорируем (меню контролирует ввод)
        if (ContextMenuManager.Instance != null && ContextMenuManager.Instance.IsOpen) return;

        // 2) Если указатель над UI — игнорируем (чтобы клики по UI не пробивались в мир)
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        // 3) Если сейчас режим установки — игнорируем (PlacementManager сам обрабатывает LMB/RMB)
        if (PlacementManager.Instance != null && PlacementManager.Instance.IsPlacing) return;

        if (playerCamera == null) playerCamera = Camera.main;
        if (playerCamera == null) return;

        // Raycast from camera center (FPS-style)
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxInteractDistance, interactableLayerMask, QueryTriggerInteraction.Ignore))
        {
            var io = hit.collider.GetComponentInParent<InteractableObject>();
            if (io != null)
            {
                // show context menu for this object
                ContextMenuManager.Instance?.Show(io);
            }
        }
    }
}
