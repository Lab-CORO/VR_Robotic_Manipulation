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

    private CanvasGroup _canvasGroup;

    // Start is called before the first frame update
    private void Start()
    {
        if (Camera.main != null) _camera = Camera.main.transform;

        // Try to get or add CanvasGroup for visibility control
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            Debug.LogWarning("[DisplayWorldMenu] No CanvasGroup found, adding one automatically.");
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        CloseMenu();
        Debug.Log("[DisplayWorldMenu] Initialized. GameObject active: " + gameObject.activeSelf);
    }

    // Update is called once per frame
    private void Update()
    {
        // Toggle menu with M key (for Menu) or Tab key in desktop mode
        if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("[DisplayWorldMenu] M or Tab pressed!");
            ToggleMenu();
        }
    }

    /// <summary>
    /// Open the menu
    /// </summary>
    private void OpenMenu()
    {
        var newPos = _camera.position;
        var newRot = _camera.rotation.eulerAngles.y;

        // Use CanvasGroup to show menu instead of SetActive(true)
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }
        else
        {
            gameObject.SetActive(true);
        }

        // Place the Menu in front of the user
        transform.position = new Vector3(newPos.x, 0, newPos.z);
        transform.rotation = Quaternion.Euler(0, newRot, 0);

        Debug.Log("[DisplayWorldMenu] Menu opened at position: " + transform.position);
    }

    /// <summary>
    /// Close the menu
    /// </summary>
    private void CloseMenu()
    {
        // Use CanvasGroup to hide menu instead of SetActive(false)
        // This keeps the script active so it can still receive keyboard input
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
        else
        {
            gameObject.SetActive(false);
        }
        Debug.Log("[DisplayWorldMenu] Menu closed");
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

        // Check if menu is visible using CanvasGroup alpha
        bool isMenuVisible = _canvasGroup != null ? _canvasGroup.alpha > 0.5f : gameObject.activeSelf;

        if (isMenuVisible)
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
        // Check if menu is visible using CanvasGroup alpha
        bool isMenuVisible = _canvasGroup != null ? _canvasGroup.alpha > 0.5f : gameObject.activeSelf;

        if (isMenuVisible)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }
}
