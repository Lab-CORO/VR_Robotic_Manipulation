using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Activate UnityEvent on pressing the button
/// </summary>
public class ButtonPhysics : MonoBehaviour
{
    // Percentage of the button pressed to activate the event
    [SerializeField] private float threshold = 0.1f;
    [SerializeField] private float deadZone = 0.025f;

    private bool _isPressed;
    private Vector3 _startPos;
    private ConfigurableJoint _joint;
    
    public UnityEvent onPressed;
    public UnityEvent onReleased;
    
    void Start()
    {
        _startPos = transform.localPosition;
        _joint = GetComponent<ConfigurableJoint>();
    }
    
    void Update()
    {
        if (!_isPressed && (GetValue() + threshold >= 1))
        {
            Pressed();
        }    
        else if (_isPressed && GetValue() - threshold <= 0)
        {
            Released();
        }
    }

    /// <summary>
    /// Get the pressed value of the button
    /// </summary>
    /// <returns>The distance pressed of the button</returns>
    private float GetValue()
    {
        var value = Vector3.Distance(_startPos, transform.localPosition) / _joint.linearLimit.limit;

        if (Math.Abs(value) < deadZone)
        {
            value = 0;
        }   
        
        return Mathf.Clamp(value, -1f, 1f);
    }

    /// <summary>
    /// Invoke Unity event on Pressed
    /// </summary>
    private void Pressed()
    {
        _isPressed = true;
        onPressed.Invoke();
    }

    /// <summary>
    /// Invoke Unity event on Release
    /// </summary>
    private void Released()
    {
        _isPressed = false;
        onReleased.Invoke();
    }
}
