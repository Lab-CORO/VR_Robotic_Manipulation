using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;

public class RosPubMRTwist : MonoBehaviour
{
    private ROSConnection _rosConnection;
    public string topicName = "/r100_0597/cmd_vel";
    public float linearSpeed = 1.0f;
    public float angularSpeed = 0.5f;
    private TwistMsg twistMsg;

    private bool safetyTriggerPressed;

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
        Vector2 leftPadInput = Vector2.zero;
        Vector2 rightPadInput = Vector2.zero;
        float safetyTriggerValue = 0.0f;

        safetyTriggerValue = Input.GetAxis("XRI_Left_Trigger");
        if (safetyTriggerValue > 0.5f )
        {
            safetyTriggerPressed = true;
        }
        else
        {
            safetyTriggerPressed = false;
        }

        if (safetyTriggerPressed)
        {

            leftPadInput.x = Input.GetAxis("XRI_Left_Primary2DAxis_Horizontal");
            rightPadInput.x = Input.GetAxis("XRI_Right_Primary2DAxis_Vertical");
            rightPadInput.y = Input.GetAxis("XRI_Right_Primary2DAxis_Horizontal");

            twistMsg = new TwistMsg()
            {
                linear = new Vector3Msg(rightPadInput.x * linearSpeed , 0, rightPadInput.y * linearSpeed),
                angular = new Vector3Msg(0, leftPadInput.x * angularSpeed, 0)
            };
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
}