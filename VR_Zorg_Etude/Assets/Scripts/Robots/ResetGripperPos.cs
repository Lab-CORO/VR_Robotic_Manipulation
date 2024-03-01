using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetGripperPos : MonoBehaviour
{
    [SerializeField] private GameObject targetGripper;

    // Start is called before the first frame update
    void Start()
    {
        targetGripper = gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            targetGripper.transform.localPosition = Vector3.zero;
        }
    }
}
