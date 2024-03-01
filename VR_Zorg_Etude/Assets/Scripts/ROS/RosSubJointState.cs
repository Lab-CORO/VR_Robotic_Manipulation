using System.Collections;
using System.Collections.Generic;
using RosMessageTypes.Sensor;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Subscribe to a JointState message to update the robot position in the scene.
/// </summary>
public class RosSubJointState : MonoBehaviour
{
    private ROSConnection _rosConnection;
    [Header("Topic name to subscribe to")] public string topicName = "joint_states";
    
    [Header("Joints Controller of the robot.")]
    [SerializeField] private JointsController joints;

    // Start is called before the first frame update
    private void Start()
    {
        // Connect to ROS and connect to a topic
        _rosConnection = ROSConnection.GetOrCreateInstance();
        _rosConnection.Subscribe<JointStateMsg>(topicName, ChangeJointsPosition);
    }

    /// <summary>
    /// Change the position of all the joints of the robot to math with the jointState message values.
    /// </summary>
    /// <param name="jointStateMsg">Message with JointState message type.</param>
    private void ChangeJointsPosition(JointStateMsg jointStateMsg)
    {
        if (joints.jointsList.Length != jointStateMsg.position.Length) return;

        for (var i = 0; i < jointStateMsg.position.Length; i++)
        {
            joints.ChangeJointPosition((float) jointStateMsg.position[i] * Mathf.Rad2Deg, i);
        }
    }

    
}
