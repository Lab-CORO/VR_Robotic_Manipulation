using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Invoke a event if the button have been pressed or released 
/// </summary>
public class ButtonScript : MonoBehaviour
{
    public UnityEvent OnPress;
    public UnityEvent OnRelease;

    public void Press()
    {
        Debug.Log("Button Pressed");
        OnPress.Invoke();
    }

    public void Release()
    {
        Debug.Log("Button Released");
        OnRelease.Invoke();
    }
}

