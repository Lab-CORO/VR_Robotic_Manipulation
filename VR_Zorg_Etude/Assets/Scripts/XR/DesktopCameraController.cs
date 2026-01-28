using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

/// <summary>
/// Desktop keyboard and mouse controller for VR environment.
/// Allows navigation without VR headset for development and testing.
/// Automatically disables when VR device is detected.
/// </summary>
public class DesktopCameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float verticalSpeed = 3f;

    [Header("Mouse Look Settings")]
    [SerializeField] private float mouseSensitivity = 2f;

    [Header("VR Detection")]
    [SerializeField] private bool autoDetectVR = true;

    [Header("Target Control Settings")]
    [SerializeField, Tooltip("Robot target to control with arrow keys")]
    private Transform robotTarget;

    [SerializeField, Tooltip("Speed for moving the target in meters per second")]
    private float targetMoveSpeed = 0.1f;

    // Private variables
    private Transform cameraTransform;      // Camera Offset transform
    private bool isDesktopMode = false;     // Desktop mode active or not
    private bool isCursorLocked = false;    // Cursor lock state
    private float rotationX = 0f;           // Vertical rotation (pitch)
    private float rotationY = 0f;           // Horizontal rotation (yaw)

    void Start()
    {
        // Find the main camera's parent (Camera Offset)
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cameraTransform = mainCamera.transform.parent;
            if (cameraTransform == null)
            {
                Debug.LogWarning("DesktopCameraController: Main Camera has no parent. Mouse look may not work correctly.");
                cameraTransform = mainCamera.transform;
            }
        }
        else
        {
            Debug.LogError("DesktopCameraController: Main Camera not found. Script disabled.");
            enabled = false;
            return;
        }

        // Initialize rotation with current transform rotation
        Vector3 currentRotation = transform.eulerAngles;
        rotationY = currentRotation.y;

        Vector3 cameraRotation = cameraTransform.localEulerAngles;
        rotationX = cameraRotation.x;
        if (rotationX > 180f) rotationX -= 360f; // Normalize to -180 to 180

        // Check if VR device is active
        bool vrActive = autoDetectVR && IsVRDeviceActive();

        if (vrActive)
        {
            Debug.Log("DesktopCameraController: VR device detected, disabling desktop controls.");
            isDesktopMode = false;
        }
        else
        {
            Debug.Log("DesktopCameraController: Desktop mode enabled.");
            isDesktopMode = true;
            LockCursor();
        }
    }

    void Update()
    {
        if (!isDesktopMode) return;

        // Toggle cursor lock with ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleCursorLock();
        }

        // Toggle desktop mode with F1 (debug)
        if (Input.GetKeyDown(KeyCode.F1))
        {
            isDesktopMode = !isDesktopMode;
            Debug.Log($"DesktopCameraController: Desktop mode {(isDesktopMode ? "enabled" : "disabled")}.");

            if (!isDesktopMode)
            {
                UnlockCursor();
            }
            else
            {
                LockCursor();
            }
        }

        // Handle movement and mouse look
        HandleMovement();
        HandleMouseLook();
        HandleTargetMovement();  // Control robot target with arrow keys
    }

    /// <summary>
    /// Handles WASD movement and vertical controls
    /// </summary>
    private void HandleMovement()
    {
        // Get input axes
        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical = Input.GetAxis("Vertical");     // W/S

        // Calculate movement direction relative to XR Origin orientation
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        // Remove vertical component for horizontal movement
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        // Calculate horizontal movement
        Vector3 movement = (forward * vertical + right * horizontal) * moveSpeed * Time.deltaTime;

        // Apply sprint multiplier
        if (Input.GetKey(KeyCode.LeftShift))
        {
            movement *= sprintMultiplier;
        }

        // Vertical movement (Space = up, Left Ctrl = down)
        if (Input.GetKey(KeyCode.Space))
        {
            movement.y += verticalSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            movement.y -= verticalSpeed * Time.deltaTime;
        }

        // Apply movement to XR Origin
        transform.position += movement;

        // Optional: Mouse scroll wheel to adjust speed
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            moveSpeed = Mathf.Clamp(moveSpeed + scroll * 5f, 1f, 20f);
        }
    }

    /// <summary>
    /// Handles mouse look rotation
    /// </summary>
    private void HandleMouseLook()
    {
        if (!isCursorLocked) return;

        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Horizontal rotation (Y-axis): Rotate XR Origin
        rotationY += mouseX;
        transform.rotation = Quaternion.Euler(0f, rotationY, 0f);

        // Vertical rotation (X-axis): Rotate Camera Offset
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f); // Prevent over-rotation
        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
    }

    /// <summary>
    /// Handles IJKL and UO keys to move the robot target in world space
    /// </summary>
    private void HandleTargetMovement()
    {
        if (robotTarget == null) return;

        // Get keyboard input
        float targetX = 0f;  // Left/Right
        float targetY = 0f;  // Up/Down
        float targetZ = 0f;  // Forward/Backward

        // Horizontal movement (IJKL)
        if (Input.GetKey(KeyCode.I)) targetZ += 1f;       // Forward (+Z)
        if (Input.GetKey(KeyCode.K)) targetZ -= 1f;       // Backward (-Z)
        if (Input.GetKey(KeyCode.J)) targetX -= 1f;       // Left (-X)
        if (Input.GetKey(KeyCode.L)) targetX += 1f;       // Right (+X)

        // Vertical movement (UO)
        if (Input.GetKey(KeyCode.U)) targetY += 1f;       // Up (+Y)
        if (Input.GetKey(KeyCode.O)) targetY -= 1f;       // Down (-Y)

        // Calculate movement in world space
        Vector3 targetMovement = new Vector3(targetX, targetY, targetZ) * targetMoveSpeed * Time.deltaTime;

        // Apply movement
        robotTarget.position += targetMovement;
    }

    /// <summary>
    /// Checks if a VR device is active and tracking
    /// </summary>
    /// <returns>True if VR device is detected</returns>
    private bool IsVRDeviceActive()
    {
        // Check if XR is initialized
        if (XRGeneralSettings.Instance == null) return false;
        if (XRGeneralSettings.Instance.Manager == null) return false;
        if (XRGeneralSettings.Instance.Manager.activeLoader == null) return false;

        // Check if a head-mounted display is present and tracking
        var inputDevices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.HeadMounted,
            inputDevices
        );

        return inputDevices.Count > 0;
    }

    /// <summary>
    /// Locks the cursor to the center of the screen
    /// </summary>
    private void LockCursor()
    {
        isCursorLocked = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Unlocks the cursor and makes it visible
    /// </summary>
    private void UnlockCursor()
    {
        isCursorLocked = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Toggles cursor lock state
    /// </summary>
    private void ToggleCursorLock()
    {
        if (isCursorLocked)
        {
            UnlockCursor();
        }
        else
        {
            LockCursor();
        }
    }

    /// <summary>
    /// Clean up cursor state when script is disabled or destroyed
    /// </summary>
    void OnDestroy()
    {
        UnlockCursor();
    }
}
