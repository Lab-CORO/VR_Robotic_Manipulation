using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Control the activation of a camera connected to this PC and displays it in an image material.
/// </summary>
public class ToggleWebcam : MonoBehaviour
{
    private WebCamTexture _webCamTexture;
    private Material _material;
    private Image _image;

    [SerializeField] private int cameraIndex = 2;
    
    // Start is called before the first frame update
    private void Start()
    {
        _webCamTexture = new WebCamTexture();
        var devices = WebCamTexture.devices;
        _webCamTexture.deviceName = devices[cameraIndex].name; // 2 to take the plugged in camera
        
        
        _image = GetComponentInChildren<Image>();
        _image.material.mainTexture = _webCamTexture;
        
        // Remove from comment to see the different camera.
        // foreach (var variableCamDevice in devices)
        // {
        //     print(variableCamDevice.name);
        // }

        Close();
    }

    // Close the camera
    private void Close()
    {
        _webCamTexture.Stop();
        gameObject.SetActive(false);
    }

    // Open the camera
    private void Open()
    {
        _webCamTexture.Play();
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Play or stop a camera connected to this PC with a toggle.
    /// </summary>
    /// <param name="toggle">The toggle that determine if its activated or not.</param>
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
    /// Play or stop a camera connected to this PC with a bool.
    /// </summary>
    /// <param name="state">The desired state of the camera.</param>
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
}
