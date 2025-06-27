using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class GetDrivePos : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI minText;
    [SerializeField] private TextMeshProUGUI maxText;
    [SerializeField] private TextMeshProUGUI currentText;

    [SerializeField] private int jointNumber;

    private ArticulationBody _articulationBody;
    
    // Start is called before the first frame update
    private void Start()
    {
        // Start at 0 in the list but 1 with the name
        jointNumber -= 1;
        
        // Get the text component of the children
        minText = transform.Find("Min").gameObject.GetComponent<TextMeshProUGUI>();
        maxText = transform.Find("Max").GetComponent<TextMeshProUGUI>();
        currentText = transform.Find("Current").GetComponent<TextMeshProUGUI>();

        // Get the ArticulationBody of the right joint
        _articulationBody = FindObjectOfType<JointsController>().jointsList[jointNumber]
            .GetComponent<ArticulationBody>();

        // Set the lower and the upper limit of the joints in the UI.
        minText.text = _articulationBody.xDrive.lowerLimit.ToString(CultureInfo.InvariantCulture);
        maxText.text = _articulationBody.xDrive.upperLimit.ToString(CultureInfo.InvariantCulture);
        currentText.text = 0f.ToString(CultureInfo.InvariantCulture);
    }

    // Update is called once per frame
    public void LateUpdate()
    {
        currentText.text = _articulationBody.xDrive.target.ToString("F0");
    }
}
