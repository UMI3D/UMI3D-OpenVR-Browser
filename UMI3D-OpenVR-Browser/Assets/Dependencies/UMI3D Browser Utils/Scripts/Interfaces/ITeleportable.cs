/*
Copyright 2019 - 2023 Inetum

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
using UnityEngine;

namespace umi3d.browser.utils
{
    public interface ITeleportable
    {
        /// <summary>
        /// Whether or not this allows to be teleported.
        /// </summary>
        bool IsTeleportationAllow { get; set; }

        /// <summary>
        /// Teleport this to <paramref name="target"/>.
        /// </summary>
        /// <param name="target"></param>
        void Teleport(Vector3 target)
        {
            if (this is not MonoBehaviour mono) return;

            mono.gameObject.transform.position = target;
        }

        /// <summary>
        /// Teleport this to <paramref name="target"/> and add an <paramref name="offset"/>.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="offset"></param>
        void TeleportWithOffset(Vector3 target, Vector3 offset)
        {
            if (this is not MonoBehaviour mono) return;

            mono.gameObject.transform.position = target + offset;
        }
    } 
}
