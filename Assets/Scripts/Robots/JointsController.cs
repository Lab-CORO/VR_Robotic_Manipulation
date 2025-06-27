using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// List the joints of the robots
/// </summary>
public class JointsController : MonoBehaviour
{
    public enum ControllerType
    {
        WithArticulationBody,
        NoArticulationBody
    }
    
    public GameObject[] jointsList;
    public ControllerType controllerType;

    private void Awake()
    {
        foreach (var joint in jointsList)
        {
            switch (controllerType)
            {
                case ControllerType.WithArticulationBody:
                    if (joint.GetComponent<JointPositionController>())
                    {
                        break;
                    }
                    joint.AddComponent<JointArticulationController>();
                    break;
               
                case ControllerType.NoArticulationBody:
                    if (joint.GetComponent<JointPositionController>())
                    {
                        break;
                    }
                    joint.AddComponent<JointPositionController>();
                    break;
               
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    /// <summary>
    /// Change the position for a specific joint
    /// </summary>
    /// <param name="position">Target position of the joint in degree.</param>
    /// <param name="jointToChange">The index of the joint to change (start at 0).</param>
    public void ChangeJointPosition(float position, int jointToChange)
    {
        switch (controllerType)
        {
            case ControllerType.WithArticulationBody:
                jointsList[jointToChange].GetComponent<JointArticulationController>().ChangeTarget(position);
                break;
            case ControllerType.NoArticulationBody:
                jointsList[jointToChange].GetComponent<JointPositionController>().ChangePosition(position * -1);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }    }
}
