using System.Collections;
using System.Collections.Generic;
using RosMessageTypes.Sensor;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Subscribe to a JointState message to update the robot position in the scene.
/// Maps joint positions by name to handle non-sequential ordering.
/// </summary>
public class RosSubJointState : MonoBehaviour
{
    private ROSConnection _rosConnection;
    [Header("Topic name to subscribe to")]
    public string topicName = "joint_states";

    [Header("Joints Controller of the robot.")]
    [SerializeField] private JointsController joints;

    [Header("Joint Name Mapping")]
    [Tooltip("Expected joint names in Unity order (e.g., joint_1, joint_2, joint_3, joint_4, joint_5, joint_6)")]
    [SerializeField] private string[] expectedJointNames = new string[]
    {
        "joint_1", "joint_2", "joint_3", "joint_4", "joint_5", "joint_6"
    };

    [Header("Debug")]
    [SerializeField] private bool logJointMapping = false;
    [SerializeField] private bool logReceivedOrder = true;

    private Dictionary<string, int> jointNameToUnityIndex;
    private bool isInitialized = false;

    // Start is called before the first frame update
    private void Start()
    {
        // Initialize joint name mapping
        InitializeJointMapping();

        // Connect to ROS and connect to a topic
        _rosConnection = ROSConnection.GetOrCreateInstance();
        _rosConnection.Subscribe<JointStateMsg>(topicName, ChangeJointsPosition);
    }

    /// <summary>
    /// Initialize the mapping between joint names and Unity joint indices.
    /// </summary>
    private void InitializeJointMapping()
    {
        jointNameToUnityIndex = new Dictionary<string, int>();

        for (int i = 0; i < expectedJointNames.Length; i++)
        {
            jointNameToUnityIndex[expectedJointNames[i]] = i;

            if (logJointMapping)
            {
                Debug.Log($"[RosSubJointState] Mapped '{expectedJointNames[i]}' to Unity index {i}");
            }
        }

        isInitialized = true;
        Debug.Log($"[RosSubJointState] Joint mapping initialized with {expectedJointNames.Length} joints");
    }

    /// <summary>
    /// Change the position of all the joints of the robot to match with the jointState message values.
    /// Uses joint names to correctly map positions regardless of message order.
    /// </summary>
    /// <param name="jointStateMsg">Message with JointState message type.</param>
    private void ChangeJointsPosition(JointStateMsg jointStateMsg)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[RosSubJointState] Joint mapping not initialized yet!");
            return;
        }

        // Check if we have joint names in the message
        if (jointStateMsg.name == null || jointStateMsg.name.Length == 0)
        {
            Debug.LogError("[RosSubJointState] No joint names in message! Cannot map positions.");
            return;
        }

        if (jointStateMsg.name.Length != jointStateMsg.position.Length)
        {
            Debug.LogError($"[RosSubJointState] Mismatch: {jointStateMsg.name.Length} names vs {jointStateMsg.position.Length} positions");
            return;
        }

        // Log received order (only first time or if enabled)
        if (logReceivedOrder)
        {
            string orderStr = "Received joint order: ";
            for (int i = 0; i < jointStateMsg.name.Length; i++)
            {
                orderStr += jointStateMsg.name[i];
                if (i < jointStateMsg.name.Length - 1) orderStr += ", ";
            }
            Debug.Log($"[RosSubJointState] {orderStr}");
            logReceivedOrder = false; // Only log once
        }

        // Map each joint position using the joint name
        for (int i = 0; i < jointStateMsg.name.Length; i++)
        {
            string jointName = jointStateMsg.name[i];

            // Find the Unity index for this joint name
            if (jointNameToUnityIndex.TryGetValue(jointName, out int unityIndex))
            {
                float positionInDegrees = (float)jointStateMsg.position[i] * Mathf.Rad2Deg;
                joints.ChangeJointPosition(positionInDegrees, unityIndex);
            }
            else
            {
                Debug.LogWarning($"[RosSubJointState] Unknown joint name '{jointName}' received from ROS");
            }
        }
    }
}
