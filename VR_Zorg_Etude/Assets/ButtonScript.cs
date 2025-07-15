using UnityEngine;
using UnityEngine.Events;

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

