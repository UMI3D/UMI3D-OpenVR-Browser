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

using System.Collections.Generic;
using System.Linq;
using umi3d.cdk;
using umi3d.cdk.interaction;
using umi3d.common.interaction;
using UnityEngine;

namespace umi3dVRBrowsersBase.tutorial.fakeServer
{
    /// <summary>
    /// Local imitation of an UMI3D InteractableContainer.
    /// </summary>
    public class FakeInteractableContainer : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Name of associated tool")]
        private string displayName;

        [SerializeField]
        [Tooltip("List of all interactions stored by associated tool")]
        private List<AbstractFakeInteraction> events = new List<AbstractFakeInteraction>();

        private void Start()
        {
            var interactions = new List<AbstractInteractionDto>();
            //ev might not be init properly. Might need to register it to the environment.
            foreach (AbstractFakeInteraction ev in events)
                interactions.Add(ev.GetDto());

            var dto = new InteractableDto
            {
                id = (FakeEnvironmentLoader.Instance as FakeEnvironmentLoader).GenerateUniqueId(),
                name = displayName,
                interactions = interactions.Select(dto => dto.id).ToList()
            };

            var interactable = new Interactable(dto);
            InteractableContainer container = gameObject.AddComponent<InteractableContainer>();
            container.Interactable = interactable;

            UMI3DEnvironmentLoader.RegisterEntityInstance(dto.id, dto, interactable, interactable.Destroy).NotifyLoaded();
        }
    }
}