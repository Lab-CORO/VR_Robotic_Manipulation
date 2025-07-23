using UnityEngine;

public class PlusButton : MonoBehaviour
{
    public PlusMinusButton controller;

    void OnMouseDown()
    {
        controller.Increment();
    }
}
