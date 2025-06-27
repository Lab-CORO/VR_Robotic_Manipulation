using System.Collections;
using System.Collections.Generic;
using RosMessageTypes.Sensor;
using Unity.Robotics.ROSTCPConnector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Subscribe to a compressed image and display it onto a canvas
/// </summary>
public class RosSubImageToCanvas : MonoBehaviour
{
    private ROSConnection _rosConnection;
    [Header("Topic name to subscribe to")] public string topicName = "/desired/topic";

    [SerializeField, Tooltip("Determine if the image is open")] private bool isOpen;
    private bool _messageIsProcessed;
    private bool _messageIsReceived;
    private Texture2D _texture2D;
    private RawImage _rawImage;

    // Start is called before the first frame update
    private void Start()
    {
        _texture2D = new Texture2D(1, 1);
        _rawImage = GetComponentInChildren<RawImage>();
        _rawImage.color = Color.white;
        
        // Connect to ROS and connect to a topic
        _rosConnection = ROSConnection.GetOrCreateInstance();
        _rosConnection.Subscribe<CompressedImageMsg>(topicName, GetImage);
        
        // Hide the image on start up
        CloseImage();
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        if (!_messageIsProcessed) return;
        _rawImage.texture = _texture2D;
        _messageIsProcessed = false;
        _messageIsReceived = false;
    }

    /// <summary>
    /// Take an image from ROS and insert it into a texture asynchronously.
    /// </summary>
    /// <param name="compressedImageMsg">A Ros message of type sensor_msgs.CompressedImage</param>
    private void GetImage(CompressedImageMsg compressedImageMsg)
    {
        //print("got the image: " + compressedImageMsg.header.seq);

        if (!isOpen || _messageIsProcessed || !gameObject.activeSelf) return;
        if (_messageIsReceived) return;
        
        _messageIsReceived = true;
        //StartCoroutine(Test((int)compressedImageMsg.header.seq));
        
        StartCoroutine(ProcessImage(compressedImageMsg.data));
        
        // print(compressedImageMsg.header.seq);
    }

    // private IEnumerator Test(int seqNbr){
    //     
    //     print(seqNbr);
    //
    //     yield return new WaitForSeconds(1);
    //     _messageIsProcessed = true;
    // }
    
    /// <summary>
    /// Put an image into a texture
    /// </summary>
    /// <param name="receivedImageData"></param>
    /// <returns></returns>
    private IEnumerator ProcessImage(byte[] receivedImageData)
    {
        yield return null;
        _texture2D.LoadImage(receivedImageData);
        yield return null;
        _messageIsProcessed = true;
    }

    /// <summary>
    /// Display the canvas and start the image stream
    /// </summary>
    private void OpenImage()
    {
        isOpen = true;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Hide the canvas and stop the image stream
    /// </summary>
    private void CloseImage()
    {
        isOpen = false;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Toggle the activation of the canvas and the stream
    /// </summary>
    /// <param name="toggle">The UI toggle responsible holding the desired value</param>
    public void ToggleImage(Toggle toggle)
    {
        switch (toggle.isOn)
        {
            case true:
                OpenImage();
                break;
            case false:
                CloseImage();
                break;
        }
    }

    /// <summary>
    ///  Change the state of the canvas and of the stream
    /// </summary>
    /// <param name="state">The desired state of the camera.</param>
    public void ManageImage(bool state)
    {
        switch (state)
        {
            case true:
                OpenImage();
                break;
            case false:
                CloseImage();
                break;
        }
    }
}
