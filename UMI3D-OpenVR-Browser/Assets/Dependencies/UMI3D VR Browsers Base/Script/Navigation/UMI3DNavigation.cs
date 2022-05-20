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

namespace umi3dVRBrowsersBase.navigation
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public class UMI3DNavigation : AbstractNavigation
    {
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
        public override void Teleport(TeleportDto data)
        {
            this.transform.position = data.position;
            this.transform.rotation = data.rotation;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="data"></param>
        public override void Navigate(NavigateDto data)
        {
            Teleport(new TeleportDto() { position = data.position, rotation = this.transform.rotation });
        }
    }
}