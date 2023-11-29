/*
Copyright 2019 - 2022 Inetum

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

using inetum.unityUtils;
using System.Collections;
using System.Collections.Generic;
using umi3d.cdk;
using umi3d.cdk.collaboration;
using umi3d.cdk.userCapture.tracking;
using umi3d.common.userCapture;
using umi3dVRBrowsersBase.ikManagement;
using UnityEngine;

namespace umi3dVRBrowsersBase.connection
{
    /// <summary>
    /// Sets up the avatar when users set their height and manages its display.
    /// </summary>
    public class SetUpSkeleton : MonoBehaviour
    {
        #region Fields

        public TrackedSubskeleton trackedSkeleton;

        public List<Tracker> trackers = new List<Tracker>();

        public SkinnedMeshRenderer Joint, Surface;
        public TrackedSubskeletonBone Viewpoint;
        public GameObject LeftWatch, RightWatch;

        /// <summary>
        /// Root of player's camera.
        /// </summary>
        public Transform OVRAnchor;

        /// <summary>
        /// Avatar skeleton root.
        /// </summary>
        public Transform skeletonContainer;

        /// <summary>
        /// ?
        /// </summary>
        public FootTargetBehavior FootTargetBehavior;

        /// <summary>
        /// Offset between anchor and the real neck position
        /// </summary>
        private Vector3 neckOffset;

        /// <summary>
        /// Avatar height stored if a player leave an environement to connect to another.
        /// </summary>
        private static float avatarHeight = -1;

        /// <summary>
        /// Avatar's neck.
        /// </summary>
        public Transform Neck;

        /// <summary>
        /// Factor to smooth body rotation.
        /// </summary>
        public float smoothRotationSpeed = .1f;

        /// <summary>
        /// If users turn their heads more than this angle, the reset of fthe body will turn too.
        /// </summary>
        public float maxAngleBeforeRotating = 50;

        /// <summary>
        /// Avatar scale associated to users heights.
        /// </summary>
        private static Vector3 sessionScaleFactor = default;

        /// <summary>
        /// List of <see cref="GameObject"/> to activate when avatar's height is set up.
        /// </summary>
        public List<GameObject> objectsToActivate;

        /// <summary>
        /// Is avatar's height set up ?
        /// </summary>
        private bool isSetup = false;

        /// <summary>
        /// Computed neck position
        /// </summary>
        private Vector3 startingVirtualNeckPosition;

        /// <summary>
        /// Distance between <see cref="startingVirtualNeckPosition"/> and <see cref="skeletonContainer"/>.
        /// </summary>
        private float diffY;

        #endregion

        #region Methods

        private void Awake()
        {
            Joint.enabled = false;
            Surface.enabled = false;
            LeftWatch.SetActive(false);
            RightWatch.SetActive(false);
        }

        private void Start()
        {
            if (AvatarHeightPanel.isSetup)
                StartCoroutine(SetUpAvatar());

            UMI3DEnvironmentLoader.Instance.onEnvironmentLoaded.AddListener(() =>
            {
                Joint.enabled = false;
                Surface.enabled = false;
            });
        }

        /// <summary>
        /// Check user's height to change avatar size.
        /// </summary>
        public IEnumerator SetUpAvatar()
        {
            Joint.enabled = true;
            Surface.enabled = true;
            LeftWatch.SetActive(true);
            RightWatch.SetActive(true);

            float height;

            if (AvatarHeightPanel.isSetup)
            {
                height = avatarHeight;

                while (OVRAnchor.localPosition.y == 0)
                    yield return null;
            }
            else
            {
                height = OVRAnchor.localPosition.y;
                avatarHeight = height;
            }

            if (sessionScaleFactor == default)
            {
                sessionScaleFactor = Vector3.one * height * 1.10f;
            }

            skeletonContainer.localScale = sessionScaleFactor;

            neckOffset = new Vector3(0, -0.060f * OVRAnchor.localPosition.y, -0.07f);

            startingVirtualNeckPosition = OVRAnchor.TransformPoint(neckOffset);
            diffY = startingVirtualNeckPosition.y - skeletonContainer.position.y;

            FootTargetBehavior.SetFootTargets();

            foreach (GameObject obj in objectsToActivate)
                obj.SetActive(true);

            trackers.ForEach(x => trackedSkeleton.controllers.Add(x.distantController));
            //trackedSkeleton.bones.Add(BoneType.Viewpoint, Viewpoint);

            isSetup = true;
        }

        /// <summary>
        /// Sets the position and rotation of the avatar according to users movments.
        /// </summary>
        private void LateUpdate()
        {
            if (isSetup)
            {
                float diffAngle = Vector3.Angle(Vector3.ProjectOnPlane(OVRAnchor.forward, Vector3.up), this.transform.forward);

                float rotX = OVRAnchor.localRotation.eulerAngles.x > 180 ? OVRAnchor.localRotation.eulerAngles.x - 360 : OVRAnchor.localRotation.eulerAngles.x;

                Neck.localRotation = Quaternion.Euler(Mathf.Clamp(rotX, -60, 60), 0, 0);

                Vector3 virtualNeckPosition = OVRAnchor.TransformPoint(neckOffset);

                transform.position = new Vector3(virtualNeckPosition.x, virtualNeckPosition.y - diffY, virtualNeckPosition.z);

                skeletonContainer.position = new Vector3(virtualNeckPosition.x, virtualNeckPosition.y - diffY, virtualNeckPosition.z);

                Vector3 anchorForwardProjected = Vector3.Cross(OVRAnchor.right, Vector3.up).normalized;
                transform.rotation = Quaternion.LookRotation(anchorForwardProjected, Vector3.up);
            }
        }

        /// <summary>
        /// Smooth rotation of the avatar.
        /// </summary>
        /// <returns></returns>
        private IEnumerator ResetCoroutine()
        {
            var targetRotation = Quaternion.Euler(transform.localEulerAngles.x, OVRAnchor.localEulerAngles.y, transform.localEulerAngles.z);
            while (Quaternion.Angle(transform.localRotation, targetRotation) > 5)
            {
                var smoothRot = Quaternion.Lerp(transform.localRotation, targetRotation, smoothRotationSpeed);
                this.transform.localRotation = smoothRot;
                yield return null;
            }
        }

        #endregion
    }
}