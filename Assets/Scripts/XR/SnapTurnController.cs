using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Controller that manage the different setting of the Snap turn provider.
/// </summary>
public class SnapTurnController : MonoBehaviour
{
    private SnapTurnProviderBase _snapTurnProvider;
    
    // Start is called before the first frame update
    private void Awake()
    {
        _snapTurnProvider = FindObjectOfType<SnapTurnProviderBase>();
    }

    /// <summary>
    /// Disable or Enable the rotation with snap turn provider.
    /// </summary>
    /// <param name="state">Desired state</param>
    public void SetTurnActivation(bool state)
    {
        _snapTurnProvider.enableTurnLeftRight = state;
    }

    /// <summary>
    /// Toggle the Snap turn left/right between activate or not.
    /// Used with UI toggle only.
    /// </summary>
    public void ToggleTurnActivation()
    {
        //_snapTurnProvider.enableTurnLeftRight = !_snapTurnProvider.enableTurnLeftRight;
        _snapTurnProvider.enabled = !_snapTurnProvider.enabled;
    }

    /// <summary>
    /// Change the snap turn angle to match with the UI slider value.
    /// </summary>
    /// <param name="slider">Slider to get the turn angle from.</param>
    public void ChangeTurnAngle(Slider slider)
    {
        _snapTurnProvider.turnAmount = slider.value;
    }

    /// <summary>
    /// Change the Activation Timeout value to match with the UI slider value.
    /// </summary>
    /// <param name="slider"></param>
    public void ChangeActivationTimeout(Slider slider)
    {
        _snapTurnProvider.debounceTime = slider.value;
    }
}
