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
using umi3d.cdk;

namespace umi3dVRBrowsersBase.tutorial.fakeServer
{
    /// <summary>
    /// This class is a local imitation of UMI3D EnvironmentLoader.
    /// </summary>
    public class FakeEnvironmentLoader : UMI3DEnvironmentLoader
    {
        #region Fields

        /// <summary>
        /// Start id.
        /// </summary>
        private ulong id = 4000;

        /// <summary>
        /// List of all events of interactions of the scene.
        /// </summary>
        private Dictionary<ulong, AbstractFakeInteraction> events = new Dictionary<ulong, AbstractFakeInteraction>();

        #endregion

        #region Methods
        /// <summary>
        /// Generates a unique id for any local entity.
        /// </summary>
        /// <returns></returns>
        public ulong GenerateUniqueId()
        {
            return id++;
        }

        /// <summary>
        /// Registers a <see cref="AbstractFakeInteraction"/> for this manager.
        /// </summary>
        /// <param name="ev"></param>
        public void RegisterInteraction(AbstractFakeInteraction ev)
        {
            if (!events.ContainsKey(ev.Id))
                events.Add(ev.Id, ev);
        }

        /// <summary>
        /// Unregisters a <see cref="AbstractFakeInteraction"/> for this manager.
        /// </summary>
        /// <param name="id"></param>
        public AbstractFakeInteraction GetInteraction(ulong id)
        {
            if (events.ContainsKey(id))
                return events[id];
            else
                return null;
        }

        #endregion
    }
}