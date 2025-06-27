using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;

public class RosSubMRTwist : MonoBehaviour
{
    private ROSConnection _rosConnection;
    public string topicName = "/r100-0597/cmd_vel";

    private Vector3 linearVelocity = Vector3.zero;
    private float angularVelocity = 0.0f;


    private void Start()
    {
        _rosConnection = ROSConnection.GetOrCreateInstance();
        _rosConnection.Subscribe<TwistMsg>(topicName, TwistVelocityCallback);
    }

    private void TwistVelocityCallback(TwistMsg msg)
    {
        linearVelocity = new Vector3((float)msg.linear.x, (float)msg.linear.y, (float)msg.linear.z);
        angularVelocity = (float)msg.angular.y;
    }

    private void Update()
    {
        transform.Translate(linearVelocity * Time.deltaTime, Space.Self);
        float rotationY = angularVelocity * Time.deltaTime;
        transform.Rotate(0, rotationY, 0);
    }
}