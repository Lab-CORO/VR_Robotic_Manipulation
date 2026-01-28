using System;
using System.Collections;
using System.Collections.Generic;
using RosMessageTypes.BuiltinInterfaces;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Publish the target pose (position + orientation) relative to robot base to ROS.
/// Simplified from Twist (velocity) publishing - now sends direct pose for inverse kinematics.
/// </summary>
public class RosPubRTTwist : MonoBehaviour
{
    private ROSConnection _rosConnection;
    private PoseStampedMsg _poseStampedMsg;

    [Header("Topic name to publish to")] public string topicName = "/unity/target_pose";

    [Header("Speed Coefficient")]
    [Tooltip("Use with UI to change the percentage of the speed")] [SerializeField]
    private float speedCoefficient = 1f;

    [Header("Objects references")]
    [Tooltip("Base of the robot arm (base_0)")] [SerializeField] private Transform robotBase;
    [Tooltip("End effector of the robot")] [SerializeField] private Transform endEffector;
    [Tooltip("Target to follow")] [SerializeField] private Transform target;

    [Header("Publish Settings")]
    [Tooltip("Minimum position change (in meters) to trigger publish")] [SerializeField]
    private float positionThreshold = 0.001f;
    [Tooltip("Minimum rotation change (in degrees) to trigger publish")] [SerializeField]
    private float rotationThreshold = 0.1f;

    // Coroutine
    private bool _recurrentPublish;
    private IEnumerator _recurrentPublishCoroutine;

    // Last published pose for change detection
    private Vector3 _lastPublishedPosition;
    private Quaternion _lastPublishedRotation;
    private bool _hasPublishedOnce = false;
    
    // Start is called before the first frame update
    private void Start()
    {
        // Vérification
        if (robotBase == null)
        {
            Debug.LogError("[RosPubRTTwist] Robot base (base_0) not assigned!");
            enabled = false;
            return;
        }

        if (target == null || endEffector == null)
        {
            Debug.LogError("[RosPubRTTwist] Target or endEffector not assigned!");
            enabled = false;
            return;
        }

        Debug.Log("[RosPubRTTwist] Initializing...");

        // Connect to ROS and create a topic
        _rosConnection = ROSConnection.GetOrCreateInstance();
        _rosConnection.RegisterPublisher<PoseStampedMsg>(topicName);

        Debug.Log($"[RosPubRTTwist] Publisher registered on topic: {topicName}");

        _recurrentPublishCoroutine = RecurrentPublishPose();

        // Initialize the message
        _poseStampedMsg = new PoseStampedMsg();

        Debug.Log($"[RosPubRTTwist] Initialized. Publishing is: {(_recurrentPublish ? "ACTIVE" : "INACTIVE")}");
        Debug.Log("[RosPubRTTwist] Press P to toggle publishing ON/OFF");
    }

    // Update to handle keyboard toggle
    private void Update()
    {
        // Toggle publishing with P key
        if (Input.GetKeyDown(KeyCode.P))
        {
            PublishPersistent(!_recurrentPublish);
        }
    }

    /// <summary>
    /// Activate or deactivate the recurrent publication of the ROS message
    /// </summary>
    /// <param name="isActive"></param>
    public void PublishPersistent(bool isActive)
    {
        _recurrentPublish = isActive;

        Debug.Log($"[RosPubRTTwist] PublishPersistent called with: {isActive}");

        switch (_recurrentPublish)
        {
            case true:
                Debug.Log("[RosPubRTTwist] Starting recurrent publishing coroutine...");
                StartCoroutine(_recurrentPublishCoroutine);
                break;
            case false:
                Debug.Log("[RosPubRTTwist] Stopping recurrent publishing coroutine...");
                StopCoroutine(_recurrentPublishCoroutine);
                // Optionnel: publier une pose "neutre" ou ne rien faire
                break;
        }
    }

    /// <summary>
    /// Call the publish function every deltaTime
    /// </summary>
    /// <returns></returns>
    private IEnumerator RecurrentPublishPose()
    {
        Debug.Log("[RosPubRTTwist] RecurrentPublishPose coroutine started");
        while (_recurrentPublish)
        {
            PublishPoseMessage();
            yield return new WaitForSeconds(Time.deltaTime);
        }
        Debug.Log("[RosPubRTTwist] RecurrentPublishPose coroutine ended");
    }

    /// <summary>
    /// Publish the target pose relative to robot base to ROS using ROSGeometry for coordinate conversion
    /// Only publishes if position or rotation changed beyond thresholds
    /// </summary>
    private void PublishPoseMessage()
    {
        // Check for changes if we've published at least once
        if (_hasPublishedOnce)
        {
            // Calculate position change (in meters)
            float positionDelta = Vector3.Distance(target.position, _lastPublishedPosition);

            // Calculate rotation change (in degrees)
            float rotationDelta = Quaternion.Angle(_lastPublishedRotation, target.rotation);

            // Skip publishing if changes are below thresholds
            if (positionDelta < positionThreshold && rotationDelta < rotationThreshold)
            {
                return;
            }

            Debug.Log($"[RosPubRTTwist] Change detected - Position: {positionDelta:F4}m, Rotation: {rotationDelta:F2}°");
        }

        // Calculer la pose de target par rapport à robotBase (en coordonnées Unity)
        Vector3 localPositionUnity = robotBase.InverseTransformPoint(target.position);
        Quaternion localRotationUnity = Quaternion.Inverse(robotBase.rotation) * target.rotation;

        Debug.Log($"[RosPubRTTwist] Target Unity Position: {target.position}, Local: {localPositionUnity}");

        // Conversion Unity → ROS coordinates avec ROSGeometry
        // Utilise .To<FLU>() pour convertir automatiquement Unity (Y-up) vers ROS (Z-up)
        Vector3<FLU> rosPosition = localPositionUnity.To<FLU>();
        Quaternion<FLU> rosRotation = localRotationUnity.To<FLU>();

        Debug.Log($"[RosPubRTTwist] ROS Position: {rosPosition}, ROS Rotation: {rosRotation}");

        // Créer le message Header
        _poseStampedMsg.header = new HeaderMsg
        {
            stamp = new TimeMsg
            {
                sec = (int)Time.time,
                nanosec = (uint)((Time.time - (int)Time.time) * 1e9)
            },
            frame_id = "base_0"  // Frame de référence
        };

        // Créer le message Pose avec les coordonnées ROS converties
        _poseStampedMsg.pose = new PoseMsg
        {
            position = rosPosition,      // Vector3<FLU> se convertit automatiquement en PointMsg
            orientation = rosRotation    // Quaternion<FLU> se convertit automatiquement en QuaternionMsg
        };

        // Publier
        _rosConnection.Publish(topicName, _poseStampedMsg);
        Debug.Log($"[RosPubRTTwist] Message published to {topicName}");

        // Update last published values for next comparison
        _lastPublishedPosition = target.position;
        _lastPublishedRotation = target.rotation;
        _hasPublishedOnce = true;
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
}
