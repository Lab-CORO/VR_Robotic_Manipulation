using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.U2D.Path.GUIFramework;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

using CommonUsages = UnityEngine.XR.CommonUsages;

/// <summary>
/// Controller that manage the activation of the teleportation ray.
/// </summary>
public class TeleportationController : MonoBehaviour
{
    [Header("Teleportation Ray References")] 
    [SerializeField] private XRController leftTeleportationRay;
    [SerializeField] private XRController rightTeleportationRay;
    [SerializeField] private float activationThreshold = 0.5f;
    [SerializeField] private bool isActivate = true;

    [Header("UI Ray References")]
    [SerializeField] private XRRayInteractor leftUIRay;
    [SerializeField] private XRRayInteractor rightUIRay;

    // Variables to determine if we hover a UI
    private Vector3 _pos = new Vector3();
    private Vector3 _norm = new Vector3();
    private int _index = 0;
    private bool _validTarget = false;

    private void Start()
    {
        // Disable teleportation ray on start.
        leftTeleportationRay.gameObject.SetActive(false);
        rightTeleportationRay.gameObject.SetActive(false);
    }

    // Update is called once per frame
    private void Update()
    {
        if (!isActivate) return;
        ActivateTeleportationRay(rightTeleportationRay, rightUIRay);
        ActivateTeleportationRay(leftTeleportationRay, leftUIRay);

    }

    /// <summary>
    /// Activate the Teleportation Ray for a controller if a UI isn't hover and if the the controller's input is activated
    /// </summary>
    /// <param name="controller">The specific controller to activate</param>
    /// <param name="rayInteractor">The UI ray of the same controller</param>
    private void ActivateTeleportationRay(XRController controller, XRRayInteractor rayInteractor)
    {
        if (!controller || !rayInteractor) return;
        
        // Check if the UI Ray is hovering a UI
        var isUIRayHovering = rayInteractor.TryGetHitInfo(out _pos, out _norm, out _index, out _validTarget);
        controller.gameObject.SetActive(CheckIfActivated(controller) && !isUIRayHovering);
    }

    /// <summary>
    /// Check if the input should activate the ray
    /// </summary>
    /// <param name="controller">The specific controller to check the input.</param>
    /// <returns>True if the input is higher than the treshold</returns>
    private bool CheckIfActivated(XRController controller)
    {
        controller.inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out var axisInput);
        return axisInput.y > activationThreshold;
    }

    /// <summary>
    /// Set the activation value
    /// </summary>
    /// <param name="state">Desired state</param>
    public void SetActivation(bool state)
    {
        isActivate = state;
    }

    /// <summary>
    /// Toggle the Teleportation ray between activate or not.
    /// Used with UI toggle only.
    /// </summary>
    public void ToggleActivation()
    {
        isActivate = !isActivate;
    }
}
