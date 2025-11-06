using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class MouseCursorHandler : MonoBehaviour
{
    [Header("Cursor Settings")]
    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private Vector2 cursorHotspot = Vector2.zero;

    [Header("Interaction Settings")]
    [SerializeField] private LayerMask interactableLayerMask = -1;
    [SerializeField] private float interactionRange = 100f;

    [Header("Events")]
    public UnityEvent<GameObject> OnObjectClicked;
    public UnityEvent<Vector3> OnGroundClicked;
    public UnityEvent OnCursorLocked;
    public UnityEvent OnCursorUnlocked;

    [Header("InstrumetntInteract")]
    [SerializeField] bool isInstrumentInHand = false;
    [SerializeField] bool isToolInteract = false;
    [SerializeField] GameObject currentInstrument;


    private InputAction leftClickAction;
    private InputAction rightClickAction;

    private CameraController cameraController;
    private Camera cam;

    private Vector2 lastCursorPosition;
    private bool isCursorLocked = false;
    private IInteractable currentInteractable;

    public bool IsHandSlotBusy
    {
        get => isInstrumentInHand;
        set => isInstrumentInHand = value;
    }

    public bool IsToolInteract
    {
        get => isToolInteract;
        set => isToolInteract = value;
    }

    private void Awake()
    {
        cameraController = FindObjectOfType<CameraController>();
        cam = Camera.main;

        if (cam == null)
            cam = FindObjectOfType<Camera>();
    }

    private void OnEnable()
    {
        var inputActions = GetComponent<PlayerInput>().actions;

        leftClickAction = inputActions["LeftClick"];
        rightClickAction = inputActions["RightClick"];

        leftClickAction.performed += OnLeftClick;
        rightClickAction.started += OnRightClickStarted;
        rightClickAction.canceled += OnRightClickCanceled;

        leftClickAction.Enable();
        rightClickAction.Enable();
    }

    private void OnDisable()
    {
        if (leftClickAction != null)
            leftClickAction.performed -= OnLeftClick;

        if (rightClickAction != null)
        {
            rightClickAction.started -= OnRightClickStarted;
            rightClickAction.canceled -= OnRightClickCanceled;
        }
    }

    private void Start()
    {
        SetCursorVisible(true);
    }

    private void OnLeftClick(InputAction.CallbackContext context)
    {
        if (cameraController != null && cameraController.IsRotating)
            return;

        if (!EventSystem.current.IsPointerOverGameObject())
        {
            HandleLeftClick();
        }
    }

    private void OnRightClickStarted(InputAction.CallbackContext context)
    {
        LockCursor();
    }

    private void OnRightClickCanceled(InputAction.CallbackContext context)
    {
        UnlockCursor();
    }

    private void HandleLeftClick()
    {
        Vector2 screenPosition = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactableLayerMask))
        {
            GameObject hitObject = hit.collider.gameObject;

            IInteractable interactable = hitObject.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.OnInteract();
            }

            OnObjectClicked?.Invoke(hitObject);
        }
        else
        {
            Vector3 worldPosition = GetWorldPositionFromScreen(screenPosition);
            OnGroundClicked?.Invoke(worldPosition);
        }
    }

    private void LockCursor()
    {
        if (isCursorLocked) return;

        lastCursorPosition = Mouse.current.position.ReadValue();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isCursorLocked = true;
        OnCursorLocked?.Invoke();
    }

    private void UnlockCursor()
    {
        if (!isCursorLocked) return;

        Cursor.lockState = CursorLockMode.None;

        Mouse.current.WarpCursorPosition(lastCursorPosition);

        SetCursorVisible(true);

        isCursorLocked = false;
        OnCursorUnlocked?.Invoke();
    }

    private void SetCursorVisible(bool visible)
    {
        if (visible)
        {
            Cursor.visible = true;

            // ”станавливаем кастомный курсор, если он задан
            if (cursorTexture != null)
            {
                Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);
            }
            else
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
        }
        else
        {
            Cursor.visible = false;
        }
    }

    private Vector3 GetWorldPositionFromScreen(Vector2 screenPosition)
    {
        Ray ray = cam.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.point;
        }

        float distance = -ray.origin.y / ray.direction.y;
        return ray.origin + ray.direction * distance;
    }

    public void ForceLockCursor()
    {
        LockCursor();
    }

    public void ForceUnlockCursor()
    {
        UnlockCursor();
    }

    public bool IsCursorLocked => isCursorLocked;

    public GameObject GetObjectUnderCursor()
    {
        Vector2 screenPosition = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactableLayerMask))
        {
            return hit.collider.gameObject;
        }

        return null;
    }

    public Vector3 GetWorldPositionUnderCursor()
    {
        Vector2 screenPosition = Mouse.current.position.ReadValue();
        return GetWorldPositionFromScreen(screenPosition);
    }

    private void Update()
    {
        if (!isCursorLocked)
        {
            HandleCursorHover();
        }
    }

    private void HandleCursorHover()
    {
        GameObject hoveredObject = GetObjectUnderCursor();
        IInteractable newInteractable = hoveredObject != null ? hoveredObject.GetComponent<IInteractable>() : null;

        if (currentInteractable != null && (newInteractable == null || newInteractable != currentInteractable))
        {
            currentInteractable.OnHoverExit();
            currentInteractable = null;
        }

        if (newInteractable != null && newInteractable != currentInteractable)
        {
            newInteractable.OnHoverEnter();
            currentInteractable = newInteractable;
        }
    }
    public void ClearHand()
    {
        currentInstrument = null;
    }
    public void SetCurrentInstrument(GameObject instrument)
    {
        currentInstrument = instrument;
    }
    public GameObject GetCurrentInstrument()
    {
        return currentInstrument;
    }

    public interface IInteractable
    {
        void OnInteract();
        void OnHoverEnter();
        void OnHoverExit();

        string GetObjectID();

        void SetHighlight(bool state);
    }
}
