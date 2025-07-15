using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;

public class SliderHandleGrab : MonoBehaviour
{
    public Slider slider;
    public Transform handTransform;  // Sera assigné dynamiquement
    public float slideRange = 0.2f;  // Longueur de glisse max

    private Vector3 startLocalPos;
    private bool isGrabbed = false;

    void Start()
    {
        startLocalPos = transform.localPosition;

        var grab = GetComponent<XRGrabInteractable>();
        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        handTransform = args.interactorObject.transform;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
    }

    void Update()
    {
        if (!isGrabbed || handTransform == null)
            return;

        // Déplacement relatif de la main
        Vector3 localHandPos = transform.parent.InverseTransformPoint(handTransform.position);
        float delta = Mathf.Clamp(localHandPos.x - startLocalPos.x, -slideRange / 2f, slideRange / 2f);

        // Convertir en valeur de slider
        float t = (delta + (slideRange / 2f)) / slideRange;
        slider.value = Mathf.Lerp(slider.minValue, slider.maxValue, t);

        // Positionner le handle visuellement
        transform.localPosition = new Vector3(startLocalPos.x + delta, startLocalPos.y, startLocalPos.z);
    }
}
