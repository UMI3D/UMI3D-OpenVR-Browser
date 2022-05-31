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

using System.Collections;
using System.Collections.Generic;
using umi3d.cdk.collaboration;
using UnityEngine;

namespace umi3dVRBrowsersBase.connection
{
    /// <summary>
    /// Component helper to enbale <see cref="MonoBehaviour"/> only once <see cref="UMI3DCollaborationClientServer"/> exists.
    /// </summary>
    public class WaitForServer : MonoBehaviour
    {
        /// <summary>
        /// <see cref="MonoBehaviour"/> to enable.
        /// </summary>
        public List<MonoBehaviour> components;

        private void Start()
        {
            StartCoroutine(WaitUntilReady());
        }

        /// <summary>
        /// Waits for <see cref="UMI3DCollaborationClientServer"/> existence to enable <see cref="components"/>.
        /// </summary>
        /// <returns></returns>
        private IEnumerator WaitUntilReady()
        {
            yield return new WaitUntil(() => UMI3DCollaborationClientServer.Exists);

            foreach (MonoBehaviour c in components)
                c.enabled = true;
        }
    }
}

