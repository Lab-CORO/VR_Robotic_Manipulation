using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Debug script to diagnose and fix teleportation issues.
/// Attach this to the XR Origin GameObject.
/// </summary>
public class TeleportationDebugger : MonoBehaviour
{
    [Header("Auto-fix on Start")]
    [SerializeField] private bool autoFixOnStart = true;

    private void Start()
    {
        if (autoFixOnStart)
        {
            DiagnoseTeleportationSetup();
        }
    }

    [ContextMenu("Diagnose Teleportation Setup")]
    public void DiagnoseTeleportationSetup()
    {
        Debug.Log("=== TELEPORTATION DIAGNOSTIC START ===");

        // 1. Check for TeleportationProvider
        TeleportationProvider teleportProvider = FindObjectOfType<TeleportationProvider>();
        if (teleportProvider == null)
        {
            Debug.LogError("❌ NO TeleportationProvider found in scene! Teleportation will NOT work.");
            Debug.LogError("   → Fix: Add a TeleportationProvider component to the XR Origin GameObject");
        }
        else
        {
            Debug.Log($"✓ TeleportationProvider found on: {teleportProvider.gameObject.name}");
        }

        // 2. Check for LocomotionSystem
        LocomotionSystem locomotionSystem = FindObjectOfType<LocomotionSystem>();
        if (locomotionSystem == null)
        {
            Debug.LogWarning("⚠ NO LocomotionSystem found in scene.");
            Debug.LogWarning("   → Fix: Add a LocomotionSystem component to the XR Origin GameObject");
        }
        else
        {
            Debug.Log($"✓ LocomotionSystem found on: {locomotionSystem.gameObject.name}");

            // Check if XR Origin is assigned
            if (locomotionSystem.xrOrigin == null)
            {
                Debug.LogError("❌ LocomotionSystem.xrOrigin is NOT assigned!");
                Debug.LogError("   → Fix: Assign the XR Origin to the Locomotion System");
            }
            else
            {
                Debug.Log($"✓ LocomotionSystem.xrOrigin is assigned to: {locomotionSystem.xrOrigin.name}");
            }
        }

        // 3. Check all TeleportationAnchor and TeleportationArea
        var anchors = FindObjectsOfType<TeleportationAnchor>();
        var areas = FindObjectsOfType<TeleportationArea>();

        Debug.Log($"Found {anchors.Length} TeleportationAnchor(s) and {areas.Length} TeleportationArea(s)");

        foreach (var anchor in anchors)
        {
            if (anchor.teleportationProvider == null)
            {
                Debug.LogWarning($"⚠ TeleportationAnchor '{anchor.gameObject.name}' has NO TeleportationProvider assigned");

                if (teleportProvider != null && autoFixOnStart)
                {
                    anchor.teleportationProvider = teleportProvider;
                    Debug.Log($"   → AUTO-FIXED: Assigned TeleportationProvider to '{anchor.gameObject.name}'");
                }
            }
            else
            {
                Debug.Log($"✓ TeleportationAnchor '{anchor.gameObject.name}' has TeleportationProvider");
            }
        }

        foreach (var area in areas)
        {
            if (area.teleportationProvider == null)
            {
                Debug.LogWarning($"⚠ TeleportationArea '{area.gameObject.name}' has NO TeleportationProvider assigned");

                if (teleportProvider != null && autoFixOnStart)
                {
                    area.teleportationProvider = teleportProvider;
                    Debug.Log($"   → AUTO-FIXED: Assigned TeleportationProvider to '{area.gameObject.name}'");
                }
            }
            else
            {
                Debug.Log($"✓ TeleportationArea '{area.gameObject.name}' has TeleportationProvider");
            }
        }

        // 4. Check XRRayInteractor on controllers
        var rayInteractors = FindObjectsOfType<XRRayInteractor>();
        Debug.Log($"Found {rayInteractors.Length} XRRayInteractor(s)");

        foreach (var ray in rayInteractors)
        {
            Debug.Log($"  - XRRayInteractor on: {ray.gameObject.name} (Active: {ray.gameObject.activeSelf}, Enabled: {ray.enabled})");

            // Check if it has line visual
            var lineVisual = ray.GetComponent<XRInteractorLineVisual>();
            if (lineVisual != null)
            {
                Debug.Log($"    Has XRInteractorLineVisual (Enabled: {lineVisual.enabled})");
            }
        }

        // 5. Check TeleportationController
        var teleportController = FindObjectOfType<TeleportationController>();
        if (teleportController == null)
        {
            Debug.LogWarning("⚠ NO TeleportationController found in scene");
        }
        else
        {
            Debug.Log($"✓ TeleportationController found (isActivate: {teleportController.enabled})");
        }

        // 6. Check Input System
        Debug.Log("--- Input System Check ---");
        var xrControllers = FindObjectsOfType<XRController>();
        Debug.Log($"Found {xrControllers.Length} XRController(s)");

        foreach (var controller in xrControllers)
        {
            Debug.Log($"  - XRController on: {controller.gameObject.name}");
            Debug.Log($"    InputDevice valid: {controller.inputDevice.isValid}");
        }

        Debug.Log("=== TELEPORTATION DIAGNOSTIC END ===");
    }

    [ContextMenu("Force Enable Teleportation Rays")]
    public void ForceEnableTeleportationRays()
    {
        var rayInteractors = FindObjectsOfType<XRRayInteractor>(true); // Include inactive
        foreach (var ray in rayInteractors)
        {
            if (ray.gameObject.name.Contains("Teleport"))
            {
                ray.gameObject.SetActive(true);
                Debug.Log($"Force enabled: {ray.gameObject.name}");
            }
        }
    }
}
