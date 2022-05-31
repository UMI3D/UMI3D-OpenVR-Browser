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

using umi3d.common.interaction;
using UnityEngine;

namespace umi3dVRBrowsersBase.tutorial.fakeServer
{
    /// <summary>
    /// This class is a local imitatation of an UMI3D Interactable.
    /// </summary>
    public abstract class AbstractFakeInteraction : MonoBehaviour
    {
        /// <summary>
        /// Name of the tool.
        /// </summary>
        public string displayName;

        /// <summary>
        /// Id of the tool.
        /// </summary>
        public ulong Id { get; set; }

        private void Awake()
        {
            this.Id = (FakeEnvironmentLoader.Instance as FakeEnvironmentLoader).GenerateUniqueId();
            (FakeEnvironmentLoader.Instance as FakeEnvironmentLoader).RegisterInteraction(this);
        }

        /// <summary>
        /// Returns the dto which describes the interaction.
        /// </summary>
        /// <returns></returns>
        public abstract AbstractInteractionDto GetDto();
    }

}

