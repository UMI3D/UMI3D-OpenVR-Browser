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

namespace umi3dVRBrowsersBase.ui.notification
{
    /// <summary>
    /// Class to represent an UMI3DNotification displayed by a 3D object.
    /// </summary>
    public class Notification3D : AbstractNotification
    {
        /// <summary>
        /// Set world position of node.
        /// </summary>
        /// <param name="position"></param>
        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }
    }
}