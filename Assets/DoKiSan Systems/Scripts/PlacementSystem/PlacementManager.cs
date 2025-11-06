using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private FirstPersonController fpsController;
    [SerializeField] private Camera playerCamera;

    [Header("Preview visuals")]
    [SerializeField] private Material previewValidMaterial;
    [SerializeField] private Material previewInvalidMaterial;

    [Header("Placement rules")]
    [SerializeField] private LayerMask placementSurfaceMask = ~0;
    [SerializeField] private LayerMask placementBlockingMask = ~0;
    [SerializeField] private float maxPlacementDistance = 6f;
    [SerializeField, Range(0f, 90f)] private float maxSurfaceAngle = 75f; // допустимый угол поверхности
    [SerializeField] private float placementHeightOffset = 0f;         // смещение от поверхности вверх

    [Header("Rotation")]
    [SerializeField] private float rotateSpeedDegreesPerSecond = 120f;

    [Header("Behavior")]
    [SerializeField] private bool allowPlayerMovementDuringPlacement = true; // true — игрок может ходить во время установки

    // runtime state
    private bool isPlacing = false;
    private GameObject prefabToPlace;
    private GameObject previewInstance;
    private InteractableObject movingTarget;
    private Vector3 movingTargetOriginalPos;
    private Quaternion movingTargetOriginalRot;
    private float currentYaw = 0f;

    private bool previewIsExisting = false; // true — previewInstance == existing placed object

    private InputAction leftClickAction;
    private InputAction rightClickAction;
    private InputAction rotateLeftAction;
    private InputAction rotateRightAction;
    private InputAction mouseDeltaAction;

    private PlaceablePreview previewComp;
    private Quaternion prefabInitialRotation = Quaternion.identity;
    private Quaternion prefabInitialRotationInverse = Quaternion.identity;

    private Dictionary<string,int> counters = new Dictionary<string,int>();

    private List<InteractableObject> placedObjects = new List<InteractableObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else DestroyImmediate(gameObject);

        if (playerCamera == null) playerCamera = Camera.main;
        if (playerInput == null) playerInput = FindObjectOfType<PlayerInput>();
        if (fpsController == null && playerInput != null) fpsController = playerInput.GetComponent<FirstPersonController>();
    }

    private void OnEnable()
    {
        if (playerInput == null || playerInput.actions == null) return;

        leftClickAction = playerInput.actions.FindAction("LeftClick", false);
        rightClickAction = playerInput.actions.FindAction("RightClick", false);
        rotateLeftAction = playerInput.actions.FindAction("RotateLeft", false);
        rotateRightAction = playerInput.actions.FindAction("RotateRight", false);
        mouseDeltaAction = playerInput.actions.FindAction("MousePosition", false);

        if (leftClickAction != null) leftClickAction.performed += OnLeftClickPerformed;
        if (rightClickAction != null) rightClickAction.performed += OnRightClickPerformed;
    }

    private void OnDisable()
    {
        if (leftClickAction != null) leftClickAction.performed -= OnLeftClickPerformed;
        if (rightClickAction != null) rightClickAction.performed -= OnRightClickPerformed;
    }

    private void Update()
    {
        if (!isPlacing) return;

        // rotation via RotateLeft / RotateRight (Q/E)
        float rotInput = 0f;
        if (rotateLeftAction != null && rotateLeftAction.IsPressed()) rotInput -= 1f;
        if (rotateRightAction != null && rotateRightAction.IsPressed()) rotInput += 1f;
        if (Mathf.Abs(rotInput) > 0.001f)
        {
            currentYaw += rotInput * rotateSpeedDegreesPerSecond * Time.deltaTime;
            UpdatePreviewRotation();
        }

        UpdatePreviewPose();
    }

    // ----------------- Public API -----------------

    // Запуск режима установки (вызвать из UI-кнопки). Закрытие UI — ответственность вызова.
    public void StartPlacementFromUI(GameObject prefab)
    {
        StartPlacement(prefab, null);
    }

    // Запуск режима перемещения существующего поставленного объекта
    public void StartReposition(InteractableObject target)
    {
        if (target == null) return;
        StartPlacement(target.gameObject, target);
    }

    // Отмена режима (RMB или вызов из UI)
    public void CancelPlacement()
    {
        if (!isPlacing) return;

        if (previewIsExisting && movingTarget != null)
        {
            // вернуть объект в оригинальную позицию и состояние
            movingTarget.transform.position = movingTargetOriginalPos;
            movingTarget.transform.rotation = movingTargetOriginalRot;

            // восстановить коллайдеры/материалы через PlaceablePreview
            var comp = movingTarget.GetComponent<PlaceablePreview>();
            if (comp != null)
            {
                comp.RestoreOriginalMaterials();
                comp.EnableColliders();
            }
            movingTarget = null;
        }
        else
        {
            // если мы создавали отдельный previewInstance (новый объект), просто уничтожаем превью
            DestroyPreview();
        }

        isPlacing = false;
        previewIsExisting = false;

        if (fpsController != null) fpsController.SetInputEnabled(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public bool IsPlacing => isPlacing;

    // Удалить поставленный объект (вызов из UI контекста)
    public void DeletePlaced(InteractableObject obj)
    {
        if (obj == null) return;
        placedObjects.Remove(obj);
        Destroy(obj.gameObject);
    }

    // ----------------- Internal flow -----------------

    private void StartPlacement(GameObject prefabOrObject, InteractableObject moveTarget)
    {
        if (isPlacing) CancelPlacement();

        prefabToPlace = prefabOrObject;
        movingTarget = moveTarget;
        isPlacing = true;
        previewIsExisting = movingTarget != null;

        if (playerCamera == null) playerCamera = Camera.main;

        if (previewIsExisting)
        {
            // --- REPOSITION MODE: use the existing object as preview (no deactivation, no new instance) ---
            movingTargetOriginalPos = movingTarget.transform.position;
            movingTargetOriginalRot = movingTarget.transform.rotation;

            previewInstance = movingTarget.gameObject;
            previewComp = previewInstance.GetComponent<PlaceablePreview>();
            if (previewComp == null) previewComp = previewInstance.AddComponent<PlaceablePreview>();

            // отключаем коллайдеры и сохраняем оригинальные материалы (PlaceablePreview делает это в Awake)
            previewComp.DisableColliders();

            // initial yaw based on existing object or camera
            currentYaw = previewInstance.transform.eulerAngles.y;

            // store prefabInitialRotation so rotation math stays consistent
            prefabInitialRotation = previewInstance.transform.rotation;
            prefabInitialRotationInverse = Quaternion.Inverse(prefabInitialRotation);

            // apply preview visuals (invalid by default until placed on valid surface)
            ApplyPreviewMaterial(false);
        }
        else
        {
            // --- NEW PLACE MODE: create a preview instance from prefab ---
            previewInstance = Instantiate(prefabToPlace);
            previewComp = previewInstance.GetComponent<PlaceablePreview>();
            if (previewComp == null) previewComp = previewInstance.AddComponent<PlaceablePreview>();
            previewComp.DisableColliders();

            prefabInitialRotation = previewInstance.transform.rotation;
            prefabInitialRotationInverse = Quaternion.Inverse(prefabInitialRotation);

            currentYaw = playerCamera.transform.eulerAngles.y;

            ApplyPreviewMaterial(false);
        }

        if (!allowPlayerMovementDuringPlacement && fpsController != null) fpsController.SetInputEnabled(false);

        // lock cursor as before (you can change behaviour if you want cursor visible during placement)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void DestroyPreview()
    {
        if (previewIsExisting)
        {
            // nothing to destroy — we used the existing object; preview was restored/cancelled elsewhere
        }
        else
        {
            if (previewInstance != null) Destroy(previewInstance);
            previewInstance = null;
            previewComp = null;
        }
    }

    private void ConfirmPlacement()
    {
        if (!isPlacing || previewInstance == null) return;
        if (!IsPreviewValid()) return;

        if (!previewIsExisting)
        {
            // New placement -> instantiate real object at preview pose
            GameObject real = Instantiate(prefabToPlace, previewInstance.transform.position, previewInstance.transform.rotation);
            EnableCollidersRecursive(real, true);

            InteractableObject io = real.GetComponent<InteractableObject>();
            if (io == null)
                io = real.AddComponent<InteractableObject>();

            string baseName = !string.IsNullOrEmpty(io.BaseName) ? io.BaseName : (prefabToPlace != null ? prefabToPlace.name : "Device");
            int idx = GetNextIndexFor(baseName);
            io.InitializeAsPlaced(baseName, idx);
            placedObjects.Add(io);

            DestroyPreview();
        }
        else
        {
            // Reposition existing -> commit previewInstance (which is movingTarget) in place
            movingTarget.transform.position = previewInstance.transform.position;
            movingTarget.transform.rotation = previewInstance.transform.rotation;

            // restore colliders/materials
            if (previewComp != null)
            {
                previewComp.RestoreOriginalMaterials();
                previewComp.EnableColliders();
            }

            movingTarget = null;
            // Do NOT destroy previewInstance because it's the real object
            previewInstance = null;
            previewComp = null;
        }

        isPlacing = false;
        previewIsExisting = false;

        if (fpsController != null) fpsController.SetInputEnabled(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ----------------- Preview placement logic -----------------

    private void UpdatePreviewPose()
    {
        if (previewInstance == null || playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxPlacementDistance, placementSurfaceMask, QueryTriggerInteraction.Ignore))
        {
            Vector3 pos = hit.point + hit.normal * placementHeightOffset;
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, hit.normal) * Quaternion.Euler(0f, currentYaw, 0f) * prefabInitialRotationInverse;
            previewInstance.transform.SetPositionAndRotation(pos, rot);

            float angle = Vector3.Angle(hit.normal, Vector3.up);
            bool surfaceOk = angle <= maxSurfaceAngle;
            bool overlap = CheckPreviewOverlap();
            bool valid = surfaceOk && !overlap;

            ApplyPreviewMaterial(valid);
            return;
        }

        Vector3 fallback = playerCamera.transform.position + playerCamera.transform.forward * (maxPlacementDistance * 0.7f);
        previewInstance.transform.position = fallback;
        previewInstance.transform.rotation = Quaternion.Euler(0f, currentYaw, 0f) * prefabInitialRotation;
        ApplyPreviewMaterial(false);
    }

    private void UpdatePreviewRotation()
    {
        if (previewInstance == null) return;
        previewInstance.transform.rotation = Quaternion.Euler(0f, currentYaw, 0f) * prefabInitialRotation;
    }

    private bool CheckPreviewOverlap()
    {
        if (previewInstance == null) return true;

        var renders = previewInstance.GetComponentsInChildren<Renderer>();
        if (renders.Length == 0) return false;

        Bounds b = renders[0].bounds;
        for (int i = 1; i < renders.Length; i++) b.Encapsulate(renders[i].bounds);

        Collider[] hits = Physics.OverlapBox(b.center, b.extents, previewInstance.transform.rotation, placementBlockingMask, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0) return false;

        foreach (var h in hits)
        {
            if (previewInstance != null && h.transform.IsChildOf(previewInstance.transform)) continue;
            if (movingTarget != null && h.transform.IsChildOf(movingTarget.transform)) continue;
            return true;
        }
        return false;
    }

    private bool IsPreviewValid()
    {
        if (previewInstance == null) return false;
        if (Vector3.Distance(playerCamera.transform.position, previewInstance.transform.position) > maxPlacementDistance + 0.1f) return false;
        if (CheckPreviewOverlap()) return false;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxPlacementDistance, placementSurfaceMask, QueryTriggerInteraction.Ignore))
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            if (angle > maxSurfaceAngle) return false;
            return true;
        }
        return false;
    }

    private void ApplyPreviewMaterial(bool valid)
    {
        if (previewComp == null && previewInstance != null) previewComp = previewInstance.GetComponent<PlaceablePreview>();
        if (previewComp == null) return;
        previewComp.SetAllMaterials(valid ? previewValidMaterial : previewInvalidMaterial);
    }

    // ----------------- Input callbacks -----------------

    private void OnLeftClickPerformed(InputAction.CallbackContext ctx)
    {
        if (!isPlacing) return;
        if (IsPreviewValid()) ConfirmPlacement();
    }

    private void OnRightClickPerformed(InputAction.CallbackContext ctx)
    {
        if (!isPlacing) return;
        CancelPlacement();
    }

    // ----------------- Utilities -----------------

    private void EnableCollidersRecursive(GameObject go, bool enable)
    {
        var cols = go.GetComponentsInChildren<Collider>(includeInactive: true);
        foreach (var c in cols) if (c != null) c.enabled = enable;
    }

    private int GetNextIndexFor(string baseName)
    {
        if (string.IsNullOrEmpty(baseName)) baseName = "Device";
        if (!counters.TryGetValue(baseName, out int v)) v = 0;
        v++;
        counters[baseName] = v;
        return v;
    }
}
