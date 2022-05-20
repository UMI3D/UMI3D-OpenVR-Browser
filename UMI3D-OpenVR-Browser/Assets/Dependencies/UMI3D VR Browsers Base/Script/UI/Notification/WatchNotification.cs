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

using umi3d.common;
using umi3dVRBrowsersBase.interactions;

namespace umi3dVRBrowsersBase.ui.notification
{
    /// <summary>
    /// Class to display an UMI3D Notification in the <see cref="watchMenu.WatchMenu"/>.
    /// </summary>
    public class WatchNotification : AbstractNotification
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="dto"></param>
        public override void Init(NotificationDto dto)
        {
            base.Init(dto);

            UnityEngine.Debug.LogError("TODO : Choose correct hand");
            AbstractControllerInputManager.Instance.VibrateController(ControllerType.LeftHandController, .3f, .2f, .2f);
        }
    }
}


