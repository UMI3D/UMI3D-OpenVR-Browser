/*
Copyright 2019 - 2021 Inetum
Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
    http://www.apache.org/licenses/LICENSE-2.0
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Sets up the avatar when users set their height and manages its display.
/// </summary>
public class SetUpAvatarHeight : MonoBehaviour
{
    public Transform anchor;

    public Transform skeletonContainer;

    public Transform rightController;
    public Transform leftController;

    public InverseKinematics rightInverseKinematics;
    public InverseKinematics leftInverseKinematics;

    /// <summary>
    /// Offset between anchor and the real neck position
    /// </summary>
    public Vector3 neckOffset;

    /// <summary>
    /// Has user set his height.
    /// </summary>
    static bool isSetup = false;

    public Transform neckPivot;

    /// <summary>
    /// Factor to smooth body rotation.
    /// </summary>
    public float smoothRotationSpeed = .1f;

    /// <summary>
    /// If users turn their heads more than this angle, the reset of fthe body will turn too.
    /// </summary>
    public float maxAngleBeforeRotating = 50;

    static Vector3 sessionScaleFactor = default;

    public Transform headBone;

    /// <summary>
    /// Asks users for their height.
    /// </summary>
    void Start()
    {
        if (!isSetup)
        {
            DialogBox.Instance.Display("Set up height", "Please, stand up and press ok when you are ready.", "OK", SetUpAvatar);
        } else
        {
            SetUpAvatar();
        }

    }

    void SetUpAvatar()
    {
        if (sessionScaleFactor == default)
            sessionScaleFactor = Vector3.one * anchor.localPosition.y * 1.064f;

        skeletonContainer.localScale = sessionScaleFactor;
        headBone.localScale = sessionScaleFactor;
        rightInverseKinematics.target = rightController;
        leftInverseKinematics.target = leftController;

        neckOffset = new Vector3(0, -0.066f * anchor.localPosition.y, -0.07f);
        isSetup = true;
    }

    /// <summary>
    /// Sets the position and rotation of the avatar according to users movments.
    /// </summary>
    void Update()
    {
        if (isSetup)
        {
            Vector3 anchorForwardProjected = Vector3.ProjectOnPlane(anchor.forward, Vector3.up);

            float diffAngle = Vector3.Angle(Vector3.ProjectOnPlane(anchor.forward, Vector3.up), this.transform.forward);
            /*if (diffAngle > maxAngleBeforeRotating)
            {
                Debug.Log("<color=cyan>JE ME LANCE </color>");
                StartCoroutine(ResetCoroutine());
            }*/
            this.transform.localRotation = Quaternion.Euler(transform.localEulerAngles.x, anchor.localEulerAngles.y, transform.localEulerAngles.z);
            transform.position = anchor.TransformPoint(neckOffset);
            neckPivot.rotation = anchor.rotation;
        }
    }

    /// <summary>
    /// Smooth rotation of the avatar.
    /// </summary>
    /// <returns></returns>
    IEnumerator ResetCoroutine()
    {
        Quaternion targetRotation = Quaternion.Euler(transform.localEulerAngles.x, anchor.localEulerAngles.y, transform.localEulerAngles.z);
        while (Quaternion.Angle(transform.localRotation, targetRotation) > 5)
        {
            var smoothRot = Quaternion.Lerp(transform.localRotation, targetRotation, smoothRotationSpeed);
            this.transform.localRotation = smoothRot;
            yield return null;
        }
    }
}
