using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fix the camera in world space
/// </summary>
public class FixRosCamera : MonoBehaviour
{
    private bool _isFix;
    private Vector3 _fixPosition;
    private Quaternion _fixRotation;
    
    private Vector3 _initPosition;
    private Quaternion _initRotation;

    // Start is called before the first frame update
    private void Start()
    {
        // Save the initial transformation of the canvas
        var o = gameObject;
        _initPosition = o.transform.localPosition;
        _initRotation = o.transform.localRotation;
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        if(!_isFix) return;
        
        var o = gameObject;
        o.transform.position = _fixPosition;
        o.transform.rotation = _fixRotation;
        
    }

    /// <summary>
    /// Toggle to fix or not the camera in world space
    /// </summary>
    /// <param name="toggle"></param>
    public void ToggleFixCamera(Toggle toggle)
    {
        _isFix = toggle.isOn;
        var o = gameObject;
        
        // Fix the camera transform in world space
        if (_isFix)
        {
            _fixPosition = o.transform.position;
            _fixRotation = o.transform.rotation;
        }
        // Return the camera transform in local space
        else
        {
            o.transform.localPosition = _initPosition;
            o.transform.localRotation = _initRotation;
        }
    }
}
