using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using UnityEngine.UI;

public class RosPubMRTwist_Single : MonoBehaviour
{
    private ROSConnection _rosConnection;
    public string topicName = "/r100_0597/cmd_vel";
    public float linearSpeed = 1.0f;
    public float angularSpeed = 0.5f;

    private float manualSpeedCoef = 1.0f;
    private float rtimeSpeedCoef = 1.0f;

    private TwistMsg twistMsg;
    private bool safetyTriggerPressed;

    private bool rtime_enabled = false;
    private bool manual_enabled = false;

    private bool x_transOnly = false;
    private bool y_transOnly = false;
    private bool rotOnly = false;

    private int moveDirection = 0; // 1 = avancer, -1 = reculer, 0 = arrêt


    private void Start()
    {
        _rosConnection = ROSConnection.GetOrCreateInstance();
        _rosConnection.RegisterPublisher<TwistMsg>(topicName);
        twistMsg = new TwistMsg()
        {
            linear = new Vector3Msg(0, 0, 0),
            angular = new Vector3Msg(0, 0, 0)
        };
    }

    private void Update()
    {
        Debug.Log("Update appelé");
        Vector2 leftPadInput = Vector2.zero;
        Vector2 rightPadInput = Vector2.zero;
        float safetyTriggerValue = Input.GetAxis("XRI_Left_Trigger");
        safetyTriggerPressed = safetyTriggerValue > 0.5f;

        float linearX = 0, linearY = 0, angular_var = 0;

        if (safetyTriggerPressed)
        {
            leftPadInput.x = moveDirection;
            rightPadInput.x = moveDirection;
            rightPadInput.y = moveDirection;

            if (rtime_enabled)
            {
                linearX = rightPadInput.x * linearSpeed * rtimeSpeedCoef;
                linearY = rightPadInput.y * linearSpeed * rtimeSpeedCoef;
                angular_var = leftPadInput.x * linearSpeed * rtimeSpeedCoef;

                Debug.Log($"[MODE] Temps Réel actif | Coef = {rtimeSpeedCoef:F2}");
            }
            else if (manual_enabled)
            {
                Debug.Log($"[MODE] Manuel actif | Coef = {manualSpeedCoef:F2}");

                if (x_transOnly)
                {
                    linearX = rightPadInput.x * linearSpeed * manualSpeedCoef;
                    Debug.Log("[AXE] Translation X activée");
                }

                if (y_transOnly)
                {
                    linearY = rightPadInput.y * linearSpeed * manualSpeedCoef;
                    Debug.Log("[AXE] Translation Y activée");
                }

                if (rotOnly)
                {
                    angular_var = leftPadInput.x * angularSpeed * manualSpeedCoef;
                    Debug.Log("[AXE] Rotation activée");
                }
            }
            else
            {
                Debug.LogWarning("[MODE] Aucun mode activé (manuel ou temps réel)");
            }

            twistMsg = new TwistMsg()
            {
                linear = new Vector3Msg(linearX, 0, linearY),
                angular = new Vector3Msg(0, angular_var, 0)
            };

            Debug.Log($"[VEL] Linear (X: {linearX:F2}, Z: {linearY:F2}) | Angular Y: {angular_var:F2}");
        }
        else
        {
            twistMsg = new TwistMsg()
            {
                linear = new Vector3Msg(0, 0, 0),
                angular = new Vector3Msg(0, 0, 0)
            };
        }

        _rosConnection.Publish(topicName, twistMsg);
    }

    public void XTranslationOnly(Toggle toggle) => x_transOnly = toggle.isOn;
    public void YTranslationOnly(Toggle toggle) => y_transOnly = toggle.isOn;
    public void RotationOnly(Toggle toggle) => rotOnly = toggle.isOn;
    public void ManualModeEnabled(Toggle toggle) => manual_enabled = toggle.isOn;
    public void RealTimeModeEnabled(Toggle toggle) => rtime_enabled = toggle.isOn;
    public void ChangeManualSpeed(Slider slider) => manualSpeedCoef = Mathf.Clamp01(slider.value / 100f);
    public void ChangeRealTimeSpeed(Slider slider) => rtimeSpeedCoef = Mathf.Clamp01(slider.value / 100f);
    // Quand on appuie sur le bouton "+" (avancer)
    public void MovePlusPress()
    {
        moveDirection = 1;
    }

    // Quand on appuie sur le bouton "−" (reculer)
    public void MoveMinusPress()
    {
        moveDirection = -1;
    }

    // Quand on relache l’un des deux boutons
    public void MoveRelease()
    {
        moveDirection = 0;
    }


}
