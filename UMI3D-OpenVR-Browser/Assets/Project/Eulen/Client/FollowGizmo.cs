using UnityEngine;
using UnityEngine.UI;

public class FollowGizmo : MonoBehaviour
{
    [SerializeField] private GameObject gizmo;
    [SerializeField] private GameObject child;
    [SerializeField] private Text uIAngle;
    [SerializeField] private Vector3 offsetPosition;
    [SerializeField] private Vector3 offsetRotation;
    public bool syncRotation = true;

    /// <summary>
    /// Updates the position of the angle tag (next to their track) and their value
    /// </summary>
    /// <param name="cameraHead"></param>
    /// <param name="angulo"></param>
    public void UpdateTagPosition(Transform cameraHead, float angulo)
    {
        transform.position = gizmo.transform.position + offsetPosition;
        transform.rotation = Quaternion.Euler(gizmo.transform.localRotation.eulerAngles + offsetRotation);

        child.transform.LookAt(cameraHead.transform, Vector3.up);

        uIAngle.text = angulo.ToString("F0") + " °";
    }

    /// <summary>
    /// Updates the position of the angle tag (next to their track) and their value
    /// </summary>
    /// <param name="cameraHead"></param>
    /// <param name="angulo"></param>
    public void UpdateAuxPosition()
    {
        transform.position = gizmo.transform.position;
        if (syncRotation) transform.rotation = Quaternion.Euler(gizmo.transform.localRotation.eulerAngles + offsetRotation);
        else transform.rotation = Quaternion.Euler(0, gizmo.transform.localRotation.eulerAngles.y + 190, 0);

    }

    public Transform GetChild()
    {
        return child.transform;
    }
}
