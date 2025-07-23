//using UnityEngine;
//using Unity.Robotics.ROSTCPConnector;
//using RosMessageTypes.Geometry;

//public class RosSubMRTwist : MonoBehaviour
//{
//    private ROSConnection _rosConnection;
//    public string topicName = "/r100-0597/cmd_vel";

//    private Vector3 linearVelocity = Vector3.zero;
//    private float angularVelocity = 0.0f;


//    private void Start()
//    {
//        _rosConnection = ROSConnection.GetOrCreateInstance();
//        _rosConnection.Subscribe<TwistMsg>(topicName, TwistVelocityCallback);
//    }

//    private void TwistVelocityCallback(TwistMsg msg)
//    {
//        linearVelocity = new Vector3((float)msg.linear.x, (float)msg.linear.y, (float)msg.linear.z);
//        angularVelocity = (float)msg.angular.y;
//    }

//    private void Update()
//    {
//        transform.Translate(linearVelocity * Time.deltaTime, Space.Self);
//        float rotationY = angularVelocity * Time.deltaTime;
//        transform.Rotate(0, rotationY, 0);
//    }
//}

using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
public class RosSubMRTwist : MonoBehaviour
{
    private ROSConnection _rosConnection;
    public string topicName = "/r100-0597/cmd_vel";

    public ArticulationBody baseArticulationBody;
    public Transform baseMobileTransform;



    private Vector3 linearVelocity = Vector3.zero;
    private float angularVelocity = 0.0f;

    private Vector3 currentPosition;
    private Quaternion currentRotation;

    private void Start()
    {
        _rosConnection = ROSConnection.GetOrCreateInstance();
        _rosConnection.Subscribe<TwistMsg>(topicName, TwistVelocityCallback);

        if (baseArticulationBody == null)
            baseArticulationBody = GetComponent<ArticulationBody>();

        currentPosition = baseArticulationBody.transform.position;
        currentRotation = baseArticulationBody.transform.rotation;
    }

    private void TwistVelocityCallback(TwistMsg msg)
    {
        // ROS convention : x avant, y latéral → on les adapte au plan XZ Unity
        linearVelocity = new Vector3((float)msg.linear.x, 0f, (float)msg.linear.z);
        angularVelocity = (float)msg.angular.y;  // Rotation autour de l’axe y en ROS
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // 1. Rotation incrémentale autour de Y
        float deltaAngle = angularVelocity * dt;
        Quaternion deltaRotation = Quaternion.Euler(0f, deltaAngle * Mathf.Rad2Deg, 0f);

        // 2. Rotation actuelle = nouvelle orientation après la rotation
        currentRotation = deltaRotation * baseArticulationBody.transform.rotation;

        // 3. Translation dans le référentiel de la base mobile
        Vector3 deltaTranslation = baseMobileTransform.rotation * linearVelocity * dt;

        // 4. Calcul de la nouvelle position en tenant compte de la rotation autour de la base
        Vector3 pivot = baseMobileTransform.position;

        // Décalage du robot par rapport à la base avant rotation
        Vector3 offset = baseArticulationBody.transform.position - pivot;

        // Appliquer la rotation à l’offset → position du robot après rotation autour de la base
        Vector3 rotatedOffset = deltaRotation * offset;

        // Nouvelle position = pivot + offset tourné + translation
        currentPosition = pivot + rotatedOffset + deltaTranslation;

        // 5. Appliquer position et rotation par TeleportRoot
        baseArticulationBody.TeleportRoot(currentPosition, currentRotation);
    }






    //private void Update()
    //{
    //    float dt = Time.deltaTime;

    //    // Calcul du déplacement
    //    Vector3 deltaMove = linearVelocity * dt;
    //    float deltaAngle = angularVelocity * dt;

    //    // Appliquer la rotation autour de l’axe Y (Unity)
    //    currentRotation = Quaternion.Euler(0f, deltaAngle * Mathf.Rad2Deg, 0f) * currentRotation;

    //    // Appliquer le déplacement dans le référentiel de l’objet (prendre en compte la rotation)
    //    currentPosition += currentRotation * deltaMove;

    //    // Appliquer via teleportation
    //    baseArticulationBody.TeleportRoot(currentPosition, currentRotation);
    //}
}
