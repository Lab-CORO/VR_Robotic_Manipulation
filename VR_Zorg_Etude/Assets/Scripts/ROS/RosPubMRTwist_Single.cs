using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using UnityEngine.UI;

/// <summary>
/// Publish a twist message to ROS to move manually the mobile base in manual time. 
/// </summary>

public class RosPubMRTwist_Single : MonoBehaviour
{
    private ROSConnection _rosConnection;
    public string topicName = "/unity/cmd_vel";

    public float linearSpeed = 1.0f;
    public float angularSpeed = 0.5f;

    [SerializeField] private float manualSpeedCoef = 1.0f;
    private TwistMsg twistMsg;


    //Mode
    private bool manual_enabled = false;

    // Variable links to the toggle button from the mobile base menu. 
    // Bool to limit the movement
    private bool x_transOnly = false;
    private bool y_transOnly = false;
    private bool rotOnly = false;

    private int moveDirection = 0; // 1 = avancer, -1 = reculer, 0 = arrêt

    // Start is called before the first frame update
    private void Start()
    {   // Connect to ROS and create a topic
        _rosConnection = ROSConnection.GetOrCreateInstance();
        _rosConnection.RegisterPublisher<TwistMsg>(topicName);
       
        // Initialize the message
        twistMsg = new TwistMsg()
        {
            linear = new Vector3Msg(0, 0, 0),
            angular = new Vector3Msg(0, 0, 0)
        };
    }

    private void Update()
    {
        // axis variable for TwistMsg
        float linearX = 0, linearY = 0, angular_var = 0;

        if (manual_enabled) // Manual mode enabled, allow movement along a specified axis
        {
            if (moveDirection != 0)
            {

                if (x_transOnly)
                { linearX = moveDirection * linearSpeed * manualSpeedCoef; }

                if (y_transOnly)
                { linearY = moveDirection * linearSpeed * manualSpeedCoef; }

                if (rotOnly)
                { angular_var = moveDirection * angularSpeed * manualSpeedCoef; }

                
                twistMsg = new TwistMsg() // sends the appropriate value
                {
                    linear = new Vector3Msg(linearX, linearY, 0),
                    angular = new Vector3Msg(0, 0, angular_var)
                };
            }
            else // if no movement is ordered, send 0
            {
                twistMsg = new TwistMsg()
                {
                    linear = new Vector3Msg(0, 0, 0),
                    angular = new Vector3Msg(0, 0, 0)
                };
            }
            _rosConnection.Publish(topicName, twistMsg);
        } 
    }

    // Function to assign the button to variables 
    public void XTranslationOnly(Toggle toggle) => x_transOnly = toggle.isOn;
    public void YTranslationOnly(Toggle toggle) => y_transOnly = toggle.isOn;
    public void RotationOnly(Toggle toggle) => rotOnly = toggle.isOn;
    public void ManualModeEnabled(Toggle toggle) => manual_enabled = toggle.isOn;
    public void ChangeManualSpeed(int value) => manualSpeedCoef = Mathf.Clamp01(value / 100f);
    
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
