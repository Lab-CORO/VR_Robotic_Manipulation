using UnityEngine;

public class ManualSpeedController : MonoBehaviour
{
    public PlusMinusButton speedButton;
    public RosPubMRTwist_Single rosPublisher;

    void Start()
    {
        if (speedButton != null && rosPublisher != null)
        {
            speedButton.OnValueChanged += (sender, value) =>
            {
                rosPublisher.ChangeManualSpeed(value);
            };
        }
        else
        {
            Debug.LogError("Assign speedButton et rosPublisher dans l'inspecteur !");
        }
    }
}
