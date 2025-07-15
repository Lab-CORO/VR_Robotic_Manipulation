using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Control the target of a specific joint to be able to make it moves.
/// </summary>
public class JointArticulationController : MonoBehaviour
{
    private float _targetPosition;
    private bool _changePosition;
    
    private ArticulationBody _articulationBody;
    private ArticulationDrive _articulationDrive;
    
    // Start is called before the first frame update
    private void Start()
    {
        _articulationBody = GetComponent<ArticulationBody>();
        _targetPosition = 0;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (!_changePosition) return;
        
        // Change the target of the articulation body
        _articulationDrive = _articulationBody.xDrive;
        _articulationDrive.target = _targetPosition;
        _articulationBody.xDrive = _articulationDrive;

        _changePosition = false;
    }

    /// <summary>
    /// Change the target of the articulationBody drive
    /// </summary>
    /// <param name="newTarget"></param>
    public void ChangeTarget(float newTarget)
    {
        _targetPosition = newTarget;
        _changePosition = true;
    }

    /// <summary>
    /// Get the current position of the joint in degree
    /// </summary>
    /// <returns>Angle position in degree</returns>
    public float GetPositionInDegree()
    {
        return Mathf.Rad2Deg * _articulationBody.jointPosition[0];
    }

    /// <summary>
    /// Get the current position of the joint in radian
    /// </summary>
    /// <returns>Angle position in radian</returns>
    public float GetPositionInRadian()
    {
        return _articulationBody.jointPosition[0];
    }
}
