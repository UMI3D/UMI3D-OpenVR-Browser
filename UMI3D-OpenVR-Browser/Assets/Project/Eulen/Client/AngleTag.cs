using UnityEngine;
using UnityEngine.UI;

public class AngleTag : MonoBehaviour
{
    [SerializeField]
    private Text label;

    [SerializeField]
    private Camera cam;

    private void Start()
    {
        Debug.Assert(cam != null);
    }

    public void UpdateTag(AngleGizmo gizmo)
    {
        if (gizmo.Enabled)
        {
            transform.position = gizmo.center;
            label.text = gizmo.angle.ToString("F0") + " °";

            transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
        }

        gameObject.SetActive(gizmo.Enabled);
    }
}
