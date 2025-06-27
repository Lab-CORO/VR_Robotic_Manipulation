using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Enable a grab with an offset so that the it keep its relative position to the controller.
/// </summary>
public class XROffsetGrabInteractable : XRGrabInteractable
{
    private Vector3 _initialAttachLocalPos;
    private Quaternion _initialAttachLocalRot;
    
    // Start is called before the first frame update
    void Start()
    {
        if (!attachTransform)
        {
            var grab = new GameObject("Grab Pivot");
            grab.transform.SetParent(transform,false);
            attachTransform = grab.transform;
        }
        
        // Set initial transform variables
        _initialAttachLocalPos = attachTransform.position;
        _initialAttachLocalRot = attachTransform.rotation;
    }
    
    /// <summary>
    /// Set the Attach point of the objet to grab at the controller's position.
    /// </summary>
    /// <param name="args">The input that enter on select (in most case it will be a controller's interactor.</param>
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        // Change the attach point
        if (args.interactorObject is XRBaseInteractor)
        {
            attachTransform.position = args.interactorObject.transform.position;
            attachTransform.rotation = args.interactorObject.transform.rotation;
        }
        else
        {
            attachTransform.position = _initialAttachLocalPos;
            attachTransform.rotation = _initialAttachLocalRot;
        }
        
        base.OnSelectEntered(args);
    }
}
