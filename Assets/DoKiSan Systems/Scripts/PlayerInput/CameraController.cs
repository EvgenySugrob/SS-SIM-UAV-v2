using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    [Header("Camera Setting")]
    [SerializeField] private Transform focusPoint;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float scrollSpeed = 5f;
    [SerializeField] private float maxPitchAngle = 80f;
    [SerializeField] private float minPitchAngle = -80f;

    [Header("Movement Constraints")]
    [SerializeField] private Vector2 horizontalBounds = new Vector2(-50f, 50f);
    [SerializeField] private Vector2 depthBounds = new Vector2(-50f, 50f);
    [SerializeField] private float maxHeight = 20f;
    [SerializeField] private float minHeight = 2f;
    [SerializeField] private float smoothDuration = 0.2f;

    private PlayerInput playerInput;

    //Input Action
    private InputAction mousePositionAction;
    private InputAction rightClickAction;
    private InputAction scrollAction;
    private InputAction moveAction;
    private InputAction sprintAction;

    //Camera State
    private Camera cam;
    private float yaw = 0f;
    private float pitch = 0f;
    private bool isRotating = false;
    private bool isSprinting = false;

    private Vector2 moveInput;
    private Vector3 cameraPosition;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        if (focusPoint == null)
        {
            GameObject focus = new GameObject("Camera Focus Point");
            focusPoint = focus.transform;
            focusPoint.position = Vector3.zero;
        }

        cameraPosition = transform.position;

        Vector3 eulerAngles = transform.eulerAngles;
        yaw = eulerAngles.y;
        pitch = eulerAngles.x;

        if (pitch > 180f) pitch -= 360f;
    }

    private void OnEnable()
    {
        playerInput = GetComponent<PlayerInput>();
        var inputActions = playerInput.actions;

        mousePositionAction = inputActions["MousePosition"];
        rightClickAction = inputActions["RightClick"];
        scrollAction = inputActions["Scroll"];
        moveAction = inputActions["Move"];
        sprintAction = inputActions["Sprint"];

        rightClickAction.started += OnRightClickStarted;
        rightClickAction.canceled += OnRightClickCanceled;
        scrollAction.performed += OnScroll;
        moveAction.performed += OnMove;
        moveAction.canceled += OnMoveStop;
        sprintAction.performed += OnSprintStart;
        sprintAction.canceled += OnSprintStop;

        mousePositionAction.Enable();
        rightClickAction.Enable();
        scrollAction.Enable();
        moveAction.Enable();
        sprintAction.Enable();
    }

    private void OnDisable()
    {
        moveInput = Vector2.zero;

        if (rightClickAction != null)
        {
            rightClickAction.started -= OnRightClickStarted;
            rightClickAction.canceled -= OnRightClickCanceled;
        }

        if (scrollAction != null)
            scrollAction.performed -= OnScroll;

        if (moveAction != null)
        {
            moveAction.performed -= OnMove;
            moveAction.canceled -= OnMoveStop;
        }

        if (sprintAction != null)
        {
            sprintAction.started -= OnSprintStart;
            sprintAction.canceled -= OnSprintStop;
        }
    }

    private void Update()
    {
        HandleRotation();
        HandleMovement();
        UpdateCameraPosition();
    }

    private void OnRightClickStarted(InputAction.CallbackContext context)
    {
        isRotating = true;
    }

    private void OnRightClickCanceled(InputAction.CallbackContext context)
    {
        isRotating = false;
    }

    private void OnScroll(InputAction.CallbackContext context)
    {
        float scrollValue = context.ReadValue<Vector2>().y;

        float targetY = cameraPosition.y + scrollValue * scrollSpeed * Time.deltaTime;
        targetY = Mathf.Clamp(targetY, minHeight, maxHeight);

        DOTween.To(() => cameraPosition.y, x => cameraPosition.y = x, targetY, smoothDuration)
        .SetUpdate(true)
        .Play();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveStop(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    private void OnSprintStart(InputAction.CallbackContext context)
    {
        isSprinting = true;
    }

    private void OnSprintStop(InputAction.CallbackContext context)
    {
        isSprinting = false;
    }

    private void HandleRotation()
    {
        if (!isRotating)
            return;

        Vector2 mouseDelta = mousePositionAction.ReadValue<Vector2>();

        yaw += mouseDelta.x * mouseSensitivity * Time.deltaTime;
        pitch -= mouseDelta.y * mouseSensitivity * Time.deltaTime;

        pitch = Mathf.Clamp(pitch, minPitchAngle, maxPitchAngle);
    }

    private void HandleMovement()
    {
        if (moveInput == Vector2.zero)
            return;

        Vector3 forward = new Vector3(MathF.Sin(yaw * Mathf.Deg2Rad), 0, Mathf.Cos(yaw * Mathf.Deg2Rad));
        Vector3 right = new Vector3(Mathf.Cos(yaw * Mathf.Deg2Rad), 0, -Mathf.Sin(yaw * Mathf.Deg2Rad));

        Vector3 movement = Vector3.zero;
        movement += forward * moveInput.y;
        movement += right * moveInput.x;

        float speed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);
        movement *= speed * Time.deltaTime;

        cameraPosition += movement;

        cameraPosition.x = Mathf.Clamp(cameraPosition.x, horizontalBounds.x, horizontalBounds.y);
        cameraPosition.z = Mathf.Clamp(cameraPosition.z, depthBounds.x, depthBounds.y);
        cameraPosition.y = Mathf.Clamp(cameraPosition.y, minHeight, maxHeight);
    }

    private void UpdateCameraPosition()
    {
        transform.position = cameraPosition;
        transform.rotation = Quaternion.Euler(pitch, yaw, 0);
    }

    public bool IsRotating => isRotating;

    public void SetFocusPoint(Transform newFocusPoint)
    {
        focusPoint = newFocusPoint;
    }

    public Vector3 GetWorldPositionUnderCursor()
    {
        Vector2 screenPosition = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.point;
        }

        return Vector3.zero;
    }

    public GameObject GetObjectUnderCursor()
    {
        Vector2 screenPosition = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.collider.gameObject;
        }

        return null;
    }
}
