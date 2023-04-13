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
    public interface IFollowable
    {
        /// <summary>
        /// The speed of this to follow the target.
        /// </summary>
        float SmoothSpeed { get; set; }
        /// <summary>
        /// The offset between this and the target.
        /// </summary>
        Vector3 Offset { get; set; }

        /// <summary>
        /// Make this follows <paramref name="target"/>.
        /// </summary>
        /// <remarks>Should be call in LateUpdate.</remarks>
        /// <param name="target"></param>
        void Follow(Vector3 target)
        {
            if (this is not MonoBehaviour mono) return;

            Vector3 desirePosition = target + Offset;
            mono.transform.position = Vector3.Lerp(mono.transform.position, desirePosition, SmoothSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Make this follows and looks at <paramref name="target"/>.
        /// </summary>
        /// <remarks>Should be call in LateUpdate.</remarks>
        /// <param name="target"></param>
        void FollowAndLookAt(Vector3 target)
        {
            if (this is not MonoBehaviour mono) return;
            Follow(target);
            mono.transform.LookAt(target);
        }
    }
}
