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


using UnityEngine;

namespace umi3dVRBrowsersBase.navigation
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public class UMI3DNavigation : umi3d.cdk.AbstractNavigation
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
            this.transform.position = data.position;
            if (cameraTransform != null)
            {
                transform.Translate(Vector3.ProjectOnPlane(transform.position - cameraTransform.position, Vector3.up), Space.World);
            }
            this.transform.rotation = data.rotation;

            if (cameraTransform != null)
            {
                float angle = Vector3.SignedAngle(transform.forward, Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up), Vector3.up);
                this.transform.Rotate(0, -angle, 0);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="data"></param>
        public override void Navigate(umi3d.common.NavigateDto data)
        {
            Teleport(new umi3d.common.TeleportDto() { position = data.position, rotation = this.transform.rotation });
        }

        public override void Embark(umi3d.common.VehicleDto data)
        {
            isActive = !data.StopNavigation;

            if (data.VehicleId == 0)
            {
                this.transform.SetParent(umi3d.cdk.UMI3DEnvironmentLoader.Instance.transform, true);

                this.transform.localPosition = data.position;
                if (cameraTransform != null)
                    transform.Translate(Vector3.ProjectOnPlane(transform.position - cameraTransform.position, Vector3.up), Space.World);

                this.transform.localRotation = data.rotation;

                if (cameraTransform != null)
                {
                    float angle = Vector3.SignedAngle(transform.forward, Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up), Vector3.up);
                    this.transform.Rotate(0, -angle, 0);
                }

                DontDestroyOnLoad(umi3d.cdk.UMI3DNavigation.Instance);

                globalVehicle.Delete -= new System.Action(() => {
                    umi3d.cdk.UMI3DNavigation.Instance.transform.SetParent(umi3d.cdk.UMI3DEnvironmentLoader.Instance.transform, true);
                    DontDestroyOnLoad(umi3d.cdk.UMI3DNavigation.Instance);
                });

                globalVehicle = null;
            }
            else
            {
                umi3d.cdk.UMI3DNodeInstance vehicle = umi3d.cdk.UMI3DEnvironmentLoader.GetNode(data.VehicleId);

                if (vehicle != null)
                {
                    globalVehicle = vehicle;

                    this.transform.SetParent(vehicle.transform, true);
                    this.transform.localPosition = data.position;
                    if (cameraTransform != null)
                        transform.Translate(Vector3.ProjectOnPlane(transform.position - cameraTransform.position, Vector3.up), Space.World);

                    this.transform.localRotation = data.rotation;

                    if (cameraTransform != null)
                    {
                        float angle = Vector3.SignedAngle(transform.forward, Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up), Vector3.up);
                        this.transform.Rotate(0, -angle, 0);
                    }

                    globalVehicle.Delete += new System.Action(() => {
                        umi3d.cdk.UMI3DNavigation.Instance.transform.SetParent(umi3d.cdk.UMI3DEnvironmentLoader.Instance.transform, true);
                        DontDestroyOnLoad(umi3d.cdk.UMI3DNavigation.Instance);
                    });
                }
            }
        }
    }
}