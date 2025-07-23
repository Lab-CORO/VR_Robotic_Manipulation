using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector;

public class base_mobile_recup_pos : MonoBehaviour
{
    [Header("Dependencies")]
    public ArticulationBody baseArticulationBody;
    public RosSubMRTwist rosSubMRTwist;

    private Vector3 currentPosition;
    private Quaternion currentRotation;

    void Start()
    {
        if (baseArticulationBody == null)
        {
            Debug.LogError("ArticulationBody is not assigned!");
        }

        if (rosSubMRTwist == null)
        {
            Debug.LogError("rosSubMRTwist is not assigned!");
        }

        currentPosition = rosSubMRTwist.transform.position;
        currentRotation = rosSubMRTwist.transform.rotation;
    }

    void Update()
    {
        // Get position and rotation from ROS
        currentPosition = rosSubMRTwist.transform.position;
        currentRotation = rosSubMRTwist.transform.rotation;

        Debug.Log("ROS Position: " + currentPosition);
        Debug.Log("ROS Rotation: " + currentRotation.eulerAngles);

        // Ensure this is the root body
        if (baseArticulationBody != null && baseArticulationBody.isRoot)
        {
            baseArticulationBody.TeleportRoot(currentPosition, currentRotation);

            // Reset velocities
            baseArticulationBody.velocity = Vector3.zero;
            baseArticulationBody.angularVelocity = Vector3.zero;
        }
    }
}
