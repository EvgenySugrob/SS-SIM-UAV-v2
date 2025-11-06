using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class MenuManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private PlayerInput playerInput; 
    [SerializeField] private FirstPersonController fpsController; 

    [Header("Action maps")]
    [SerializeField] private string gameplayMapName = "Player";
    [SerializeField] private string uiMapName = "UI";

    [Header("UI focus (optional)")]
    [SerializeField] private GameObject defaultSelectedForMenu;

    private InputAction openMenuAction;
    private bool isOpen;

    private void Reset()
    {
        if (playerInput == null) playerInput = FindObjectOfType<PlayerInput>();
        if (fpsController == null && playerInput != null) fpsController = playerInput.GetComponent<FirstPersonController>();
    }

    private void Awake()
    {
        if (menuRoot == null) Debug.LogError($"[{nameof(MenuManager)}] menuRoot not assigned.", this);
        if (playerInput == null) Debug.LogWarning($"[{nameof(MenuManager)}] PlayerInput not assigned - trying FindObjectOfType.", this);
        if (playerInput == null) playerInput = FindObjectOfType<PlayerInput>();
        if (fpsController == null && playerInput != null) fpsController = playerInput.GetComponent<FirstPersonController>();
    }

    private void OnEnable()
    {
        if (playerInput == null || playerInput.actions == null)
        {
            Debug.LogError($"[{nameof(MenuManager)}] PlayerInput or actions asset missing.", this);
            return;
        }

        openMenuAction = playerInput.actions.FindAction("OpenMenu", throwIfNotFound: false);

        if (openMenuAction == null)
        {
            Debug.LogWarning($"[{nameof(MenuManager)}] OpenMenu action not found in the assigned actions asset.");
            return;
        }

        openMenuAction.performed += OnOpenMenuPerformed;
        openMenuAction.Enable();
    }

    private void OnDisable()
    {
        if (openMenuAction != null)
        {
            openMenuAction.performed -= OnOpenMenuPerformed;
            openMenuAction.Disable();
        }
    }

    private void OnOpenMenuPerformed(InputAction.CallbackContext ctx)
    {
        ToggleMenu();
    }

    public void ToggleMenu(bool? force = null)
    {
        bool target = force ?? !isOpen;
        if (target) OpenMenu();
        else CloseMenu();
    }

    public void OpenMenu()
    {
        if (isOpen) return;
        isOpen = true;

        if (menuRoot != null) menuRoot.SetActive(true);

        if (!string.IsNullOrEmpty(uiMapName) && playerInput != null && playerInput.actions != null)
        {
            var map = playerInput.actions.FindActionMap(uiMapName, throwIfNotFound: false);
            if (map != null)
            {
                playerInput.SwitchCurrentActionMap(uiMapName);
            }
        }

        if (openMenuAction != null && !openMenuAction.enabled) openMenuAction.Enable();

        if (fpsController != null) fpsController.SetInputEnabled(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (defaultSelectedForMenu != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(defaultSelectedForMenu);
        }
    }

    public void CloseMenu()
    {
        if (!isOpen) return;
        isOpen = false;

        if (menuRoot != null) menuRoot.SetActive(false);

        if (!string.IsNullOrEmpty(gameplayMapName) && playerInput != null && playerInput.actions != null)
        {
            var map = playerInput.actions.FindActionMap(gameplayMapName, throwIfNotFound: false);
            if (map != null)
            {
                playerInput.SwitchCurrentActionMap(gameplayMapName);
            }
        }

        if (fpsController != null) fpsController.SetInputEnabled(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);

        if (openMenuAction != null && !openMenuAction.enabled) openMenuAction.Enable();
    }

    public void OnCloseButtonPressed()
    {
        CloseMenu();
    }

    public bool IsOpen => isOpen;
}
