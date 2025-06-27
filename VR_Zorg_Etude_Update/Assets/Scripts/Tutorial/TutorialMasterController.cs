using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Control the different functionalities available to the VR controllers.
/// </summary>
public class TutorialMasterController : MonoBehaviour
{
    [Header("Setup")] 
    [SerializeField] private Transform startingPosition;
    [SerializeField] private GameObject teleporterParent;

    [Header("Controller References")]
    [SerializeField] private XRController leftDirectInteractor;
    [SerializeField] private XRController rightDirectInteractor;
    
    private TeleportationController _teleportationController;
    private SnapTurnController _snapTurnController;
    
    // Start is called before the first frame update
    private void Start()
    {
        // Disable grip
        leftDirectInteractor.selectUsage = InputHelpers.Button.None;
        rightDirectInteractor.selectUsage = InputHelpers.Button.None;
        
        // Set the threshold for the Teleportation Ray to high to activate
        _teleportationController = GetComponent<TeleportationController>();
        _teleportationController.SetActivation(false);

        // Disable Snap turn provider
        _snapTurnController = GetComponent<SnapTurnController>();
        _snapTurnController.SetTurnActivation(false);
    }

    /// <summary>
    /// Enable the controller to select. This means it now should be able to grab.
    /// </summary>
    public void EnableSelectUsage()
    {
        leftDirectInteractor.selectUsage = InputHelpers.Button.Grip;
        rightDirectInteractor.selectUsage = InputHelpers.Button.Grip;
    }
    /// <summary>
    /// Enable the user to move with the controller.
    /// </summary>
    public void EnableLocomotion()
    {
        _teleportationController.SetActivation(true);
        _snapTurnController.SetTurnActivation(true);
    }
}
