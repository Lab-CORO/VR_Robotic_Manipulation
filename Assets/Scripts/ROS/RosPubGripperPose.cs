using System.Collections;
using System.Collections.Generic;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Publish a new position to a Robotiq gripper.
/// </summary>
public class RosPubGripperPose : MonoBehaviour
{
    private enum GripperPose
    {
        Close = 0,
        Open = 100
    }
    
    private ROSConnection _rosConnection;
    private Float32Msg _float32Msg;
    [Header("Topic name to publish to")] public string topicName = "/desired/topic";

    [Header("Desired pose of the gripper")] [Tooltip("100 is open and 0 is close")] [Range(0, 100)] [SerializeField]
    private float desiredPose;
    
    // Start is called before the first frame update
    private void Start()
    {
        // Connect to ROS and create a topic
        _rosConnection = ROSConnection.GetOrCreateInstance();
        _rosConnection.RegisterPublisher<Float32Msg>(topicName);
        
        // Initialize the ROS message
        _float32Msg = new Float32Msg();
    }

    /// <summary>
    /// Publish the desired pose of the gripper to ROS
    /// </summary>
    /// <param name="gripperPose">Desired pose of the gripper</param>
    private void PublishGripperPose(float gripperPose)
    {
        _float32Msg.data = gripperPose;
        _rosConnection.Publish(topicName, _float32Msg);
    }
    
    /// <summary>
    /// Publish a custom pose for the gripper
    /// </summary>
    /// <param name="value">Desired pose of the gripper</param>
    private void CustomPose(float value)
    {
        switch (value)
        {
            case < (float) GripperPose.Close:
                PublishGripperPose((float) GripperPose.Close);
                break;
            case > (float)GripperPose.Open:
                PublishGripperPose((float) GripperPose.Open);
                break;
            default:
                PublishGripperPose(value);
                break;
        }
    }

    /// <summary>
    /// Publish a message to open the gripper
    /// </summary>
    public void OpenGripper()
    {
        PublishGripperPose((float) GripperPose.Open);
    }

    /// <summary>
    /// Publish a message to close the gripper
    /// </summary>
    public void CloseGripper()
    {
        PublishGripperPose((float) GripperPose.Close);
    }

    /// <summary>
    /// Change the desired pose of the gripper to match the value of a slider
    /// </summary>
    /// <param name="slider">Slider to take the value from</param>
    public void ChangeDesiredPose(Slider slider)
    {
        desiredPose = slider.value;
    }
    
    /// <summary>
    /// Publish the desired pose
    /// </summary>
    public void PublishDesiredPose()
    {
        CustomPose(desiredPose);
    }
}
