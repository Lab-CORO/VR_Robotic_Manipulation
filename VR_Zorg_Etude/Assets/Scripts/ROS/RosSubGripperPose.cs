using System.Collections;
using System.Collections.Generic;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;

/// <summary>
/// Move the gripper to a specific position using an animator.
/// </summary>
public class RosSubGripperPose : MonoBehaviour
{
    private Animator _animator;
    private static readonly int ActivateGripper = Animator.StringToHash("ActivateGripper");
    private static readonly int Frame = Animator.StringToHash("Frame");

    [Tooltip("Maximum open distance of the gripper (meter)")]
    public float maxOpenDistance;

    private ROSConnection _rosConnection;
    [Header("Topic name to publish to")] public string topicName = "/desired/topic";

    // Start is called before the first frame update
    private void Start()
    {
        // Initialize the animator
        _animator = GetComponent<Animator>();
        _animator.SetTrigger(ActivateGripper);
        
        // Connect to ROS and create a topic
        _rosConnection = ROSConnection.GetOrCreateInstance();
        _rosConnection.Subscribe<Float32Msg>(topicName, SetGripperPose);
    }

    /// <summary>
    /// Set the animator to a specific frame to match the gripper position
    /// </summary>
    /// <param name="message">Position of the gripper</param>
    private void SetGripperPose(Float32Msg message)
    {
        if (_animator)
        {
            // 0 is open and 1 is close
            _animator.SetFloat(Frame, (maxOpenDistance - message.data) / maxOpenDistance);
        }
    }
}
