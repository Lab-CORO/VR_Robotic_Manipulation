using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

/// <summary>
/// Control where a world Menu on a canvas should be display or not
/// </summary>
public class DisplayWorldMenu : MonoBehaviour
{
    // Main camera
    private Transform _camera;
    
    // Determine if you can activate the menu or not in the tutorial
    [SerializeField] bool tutorialActiveState;

    // Start is called before the first frame update
    private void Start()
    {
        if (Camera.main != null) _camera = Camera.main.transform;
        
        CloseMenu();
    }

    /// <summary>
    /// Open the menu
    /// </summary>
    private void OpenMenu()
    {
        var newPos = _camera.position;
        var newRot = _camera.rotation.eulerAngles.y;
        
        gameObject.SetActive(true);
        
        // Place the Menu in front of the user
        transform.position = new Vector3(newPos.x, 0, newPos.z);
        transform.rotation = Quaternion.Euler(0, newRot, 0);
    }

    /// <summary>
    /// Close the menu
    /// </summary>
    private void CloseMenu()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Manage if the menu needs to close or to open.
    /// Don't open if in tutorial and isn't activated.
    /// </summary>
    public void ManageMenuTutorial()
    {
        if (!tutorialActiveState)
        {
            return;
        }
        
        if (gameObject.activeSelf)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }

    /// <summary>
    /// Enable or disable the possibility to open the menu in the tutorial
    /// </summary>
    /// <param name="value"></param>
    public void SetTutorialActiveState(bool value)
    {
        tutorialActiveState = value;
    }
    
    /// <summary>
    /// Manage if the menu needs to close or to open.
    /// </summary>
    public void ToggleMenu()
    {
        if (gameObject.activeSelf)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }
}
