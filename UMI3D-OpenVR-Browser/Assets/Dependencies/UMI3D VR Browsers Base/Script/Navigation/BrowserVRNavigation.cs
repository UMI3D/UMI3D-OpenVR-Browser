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


using umi3d.cdk;
using umi3d.common;
using Unity.Burst.Intrinsics;
using UnityEngine;

namespace umi3dVRBrowsersBase.navigation
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public class BrowserVRNavigation : umi3d.cdk.AbstractNavigation
    {
        [SerializeField]
        Transform cameraTransform;

        /// <summary>
        /// Is player active ?
        /// </summary>
        protected bool isActive = false;
        protected umi3d.cdk.UMI3DNodeInstance globalVehicle;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Activate() { }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Disable() { }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="data"></param>
        public override void Teleport(umi3d.common.TeleportDto data)
        {
            this.transform.position = data.position.Struct();
            if (cameraTransform != null)
            {
                transform.Translate(Vector3.ProjectOnPlane(transform.position - cameraTransform.position, Vector3.up), Space.World);
            }
            this.transform.rotation = data.rotation.Quaternion();

            if (cameraTransform != null)
            {
                float angle = Vector3.SignedAngle(transform.forward, Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up), Vector3.up);
                this.transform.Rotate(0, -angle, 0);
            }
        }

        protected bool vehicleFreeHead = false;
        protected UMI3DNodeInstance globalFrame;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="data"></param>
        public override void Navigate(umi3d.common.NavigateDto data)
        {
            Teleport(new umi3d.common.TeleportDto() { position = data.position, rotation = this.transform.rotation.Dto() });
        }

        public override void UpdateFrame(FrameRequestDto data)
        {
            //vehicleFreeHead = data.StopNavigation;

            if (data.FrameId == 0)
            {
                this.transform.SetParent(UMI3DLoadingHandler.Instance.transform, true);
                //this.transform.localPosition = data.position;
                //this.transform.localRotation = data.rotation;
                DontDestroyOnLoad(UMI3DNavigation.Instance);
                globalFrame.Delete -= GlobalFrameDeleted;
                globalFrame = null;
            }
            else
            {
                UMI3DNodeInstance Frame = UMI3DEnvironmentLoader.GetNode(data.FrameId);
                if (Frame != null)
                {
                    globalFrame = Frame;
                    this.transform.SetParent(Frame.transform, true);
                    //this.transform.localPosition = data.position;
                    //this.transform.localRotation = data.rotation;
                    globalFrame.Delete += GlobalFrameDeleted;
                }
            }
        }

        void GlobalFrameDeleted()
        {
            this.transform.SetParent(UMI3DLoadingHandler.Instance.transform, true);
            DontDestroyOnLoad(UMI3DNavigation.Instance);
        }
    }
}