using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using UnityEngine.UI;


/// <summary>
/// Publish a twist message to ROS to move the mobile base with the controller in real time.
/// </summary>

public class RosPubMRTwist : MonoBehaviour
{
    private ROSConnection _rosConnection;
    public string topicName = "/unity/cmd_vel";
    public float linearSpeed = 0.1f;
    public float angularSpeed = 0.1f;
    private TwistMsg twistMsg;

    private bool safetyTriggerPressed;

    //Mode
    private bool rtime_enabled = false;

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
        Vector2 leftPadInput = Vector2.zero;
        Vector2 rightPadInput = Vector2.zero;
        float safetyTriggerValue = 0.0f;

        // Validate if the left trigger have been pressed. Act as dead man switch
        safetyTriggerValue = Input.GetAxis("XRI_Left_Trigger");
        if (rtime_enabled)
        {
            if (safetyTriggerValue > 0.5f)
            {
                safetyTriggerPressed = true;
            }
            else
            {
                safetyTriggerPressed = false;
            }

            if (safetyTriggerPressed)
            {
                // Input to guide the mobile base 
                leftPadInput.x = Input.GetAxis("XRI_Left_Primary2DAxis_Horizontal");
                rightPadInput.x = Input.GetAxis("XRI_Right_Primary2DAxis_Vertical");
                rightPadInput.y = Input.GetAxis("XRI_Right_Primary2DAxis_Horizontal");

                // Initialize the message
                twistMsg = new TwistMsg()
                {
                    linear = new Vector3Msg(rightPadInput.x * linearSpeed, rightPadInput.y * linearSpeed, 0),
                    angular = new Vector3Msg(0, 0, leftPadInput.x * angularSpeed)
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
    public void RealTimeModeEnabled(Toggle toggle) => rtime_enabled = toggle.isOn;
}