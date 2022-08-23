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

using umi3d.cdk.interaction;

namespace umi3dBrowsers.interaction.selection
{
    /// <summary>
    /// Information of selection tagging interface
    /// </summary>
    public abstract class AbstractSelectionData
    {
        /// <summary>
        /// Controller source of detection
        /// </summary>
        public AbstractController controller;

        /// <summary>
        /// Set to true when the object is the one chosen to be projected on the controller
        /// </summary>
        public bool hasBeenSelected;
    }
}