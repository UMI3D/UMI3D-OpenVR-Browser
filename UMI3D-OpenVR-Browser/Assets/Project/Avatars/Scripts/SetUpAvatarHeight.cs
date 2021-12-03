using System.Collections;
using UnityEngine;

/// <summary>
/// Sets up the avatar when users set their height and manages its display.
/// </summary>
public class SetUpAvatarHeight : MonoBehaviour
{
    public Transform VRAnchor;

    public Transform skeletonContainer;

    public FootTargetBehavior FootTargetBehavior;
    public IKControl IKControl;

    //public Transform rightController;
    //public Transform leftController;

    //public InverseKinematics rightInverseKinematics;
    //public InverseKinematics leftInverseKinematics;

    /// <summary>
    /// Offset between anchor and the real neck position
    /// </summary>
    Vector3 neckOffset;

    /// <summary>
    /// Avatar height stored if a player leave an environement to connect to another.
    /// </summary>
    static float avatarHeight = -1;

    //public Transform NeckPivot;
    public Transform Neck;

    /// <summary>
    /// Factor to smooth body rotation.
    /// </summary>
    public float smoothRotationSpeed = .1f;

    /// <summary>
    /// If users turn their heads more than this angle, the reset of fthe body will turn too.
    /// </summary>
    public float maxAngleBeforeRotating = 50;

    static Vector3 sessionScaleFactor = default;

    private void Start()
    {
        if (AvatarHeightPanel.isSetup)
            StartCoroutine(SetUpAvatar());
    }

    bool isSetup = false;

    Vector3 startingVirtualNeckPosition;
    float diffY;

    /// <summary>
    /// Check user's height to change avatar size.
    /// </summary>
    public IEnumerator SetUpAvatar()
    {
        float height;

        if (AvatarHeightPanel.isSetup)
        {
            height = avatarHeight;

            while (VRAnchor.localPosition.y == 0)
                yield return null;
        }
        else
        {
            height = VRAnchor.localPosition.y;
            avatarHeight = height;
        }

        if (sessionScaleFactor == default)
            sessionScaleFactor = Vector3.one * height * 1.05f;

        skeletonContainer.localScale = sessionScaleFactor;
        //rightInverseKinematics.target = rightController;
        //leftInverseKinematics.target = leftController;

        neckOffset = new Vector3(0, -0.060f * VRAnchor.localPosition.y, -0.07f);

        startingVirtualNeckPosition = VRAnchor.TransformPoint(neckOffset);
        diffY = startingVirtualNeckPosition.y - skeletonContainer.position.y;

        // IKControl.headIkActive = true;
        IKControl.controllerIkActive = true;

        FootTargetBehavior.SetFootTargets();

        isSetup = true;
    }

    private void Update()
    {
        //Debug.Log(OVRAnchor.localRotation.y);
        //Debug.Log(OVRAnchor.rotation.y);
    }

    /// <summary>
    /// Sets the position and rotation of the avatar according to users movments.
    /// </summary>
    void LateUpdate()
    {
        if (isSetup)
        {
            //Vector3 anchorForwardProjected = Vector3.ProjectOnPlane(transform.worldToLocalMatrix.MultiplyVector(OVRAnchor.forward), Vector3.up);

            float diffAngle = Vector3.Angle(Vector3.ProjectOnPlane(VRAnchor.forward, Vector3.up), this.transform.forward);
            /*if (diffAngle > maxAngleBeforeRotating)
            {
                Debug.Log("<color=cyan>JE ME LANCE </color>");
                StartCoroutine(ResetCoroutine());
            }*/


            float rotX = VRAnchor.localRotation.eulerAngles.x > 180 ? VRAnchor.localRotation.eulerAngles.x - 360 : VRAnchor.localRotation.eulerAngles.x;

            Neck.localRotation = Quaternion.Euler(Mathf.Clamp(rotX, -60, 60), 0, 0);

            Vector3 virtualNeckPosition = VRAnchor.TransformPoint(neckOffset);

            transform.position = new Vector3(virtualNeckPosition.x, 0, virtualNeckPosition.z);

            skeletonContainer.position = new Vector3(virtualNeckPosition.x, virtualNeckPosition.y - diffY, virtualNeckPosition.z);
            

            Vector3 anchorForwardProjected = Vector3.Cross(VRAnchor.right, Vector3.up).normalized;
            transform.rotation = Quaternion.LookRotation(anchorForwardProjected, Vector3.up);
        }
    }

    /// <summary>
    /// Smooth rotation of the avatar.
    /// </summary>
    /// <returns></returns>
    IEnumerator ResetCoroutine()
    {
        Quaternion targetRotation = Quaternion.Euler(transform.localEulerAngles.x, VRAnchor.localEulerAngles.y, transform.localEulerAngles.z);
        while (Quaternion.Angle(transform.localRotation, targetRotation) > 5)
        {
            var smoothRot = Quaternion.Lerp(transform.localRotation, targetRotation, smoothRotationSpeed);
            this.transform.localRotation = smoothRot;
            yield return null;
        }
    }
}
