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

using System.Threading.Tasks;
using umi3d.cdk;
using umi3d.common;
using umi3dVRBrowsersBase.ui.watchMenu;
using UnityEngine;

namespace umi3dVRBrowsersBase.ui.notification
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    [CreateAssetMenu(fileName = "VRNotificationLoader", menuName = "UMI3D/VR Notification Loader")]
    public class VRNotificationLoader : NotificationLoader
    {
        #region Fields

        /// <summary>
        /// Prefab for watch notification.
        /// </summary>
        public WatchNotification notificationPrefab;

        /// <summary>
        /// Prefab for world notification.
        /// </summary>
        public Notification3D notification3DPrefab;

        #endregion


        public override AbstractLoader GetNotificationLoader()
        {
            return new VrInternalNotificationLoader(this);
        }

        public AbstractNotification InstantiatePrefab(bool is3D)
        {
            if (is3D)
            {
                return Instantiate(notification3DPrefab);
            }
            else
            {
                return Instantiate(notificationPrefab);
            }
        }

    }

    public class VrInternalNotificationLoader : InternalNotificationLoader
    {
        public VRNotificationLoader loader;

        public VrInternalNotificationLoader(VRNotificationLoader loader)
        {
            this.loader = loader;
        }

        public override async Task ReadUMI3DExtension(ReadUMI3DExtensionData value)
        {
            await base.ReadUMI3DExtension(value);
            var dto = value.dto as NotificationDto;
            AbstractNotification notification;

            if (dto is NotificationOnObjectDto notificationOnObjectDto)
            {
                notification = loader.InstantiatePrefab(true);

                UMI3DNodeInstance obj = UMI3DEnvironmentLoader.GetNode(notificationOnObjectDto.objectId);

                notification.SetParent(obj?.gameObject.transform, Vector3.zero, Quaternion.identity);

                notification.Init(dto);
                UMI3DEnvironmentLoader.RegisterNodeInstance(dto.id, dto, notification.gameObject);
            }
            else
            {
                Debug.LogError("TODO : only display notification in one watch");
                foreach (WatchMenu watch in WatchMenu.instances)
                {
                    notification = loader.InstantiatePrefab(false);
                    notification.SetParent(watch.notificationContainer, Vector3.zero, Quaternion.identity);

                    notification.Init(dto);
                    UMI3DEnvironmentLoader.RegisterNodeInstance(dto.id, dto, notification.gameObject);
                }
            }
        }
    }

}
