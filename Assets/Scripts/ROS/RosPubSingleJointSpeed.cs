using System;
using System.Collections;
using System.Collections.Generic;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using UnityEngine.UI;

public class RosPubSingleJointSpeed : MonoBehaviour
{
    private ROSConnection _rosConnection;
    private Float64MultiArrayMsg _jointsSpeed;

    [Header("Topic name to publish to")] public string topicName = "/desired/Float64multiarray_topic";
    
    [Header("Speed Variables")] 
    [Tooltip("Speed in radian"), SerializeField] private float maxSpeed = Mathf.PI / 10;
    [Tooltip("Use with UI to change the percentage of the speed. From 0 to 100"), SerializeField]
    private float speedCoefficient = 50f;

    private bool _isActive;
    private int _jointToMove;
    
    // Start is called before the first frame update
    public void Start()
    {
        // Connect to ROS and create a topic
        _rosConnection = ROSConnection.GetOrCreateInstance();
        _rosConnection.RegisterPublisher<Float64MultiArrayMsg>(topicName);

        _jointsSpeed = new Float64MultiArrayMsg()
        {
            data =  new double[FindObjectOfType<JointsController>().jointsList.Length]
        };
    }

    /// <summary>
    /// Publish a Float64MultiArray
    /// </summary>
    private void PublishMessage()
    {
        _rosConnection.Publish(topicName,_jointsSpeed);
    }

    /// <summary>
    /// Publish a speed of zero on all joints
    /// </summary>
    private void PublishZero()
    {
        for (var i = 0; i < _jointsSpeed.data.Length; i++)
        {
            _jointsSpeed.data[i] = 0;
        }
        
        PublishMessage();
    }
    
    /// <summary>
    /// Activate or deactivate the single joint control.
    /// </summary>
    /// <param name="toggle">The toggle to check the value from.</param>
    public void ActiveJoint(Toggle toggle)
    {
        _isActive = toggle.isOn;
    }
    
    /// <summary>
    /// Set the joint number so that only that one will move.
    /// -1 since the joint list starts at 0 but the joint name start at 1.
    /// </summary>
    /// <param name="jointNumber">The number of the joint to move.</param>
    public void SetJointToMove(int jointNumber)
    {
        _jointToMove = jointNumber - 1;
    }
    
    /// <summary>
    /// Change the speed coefficient with a slider.
    /// </summary>
    /// <param name="slider">The slider to get the value from</param>
    public void ChangeSpeedCoefficient(Slider slider)
    {
        var sliderValue = slider.value;
        speedCoefficient = sliderValue;
    }
    
    /// <summary>
    /// Send a speed to move a specific joint.
    /// </summary>
    /// <param name="slider">The slider that determine if you need to publish a speed or not.</param>
    public void JointPublishSpeed(Slider slider)
    {
        if (!_isActive)
        {
            return;
        }
        
        switch (slider.value)
        {
            case 0:
                PublishZero();
                break;
            default:
                for (var i = 0; i < _jointsSpeed.data.Length; i++)
                {
                    if (i != _jointToMove)
                    {
                        _jointsSpeed.data[i] = 0;
                    }
                    else
                    {
                        _jointsSpeed.data[i] = slider.value * maxSpeed * (speedCoefficient / 100) ;
                    }
                }
                PublishMessage();
                break;
        }
    }
}
