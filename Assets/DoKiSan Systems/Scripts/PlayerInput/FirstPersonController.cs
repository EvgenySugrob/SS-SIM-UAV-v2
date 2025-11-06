using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class FirstPersonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private CharacterController characterController;

    [Header("Movement")]
    [SerializeField, Min(0f)] private float walkSpeed = 4f;
    [SerializeField, Min(0f)] private float sprintMultiplier = 1.9f;
    [SerializeField, Min(0f)] private float acceleration = 20f;
    [SerializeField, Min(0f)] private float deceleration = 16f;
    [SerializeField, Min(0f)] private float gravity = 9.81f;
    [SerializeField, Min(0f)] private float terminalVelocity = 53f;
    [SerializeField, Min(0f)] private float jumpHeight = 1.6f;
    [SerializeField] private bool allowJump = true;

    [Header("Look")]
    [SerializeField, Min(0.1f)] private float mouseSensitivity = 120f;
    [SerializeField, Range(0f, 90f)] private float maxPitch = 80f;
    [SerializeField, Range(-90f, 0f)] private float minPitch = -80f;
    [SerializeField, Range(0f, 1f)] private float lookSmoothing = 0.06f;

    [Header("Ground / Step")]
    [SerializeField, Min(0f)] private float skinWidth = 0.08f;
    [SerializeField, Min(0f)] private float stepOffset = 0.3f;
    [SerializeField, Min(0f)] private LayerMask groundLayers = ~0;

    [Header("Cursor & Input")]
    [SerializeField] private bool lockCursorOnStart = true;
    [SerializeField] private bool rotateOnlyOnRightClick = false; // если true — вращение по мыши работает только при нажатой правой кнопке

    // runtime
    private Vector2 moveInput = Vector2.zero;
    private bool sprinting = false;
    private Vector2 rawMouseDelta = Vector2.zero;
    private bool rightClickPressed = false;
    private bool leftClickPressed = false;
    private bool scrollPressed = false;
    private bool inputEnabled = true;

    private float yaw; // rotation around Y
    private float pitch; // rotation around X
    private Vector2 currentLookVelocity;
    private Vector3 currentVelocity; // horizontal movement velocity
    private float verticalVelocity = 0f; // Y axis velocity

    private PlayerInput playerInput;
    private InputAction mousePositionAction;
    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction rightClickAction;
    private InputAction leftClickAction;
    private InputAction jumpAction;

    private void Reset()
    {
        characterController = GetComponent<CharacterController>();
        if (cameraTransform == null && transform.childCount > 0)
            cameraTransform = transform.GetChild(0);
    }

    private void Awake()
    {
        characterController = characterController ? characterController : GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        if (cameraTransform == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) cameraTransform = cam.transform;
        }

        Vector3 euler = transform.eulerAngles;
        yaw = euler.y;
        pitch = cameraTransform ? cameraTransform.localEulerAngles.x : 0f;
        if (pitch > 180f) pitch -= 360f;

        if (characterController)
        {
            characterController.skinWidth = skinWidth;
            characterController.stepOffset = stepOffset;
        }
    }

    private void OnEnable()
    {
        if (playerInput == null) playerInput = GetComponent<PlayerInput>();
        var actions = playerInput ? playerInput.actions : null;
        if (actions == null)
        {
            Debug.LogError("[FirstPersonController] PlayerInput.actions is null. Attach PlayerInput and assign action asset.");
            return;
        }

        mousePositionAction = actions["MousePosition"];
        moveAction = actions["Move"];
        sprintAction = actions["Sprint"];
        rightClickAction = actions["RightClick"];
        leftClickAction = actions["LeftClick"];
        jumpAction = actions["Jump"];

        // subscribe
        if (mousePositionAction != null) mousePositionAction.Enable();
        if (moveAction != null)
        {
            moveAction.performed += OnMovePerformed;
            moveAction.canceled += OnMoveCanceled;
            moveAction.Enable();
        }
        if (sprintAction != null)
        {
            sprintAction.performed += ctx => sprinting = true;
            sprintAction.canceled += ctx => sprinting = false;
            sprintAction.Enable();
        }
        if (rightClickAction != null)
        {
            rightClickAction.performed += ctx => rightClickPressed = true;
            rightClickAction.canceled += ctx => rightClickPressed = false;
            rightClickAction.Enable();
        }
        if (leftClickAction != null)
        {
            leftClickAction.performed += ctx => leftClickPressed = true;
            leftClickAction.canceled += ctx => leftClickPressed = false;
            leftClickAction.Enable();
        }
        if (jumpAction != null)
        {
            jumpAction.performed += OnJumpPerformed;
            jumpAction.Enable();
        }

        if (lockCursorOnStart) LockCursor(true);
    }

    private void OnDisable()
    {
        if (mousePositionAction != null) mousePositionAction.Disable();
        if (moveAction != null)
        {
            moveAction.performed -= OnMovePerformed;
            moveAction.canceled -= OnMoveCanceled;
            moveAction.Disable();
        }
        if (sprintAction != null)
        {
            sprintAction.performed -= ctx => sprinting = true;
            sprintAction.canceled -= ctx => sprinting = false;
            sprintAction.Disable();
        }
        if (rightClickAction != null)
        {
            rightClickAction.performed -= ctx => rightClickPressed = true;
            rightClickAction.canceled -= ctx => rightClickPressed = false;
            rightClickAction.Disable();
        }
        if (leftClickAction != null)
        {
            leftClickAction.performed -= ctx => leftClickPressed = true;
            leftClickAction.canceled -= ctx => leftClickPressed = false;
            leftClickAction.Disable();
        }
        if (jumpAction != null)
        {
            jumpAction.performed -= OnJumpPerformed;
            jumpAction.Disable();
        }
    }

    private void Update()
    {
        if (inputEnabled)
        {
            ReadMouseDelta();
            HandleLook();
            HandleMovement();
        }
        else
        {
            if(!characterController.isGrounded)
            {
                verticalVelocity -= gravity * Time.deltaTime;
                verticalVelocity = Mathf.Max(verticalVelocity, -terminalVelocity);
                characterController.Move(new Vector3(0,verticalVelocity,0)*Time.deltaTime);
            }
        }     
    }

    private void ReadMouseDelta()
    {
        if (mousePositionAction == null) return;
        rawMouseDelta = mousePositionAction.ReadValue<Vector2>();
    }

    private void HandleLook()
    {
        bool shouldRotate = !rotateOnlyOnRightClick || rightClickPressed;
        if (!shouldRotate) return;

        Vector2 target = rawMouseDelta * (mouseSensitivity * 0.01f);
        Vector2 smooth = Vector2.SmoothDamp(Vector2.zero, target, ref currentLookVelocity, lookSmoothing);
        yaw += smooth.x * Time.deltaTime * 100f;
        pitch -= smooth.y * Time.deltaTime * 100f;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // rotate
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void HandleMovement()
    {
        Vector3 desiredLocal = Vector3.zero;
        desiredLocal += Vector3.forward * moveInput.y;
        desiredLocal += Vector3.right * moveInput.x;
        desiredLocal = Vector3.ClampMagnitude(desiredLocal, 1f);

        Vector3 desiredWorld = transform.TransformDirection(desiredLocal);

        float targetSpeed = walkSpeed * (sprinting ? sprintMultiplier : 1f);
        Vector3 targetVelocity = desiredWorld * targetSpeed;

        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, (desiredWorld.sqrMagnitude > 0.001f ? acceleration : deceleration) * Time.deltaTime);

        if (characterController.isGrounded)
        {
            if (verticalVelocity < 0f) verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
            verticalVelocity = Mathf.Max(verticalVelocity, -terminalVelocity);
        }

        Vector3 move = currentVelocity;
        move.y = verticalVelocity;

        characterController.Move(move * Time.deltaTime);
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;

        if(!enabled)
        {
            moveInput = Vector2.zero;
            rawMouseDelta = Vector2.zero;
            currentVelocity = Vector3.zero;
        }
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        if (!allowJump) return;
        if (!characterController) return;
        if (characterController.isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(2f * gravity * jumpHeight);
        }
    }

    private void LockCursor(bool value)
    {
        Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !value;
    }

    public bool IsSprinting => sprinting;
    public bool IsGrounded => characterController ? characterController.isGrounded : false;
    public Transform CameraTransform => cameraTransform;

    public void ToggleCursorLock()
    {
        bool locked = Cursor.lockState == CursorLockMode.Locked;
        LockCursor(!locked);
    }
}

