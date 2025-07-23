using UnityEngine;

public class MinusButton : MonoBehaviour
{
    public PlusMinusButton controller;

    void OnMouseDown()
    {
        controller.Decrement();
    }
}

