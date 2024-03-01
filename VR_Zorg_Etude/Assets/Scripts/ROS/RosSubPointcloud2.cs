using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using RosMessageTypes.Sensor;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Subscribe to a pointcloud2 message and manage when to displays it.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RosSubPointcloud2 : MonoBehaviour
{
    private ROSConnection _rosConnection;
    [Header("Topic name to subscribe to")] public string topicName = "/desired/pointcloud2_topic";
    
    // Mesh variables
    private Mesh _mesh;
    private Vector3[] _vertices;
    private Color[] _colors;

    private bool _isMessageReceived;
    private bool _isMessageProcessed;
    public bool isOpen;

    private Thread _thread;

    [SerializeField, Tooltip("Transform of where the camera should be.")]
    private Transform attachPoint;

    // Start is called before the first frame update
    private void Start()
    {
        // Connect to ROS and connect to a topic
        _rosConnection = ROSConnection.GetOrCreateInstance();
        _rosConnection.Subscribe<PointCloud2Msg>(topicName, ReceivedMessage); 
        
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;
        
        // Hide the image on start up
        ClosePointcloud2();
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        if (isOpen && _isMessageReceived)
        {
            ProcessMessage();
        }
    }

    /// <summary>
    /// Store the message information and block other message while the current pointcloud2 is updating.
    /// </summary>
    /// <param name="message">Ros message of type Pointcloud2</param>
    private void ReceivedMessage(PointCloud2Msg message)
    {
        //print(message.header.seq);
        
        // return;
        
        if (_isMessageReceived || !isOpen)
        {
            return;
        }
        
        // print(message.header.seq);
        
        //StartCoroutine(ProcessData(message));
        ////print("got the message");        
        _isMessageReceived = true;
        
        var pointStep = message.point_step;
        var I = message.data.Length / pointStep;
        var rgbPointS3 = new RgbPoint3[I];
        var byteSlice = new byte[pointStep];
        
        _vertices = new Vector3[I];
        _colors = new Color[I];
        
        for (var i = 0; i < I; i++)
        {
            Array.Copy(message.data, i * pointStep, byteSlice, 0, pointStep);
            rgbPointS3[i] = new RgbPoint3(byteSlice, message.fields);
        
            _vertices[i].x = -rgbPointS3[i].y;
            _vertices[i].y = rgbPointS3[i].z;
            _vertices[i].z = rgbPointS3[i].x;
        
            _colors[i].r = rgbPointS3[i].rgb[2] / 255f;
            _colors[i].g = rgbPointS3[i].rgb[1] / 255f;
            _colors[i].b = rgbPointS3[i].rgb[0] / 255f;
            _colors[i].a = 1f;
        }
    }

    private IEnumerator ProcessData(PointCloud2Msg message)
    {
        yield return new WaitForSeconds(.1f);
        
        _isMessageReceived = true;

        var pointStep = message.point_step;
        var I = message.data.Length / pointStep;
        var rgbPointS3 = new RgbPoint3[I];
        var byteSlice = new byte[pointStep];

        _vertices = new Vector3[I];
        _colors = new Color[I];

        for (var i = 0; i < I; i++)
        {
            Array.Copy(message.data, i * pointStep, byteSlice, 0, pointStep);
            rgbPointS3[i] = new RgbPoint3(byteSlice, message.fields);

            _vertices[i].x = -rgbPointS3[i].y;
            _vertices[i].y = rgbPointS3[i].z;
            _vertices[i].z = rgbPointS3[i].x;

            _colors[i].r = rgbPointS3[i].rgb[2] / 255f;
            _colors[i].g = rgbPointS3[i].rgb[1] / 255f;
            _colors[i].b = rgbPointS3[i].rgb[0] / 255f;
            _colors[i].a = 1f;
            
        }
        
        yield return null;
        
        ProcessMessage();
        yield return null;
    }

    /// <summary>
    /// Update the Mesh to match with the new point cloud received.
    /// </summary>
    private void ProcessMessage()
    {
        if (_isMessageProcessed)
        {
            return;
        }
        
        _isMessageProcessed = true;

        _mesh.Clear();
        _mesh.vertices = _vertices;
        _mesh.colors = _colors;

        var indices = new int[_vertices.Length];

        for (var i = 0; i < _vertices.Length; i++)
        {
            indices[i] = i;
        }
        
        _mesh.SetIndices(indices, MeshTopology.Points, 0);
        _mesh.RecalculateBounds();

        var o = gameObject;
        o.transform.position = attachPoint.position;
        o.transform.rotation = attachPoint.rotation;
        
        _isMessageProcessed = false;
        _isMessageReceived = false;
    }

    /// <summary>
    /// Open the point cloud
    /// </summary>
    private void OpenPointcloud2()
    {
        isOpen = true;
        gameObject.SetActive(isOpen);
    }

    /// <summary>
    /// Close the point cloud
    /// </summary>
    private void ClosePointcloud2()
    {
        isOpen = false;
        gameObject.SetActive(isOpen);
    }

    /// <summary>
    /// Manage if the point cloud should be open or not.
    /// </summary>
    /// <param name="toggle">UI toggle that will manage the point cloud</param>
    public void ManagePointcloud2(Toggle toggle)
    {
        switch (toggle.isOn)
        {
            case true:
                OpenPointcloud2();
                break;
            case false:
                ClosePointcloud2();
                break;
        }
    }

    /// <summary>
    /// Manage if the point cloud should be open or not.
    /// </summary>
    /// <param name="state">The desired state of the point cloud.</param>
    public void ManagePointcloud2(bool state)
    {
        switch (state)
        {
            case true:
                OpenPointcloud2();
                break;
            case false:
                ClosePointcloud2();
                break;
        }
    }
}
