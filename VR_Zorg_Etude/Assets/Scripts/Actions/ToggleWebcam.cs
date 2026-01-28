using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RosMessageTypes.Sensor;
using Unity.Robotics.ROSTCPConnector;

/// <summary>
/// Subscribe to ROS compressed image topic and display it in an Image component with material support.
/// Migrated from USB webcam (WebCamTexture) to ROS2 image streaming.
/// </summary>
public class ToggleWebcam : MonoBehaviour
{
    private ROSConnection _rosConnection;
    private Image _image;
    private Texture2D _texture2D;

    [Header("ROS Configuration")]
    [SerializeField] private string topicName = "/image_raw";

    [Header("State Management")]
    [SerializeField, Tooltip("Determine if the image is open")]
    private bool isOpen;
    private bool _messageIsProcessed;
    private bool _messageIsReceived;

    // Start is called before the first frame update
    private void Start()
    {
        // Initialize texture and get Image component
        _texture2D = new Texture2D(1, 1);
        _image = GetComponentInChildren<Image>();

        if (_image == null)
        {
            Debug.LogError("[ToggleWebcam] No Image component found in children!");
            enabled = false;
            return;
        }

        if (_image.material != null)
        {
            _image.material.mainTexture = _texture2D;
        }

        // Connect to ROS and subscribe to topic
        try
        {
            _rosConnection = ROSConnection.GetOrCreateInstance();
            _rosConnection.Subscribe<CompressedImageMsg>(topicName, GetImage);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ToggleWebcam] Failed to connect to ROS: {e.Message}");
            enabled = false;
            return;
        }

        // Hide the image on startup
        Close();
    }

    private void LateUpdate()
    {
        if (!_messageIsProcessed) return;

        // Update the texture on the material
        if (_image != null && _image.material != null)
        {
            _image.material.mainTexture = _texture2D;
        }

        _messageIsProcessed = false;
        _messageIsReceived = false;
    }

    /// <summary>
    /// Take an image from ROS and insert it into a texture asynchronously.
    /// </summary>
    /// <param name="compressedImageMsg">A ROS message of type sensor_msgs/CompressedImage</param>
    private void GetImage(CompressedImageMsg compressedImageMsg)
    {
        if (!isOpen || _messageIsProcessed || !gameObject.activeSelf) return;
        if (_messageIsReceived) return;
        // Debug.Log($"Image reçue: {compressedImageMsg.header}");

        _messageIsReceived = true;
        StartCoroutine(ProcessImage(compressedImageMsg.data));
    }

    /// <summary>
    /// Put an image into a texture
    /// </summary>
    /// <param name="receivedImageData">Compressed image data bytes</param>
    /// <returns></returns>
    private IEnumerator ProcessImage(byte[] receivedImageData)
    {
        yield return null;
        _texture2D.LoadImage(receivedImageData);
        yield return null;
        _messageIsProcessed = true;
    }

    // Close the image stream
    private void Close()
    {
        isOpen = false;
        gameObject.SetActive(false);
    }

    // Open the image stream
    private void Open()
    {
        isOpen = true;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Play or stop the ROS image stream with a toggle.
    /// </summary>
    /// <param name="toggle">The toggle that determines if it's activated or not.</param>
    public void ToggleActivation(Toggle toggle)
    {
        switch (toggle.isOn)
        {
            case true:
                Open();
                break;
            case false:
                Close();
                break;
        }
    }

    /// <summary>
    /// Play or stop the ROS image stream with a bool.
    /// </summary>
    /// <param name="state">The desired state of the image stream.</param>
    public void ManageCamera(bool state)
    {
        switch (state)
        {
            case true:
                Open();
                break;
            case false:
                Close();
                break;
        }
    }

    private void OnDestroy()
    {
        if (_texture2D != null)
        {
            Destroy(_texture2D);
        }
    }
}
