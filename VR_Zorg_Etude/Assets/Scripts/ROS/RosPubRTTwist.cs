using System;
using System.Collections;
using System.Collections.Generic;
using RosMessageTypes.Geometry;
using Unity.Mathematics;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Publish a twist message to ROS that scale with the distance between a target and the end effector of the robot.
/// </summary>
public class RosPubRTTwist : MonoBehaviour
{
    private ROSConnection _rosConnection;
    private TwistMsg _twistMsg;
    
    [Header("Topic name to publish to")] public string topicName = "/unity/twist";
    
    [Header("Speed Coefficient")]
    [SerializeField] private float linearSpeed = .1f;
    [SerializeField] private float angularSpeed = Mathf.PI / 10f;
    [Tooltip("Use with UI to change the percentage of the speed")] [SerializeField]
    private float speedCoefficient = 1f;

    [Header("Objects references")]
    [Tooltip("End effector of the robot")] [SerializeField] private Transform endEffector;
    [Tooltip("Target to follow")] [SerializeField] private Transform target;

    [Header("Reference to limit movement"), Tooltip("Limit the movement in Z negative"), SerializeField]
    private Transform heightReference;
    [SerializeField] private float minimalHeight;

    // Bool to limit the movement
    private bool _transZOnly;
    private bool _transXYOnly;
    
    // Coroutine
    private bool _recurrentPublish;
    private IEnumerator _recurrentPublishCoroutine;
    
    // Start is called before the first frame update
    private void Start()
    {
        // Connect to ROS and create a topic
        _rosConnection = ROSConnection.GetOrCreateInstance();
        _rosConnection.RegisterPublisher<TwistMsg>(topicName);
        
        _recurrentPublishCoroutine = RecurrentPublishJoints();
        
        // Initialize the message
        _twistMsg = new TwistMsg()
        {
            angular = new Vector3Msg(0, 0, 0),
            linear = new Vector3Msg(0, 0, 0)
        };
    }

    /// <summary>
    /// Activate or deactivate the recurrent publication of the ROS message
    /// </summary>
    /// <param name="isActive"></param>
    public void PublishPersistent(bool isActive)
    {
        _recurrentPublish = isActive;

        switch (_recurrentPublish)
        {
            case true:
                StartCoroutine(_recurrentPublishCoroutine);
                break;
            case false:
                StopCoroutine(_recurrentPublishCoroutine);
                _twistMsg.angular = new Vector3Msg(0, 0, 0);
                _twistMsg.linear = new Vector3Msg(0, 0, 0);
                _rosConnection.Publish(topicName,_twistMsg);
                break;
        }
    }
    
    /// <summary>
    /// Call the publish function every deltaTime
    /// </summary>
    /// <returns></returns>
    private IEnumerator RecurrentPublishJoints()
    {
        while (_recurrentPublish)
        {
            PublishTwistMessage();
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    /// <summary>
    /// Publish the message to ROS
    /// This function works with the Doosan Coordinate System
    /// </summary>
    private void PublishTwistMessage()
    {
        var posDiff = target.position - endEffector.position;
        var rotDiff = endEffector.rotation * Quaternion.Inverse(target.rotation);

        if (!_transZOnly && !_transXYOnly)
        {
            _twistMsg.angular.x = CalculateAngularTwist(SignAngle(rotDiff.eulerAngles.z));
            _twistMsg.angular.y = CalculateAngularTwist(-SignAngle(rotDiff.eulerAngles.x));
            _twistMsg.angular.z = CalculateAngularTwist(SignAngle(rotDiff.eulerAngles.y));
        }

        if (!_transZOnly)
        {
            _twistMsg.linear.x = CalculateLinearTwist(posDiff.z);
            _twistMsg.linear.y = CalculateLinearTwist(-posDiff.x);
        }

        if (!_transXYOnly)
        {
            _twistMsg.linear.z = CheckHeight(CalculateLinearTwist(posDiff.y));
        }

        _rosConnection.Publish(topicName, _twistMsg);
    }

    /// <summary>
    /// Calculate the required linear speed to move depending on the distance
    /// </summary>
    /// <param name="difference">Difference in position on an axis from two different points</param>
    /// <returns>Speed value</returns>
    private float CalculateLinearTwist(float difference)
    {
        var absDifference = Mathf.Abs(difference);
        var sign = Mathf.Sign(difference);
        const float innerThreshold = 0.01f;
        const float outerThreshold = 0.5f;

        return absDifference switch
        {
            // Linear variation
             < innerThreshold => 0,
             >= innerThreshold and < outerThreshold => difference / outerThreshold * linearSpeed * speedCoefficient,
             _ => linearSpeed * speedCoefficient * sign

            // Logarithmic variation
            // < innerThreshold => 0,
            // >= innerThreshold and < outerThreshold => (0.4f * Mathf.Log10(absDifference / outerThreshold) + 1) *
            //                                           linearSpeed *
            //                                           speedCoefficient * sign,
            // _ => linearSpeed * speedCoefficient * sign
        };
    }

    /// <summary>
    /// Calculate the required angular speed to move depending on the distance
    /// </summary>
    /// <param name="difference">Difference in rotation on an axis from two different points</param>
    /// <returns>Speed value in degree</returns>
    private float CalculateAngularTwist(float difference)
    {
        var absDifference = Mathf.Abs(difference);
        var sign = Mathf.Sign(difference);
        const float innerThreshold = 1f;
        const float outerThreshold = 45f;

        return absDifference switch
        {
            < innerThreshold => 0,
            >= innerThreshold and < outerThreshold => difference / outerThreshold * angularSpeed * speedCoefficient,
            _ => angularSpeed * speedCoefficient * sign
        };
    }

    /// <summary>
    /// Return an angle between -180 and 180 instead of 0 to 360;
    /// This allow the rotation to go in both direction.
    /// </summary>
    /// <param name="angle">An angle between 0 and 360</param>
    /// <returns>A sign angle between -180 and 180</returns>
    private static float SignAngle(float angle)
    {
        if (angle > 180)
        {
            return angle - 360;
        }

        return angle;
    }

    /// <summary>
    /// Set the target position and rotation the same as the endEffector position and rotation
    /// </summary>
    public void ResetTargetPosition()
    {
        var transform1 = target.transform;
        var transform2 = endEffector.transform;
        transform1.position = transform2.position;
        transform1.rotation = transform2.rotation;
    }

    /// <summary>
    /// Change the speed coefficient with a slider.
    /// </summary>
    /// <param name="slider">The slider to get the value from</param>
    public void ChangeSpeedCoefficient(Slider slider)
    {
        var newCoefficient = slider.value;

        speedCoefficient = newCoefficient switch
        {
            < 0 => 0,
            > 100 => 1,
            _ => newCoefficient / 100
        };
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="toggle"></param>
    public void OnlyZTranslation(Toggle toggle)
    {
        _transZOnly = toggle.isOn;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="toggle"></param>
    public void OnlyXYTranslation(Toggle toggle)
    {
        _transXYOnly = toggle.isOn;
    }

    /// <summary>
    /// Prevent to go further down in Z if too low.
    /// </summary>
    /// <param name="speedInZ">The current desired speed to send.</param>
    /// <returns>Current speed in Z or 0 if too low with a negative speed.</returns>
    private float CheckHeight(float speedInZ)
    {
        if (heightReference.position.y < minimalHeight && speedInZ < 0)
        {
            return 0;
        }

        return speedInZ;
    }
}
