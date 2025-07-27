using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;
using RosMessageTypes.Dsr;
using System.Collections.Generic;

/// <summary>
/// Represents a point in the path with position (m) and rotation (deg)
/// </summary>
public class PointNode
{
    /// <summary> Position 3D vector in meters (Unity world coordinates)) </summary>
    public Vector3 position;

    /// <summary> Rotation 3D vector in degrees (Unity world coordinates) </summary>
    public Vector3 rotation;

    /// <summary>
    /// Initializes a new instance of the PointNode class with position and rotation.
    /// </summary>
    /// <param name="pos">Position (Unity)</param>
    /// <param name="rot">Rotation Euler (Unity)</param>
    public PointNode(Vector3 pos, Vector3 rot)
    {
        position = pos;
        rotation = rot;
    }
}

/// <summary>
/// Generates a path for the robot using Doosan's ROS2 MoveBlending service with a maximum of 50 segments
/// </summary>
public class RosClientMoveBlending_Test : MonoBehaviour
{
    ROSConnection ros;

    /// <summary>Reference to the robot's base GameObject</summary>
    public GameObject robotBase;
    /// <summary>Reference to the wall GameObject</summary>
    public GameObject Wall;

    /// <summary> Service name for MoveBlending </summary>
    public string serviceName = "/dsr01/motion/move_blending";

    /// <summary> Linked list of path points </summary>
    private LinkedList<PointNode> pathPoints = new LinkedList<PointNode>();

    /// <summary> Velocity and acceleration for the robot's movement </summary>
    public double[] velocity = new double[] { 200, 100 }; // mm/s, deg/s
    public double[] acceleration = new double[] { 200, 100 }; // mm/s^2, deg/s^2

    /// <summary> Initializes the service client for MoveBlending </summary>
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterRosService<MoveBlendingRequest, MoveBlendingResponse>(serviceName);

        AddPathPoint(new Vector3(307.2f, 3.35f, 900.39f), new Vector3(172.87f, -94.74f, -151.49f));
        AddPathPoint(new Vector3(504.55f, 414.37f, 799.34f), new Vector3(167.01f, -95.51f, 130.53f));
        AddPathPoint(new Vector3(399.54f, 235.50f, 1147.85f), new Vector3(160.48f, -95.71f, 168.06f));
        AddPathPoint(new Vector3(350.00f, 100.00f, 1100.00f), new Vector3(150.00f, -90.00f, -160.00f));
    }

    /// <summary> Calls the MoveBlending service to move the robot along the defined path </summary>
    public void CallMoveBlendingService()
    {

        MoveBlendingRequest request = new MoveBlendingRequest();
        List<Float64MultiArrayMsg> segments = new List<Float64MultiArrayMsg>();

        int count = 0;
        var node = pathPoints.First;

        // Generate segments from the linked list of path points
        while (node != null && node.Next != null && count < 50)
        {
            PointNode n1 = node.Value;
            PointNode n2 = node.Next.Value;

            //double[] pos1 = ConvertToRobotFrame(n1.position, n1.rotation);
            //double[] pos2 = ConvertToRobotFrame(n2.position, n2.rotation);
            double[] pos1 = new double[6];
            double[] pos2 = new double[6];

            pos1[0] = n1.position.x;
            pos1[1] = n1.position.y;
            pos1[2] = n1.position.z;
            pos1[3] = n1.rotation.x;
            pos1[4] = n1.rotation.y;
            pos1[5] = n1.rotation.z;

            pos2[0] = n2.position.x;
            pos2[1] = n2.position.y;
            pos2[2] = n2.position.z;
            pos2[3] = n2.rotation.x;
            pos2[4] = n2.rotation.y;
            pos2[5] = n2.rotation.z;
 

            Float64MultiArrayMsg segment = new Float64MultiArrayMsg();
            segment.data = new double[14];

            for (int j = 0; j < 6; j++)
            {
                segment.data[j] = pos1[j];
                segment.data[j + 6] = pos2[j];
            }
            segment.data[12] = 0; // LINE // 0: LINE, 1: CIRCULAR
            segment.data[13] = 20; // blending radius (mm)

            segments.Add(segment);

            node = node.Next;
            count++;
        }

        request.segment = segments.ToArray();
        request.pos_cnt = (sbyte)segments.Count;
        request.vel = velocity;
        request.acc = acceleration;
        request.time = 0.0; // 0.0 // time from velocity and acceleration
        request.@ref = 2; // DR_WORLD // 0: DR_BASE, 1: DR_TOOL, 2: DR_WORLD
        request.mode = 0; // MOVE ABSOLUTE // 0: MOVE_ABSOLUTE, 1: MOVE_RELATIVE
        request.sync_type = 0; // SYNC // 0: SYNC, 1: ASYNC

        ros.SendServiceMessage<MoveBlendingRequest>(serviceName, request);
        
    }

    //void Callback_Service(MoveBlendingResponse response)
    //{
    //    if (response.success)
    //    {
    //        Debug.Log("MoveBlending service call successful");
    //    }
    //    else
    //    {
    //        Debug.LogError($"MoveBlending service call failed with result:");
    //    }
    //}

    /// <summary>
    /// Converts a position and rotation from Unity world coordinates to the robot's coordinate frame.
    /// </summary>
    /// <param name="position"> Position in meters (Unity)</param>
    /// <param name="rotation"> Rotation in degrees (Euler)</param>
    /// <returns>Array double[6] : [x, y, z, rx, ry, rz]</returns>
    private double[] ConvertToRobotFrame(Vector3 position, Vector3 rotation)
    {
        // Convert Unity world coordinates to the robot's coordinate frame
        Vector3 worldPos = Wall.transform.InverseTransformPoint(position);
        Vector3 robotPosition = robotBase.transform.InverseTransformPoint(worldPos) * 1000.0f; // m to mm

        // Convert rotation from Unity world coordinates to the robot's coordinate frame
        Quaternion worldRot = Wall.transform.rotation * Quaternion.Euler(rotation);
        Quaternion robotRot = Quaternion.Inverse(robotBase.transform.rotation) * worldRot;
        Vector3 robotRotation = robotRot.eulerAngles;

        return new double[]
        {
            (double)robotPosition.x,
            (double)robotPosition.y,
            (double)robotPosition.z,
            (double)robotRotation.x,
            (double)robotRotation.y,
            (double)robotRotation.z
        };
    }

    /// <summary>
    /// Adds a new path point to the linked list of path points.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    public void AddPathPoint(Vector3 position, Vector3 rotation)
    {
        PointNode newNode = new PointNode(position, rotation);
        pathPoints.AddLast(newNode);
    }

    /// <summary>
    /// Clears the linked list of path points.
    /// </summary>
    public void ClearPathPoints()
    {
        pathPoints.Clear();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            CallMoveBlendingService();
            Debug.Log("MoveBlending service called");
            
        }
    }
}
