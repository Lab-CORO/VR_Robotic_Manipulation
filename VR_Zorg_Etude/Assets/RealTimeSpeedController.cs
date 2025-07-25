using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RealTimeSpeedController : MonoBehaviour
{
    public PlusMinusButton speedButton;
    public RosPubMRTwist rosPublisher;

    void Start()
    {
        if (speedButton != null && rosPublisher != null)
        {
            speedButton.OnValueChanged += (sender, value) =>
            {
                rosPublisher.ChangeRealTimeSpeed(value);
            };
        }
        else
        {
            Debug.LogError("Assign speedButton et rosPublisher dans l'inspecteur !");
        }
    }
}
