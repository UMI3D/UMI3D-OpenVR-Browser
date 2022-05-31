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
using UnityEngine.Events;

namespace umi3dVRBrowsersBase.tutorial.fakeServer
{
    /// <summary>
    /// This class is a local imitation of an UMI3D Event.
    /// </summary>
    public class FakeEvent : AbstractFakeInteraction
    {
        /// <summary>
        /// Event trigger when <see cref="isHold"/> is equal to false and a user interacts with this event.
        /// </summary>
        public UnityEvent onTrigger;

        /// <summary>
        /// Event trigger when <see cref="isHold"/> is equal to true and a user starts interacting with this event.
        /// </summary>
        public UnityEvent onHold;

        /// <summary>
        ///  Event trigger when <see cref="isHold"/> is equal to true and a user stops interacting with this event.
        /// </summary>
        public UnityEvent onRelease;

        /// <summary>
        /// Is a hold event .
        /// </summary>
        public bool isHold = false;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override AbstractInteractionDto GetDto()
        {
            return new EventDto
            {
                id = Id,
                name = displayName,
                hold = isHold
            };
        }
    }
}