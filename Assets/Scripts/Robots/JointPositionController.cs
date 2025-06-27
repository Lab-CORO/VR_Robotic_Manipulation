using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Save the current position of the joint around its axis of rotation and make it turn to match that angle.
/// </summary>
public class JointPositionController : MonoBehaviour
{
    [SerializeField, Tooltip("Current angle position")]
    private float currentPosition;

    private void Awake()
    {
        currentPosition = 0;
    }

    /// <summary>
    /// Change the joint's angle and make it rotate.
    /// </summary>
    /// <param name="angle">Desired joint's angle in degree</param>
    public void ChangePosition(float angle)
    {
        transform.rotation *= Quaternion.AngleAxis(angle - currentPosition, Vector3.up);
        currentPosition = angle;
    }
}
