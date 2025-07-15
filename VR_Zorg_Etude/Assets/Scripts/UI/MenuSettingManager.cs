using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This script manage the buttons and the setting for a specific content. It make sure the good content is display
/// when clicking on a button while also highlighting the button.
/// </summary>
public class MenuSettingManager : MonoBehaviour
{
    // Canvas to show on opening the setting
    public GameObject defaultSettingCanvas;
    
    // Parent holding the buttons on the left of the menu
    public GameObject buttonsParent;
    // Parent holding the settings on the right
    public GameObject settingsParent;
    
    // Children list
    private List<GameObject> _uiButtons;
    private List<GameObject> _uiSettings;

    // Start is called before the first frame update
    void Start()
    {
        // Create mew empty list
        _uiButtons = new List<GameObject>();
        _uiSettings = new List<GameObject>();
        
        // Add the buttons from the child to the list
        foreach (Transform child in buttonsParent.transform)
        {
            _uiButtons.Add(child.gameObject);
        }
        
        // Add the settings canvas to the list
        foreach (Transform child in settingsParent.transform)
        {
            _uiSettings.Add(child.gameObject);
        }
        
        // Display the default setting canvas
        ModifyDisplay(defaultSettingCanvas);
    }

    /// <summary>
    /// Change the canvas to display selected setting.
    /// </summary>
    /// <param name="pressedButton">The gameObject of the button to press</param>
    public void ModifyDisplay(GameObject pressedButton)
    {
        // Get the name of the pressed button
        var settingName = pressedButton.name;

        ChangeSelectedButton(settingName);
        ChangeSettingContent(settingName);
    }

    /// <summary>
    /// Change the color of all the button to highlight only the one that was press.
    /// </summary>
    /// <param name="buttonName"></param>
    private void ChangeSelectedButton(string buttonName)
    {
        foreach (var child in _uiButtons)
        {
            // Change the color of the selected button to its Selected Color
            if (child.name == buttonName)
            {
                child.GetComponent<Image>().color = child.GetComponent<Button>().colors.selectedColor;
            }
            // Change the color of the other button to their default color
            else
            {
                var defaultColor = child.GetComponent<Button>().colors.normalColor;
                defaultColor.a = 0;
                child.GetComponent<Image>().color = defaultColor;
            }
        }
    }

    /// <summary>
    /// Change the setting canvas to display.
    /// </summary>
    /// <param name="settingName"></param>
    private void ChangeSettingContent(string settingName)
    {
        foreach (var child in _uiSettings)
        {
            child.SetActive(child.name == settingName);
        }
    }
}
