using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;

/// <summary>
/// Subscribe to  a twist message to ROS to move the mobile base.
/// </summary>

public class RosSubMRTwist : MonoBehaviour
{
    private ROSConnection _rosConnection;
    public string topicName = "/unity/cmd_vel";

    // Attribution des variables pour la base_link du robot et la base mobile  
    public ArticulationBody baseArticulationBody;
    public Transform baseMobileTransform;

    // Initialisaton dea vélocités
    private Vector3 linearVelocity = Vector3.zero;
    private float angularVelocity = 0.0f;

    // Variables de position du robot 
    private Vector3 currentPosition;
    private Quaternion currentRotation;

    private void Start()
    {
        // Connect to ROS and create a topic
        _rosConnection = ROSConnection.GetOrCreateInstance();
        _rosConnection.Subscribe<TwistMsg>(topicName, TwistVelocityCallback);

        if (baseArticulationBody == null)
            baseArticulationBody = GetComponent<ArticulationBody>();
        
        //Position actuelle du robot 
        currentPosition = baseArticulationBody.transform.position;
        currentRotation = baseArticulationBody.transform.rotation;
    }

    private void TwistVelocityCallback(TwistMsg msg)
    {
        // ROS convention : x avant, y latéral → on les adapte au plan XZ Unity
        linearVelocity = new Vector3((float)msg.linear.x, 0f, (float)msg.linear.y);
        angularVelocity = (float)msg.angular.z;  // Rotation autour de l’axe y en ROS
    }

    private void Update()
    {
        float dt = Time.deltaTime; // Intervalle de temps

        // Rotation incrémentale autour de Y
        float deltaAngle = - angularVelocity * dt;
        Quaternion deltaRotation = Quaternion.Euler(0f, deltaAngle * Mathf.Rad2Deg, 0f);

        // Rotation actuelle = nouvelle orientation après la rotation
        currentRotation = deltaRotation * baseArticulationBody.transform.rotation;

        //Translation dans le référentiel de la base mobile
        Vector3 deltaTranslation = baseMobileTransform.rotation * linearVelocity * dt;

        // Calcul de la nouvelle position en tenant compte de la rotation autour de la base
        Vector3 pivot = baseMobileTransform.position;

        // Décalage du robot par rapport à la base avant rotation
        Vector3 offset = baseArticulationBody.transform.position - pivot;

        // Appliquer la rotation à l’offset → position du robot après rotation autour de la base
        Vector3 rotatedOffset = deltaRotation * offset;

        // Nouvelle position = pivot + offset tourné + translation
        currentPosition = pivot + rotatedOffset + deltaTranslation;

        // Appliquer position et rotation par TeleportRoot
        baseArticulationBody.TeleportRoot(currentPosition, currentRotation);
    }
}
