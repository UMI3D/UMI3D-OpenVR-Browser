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
using umi3dVRBrowsersBase.interactions;
using UnityEngine;
namespace umi3dVRBrowsersBase.tutorial
{
    /// <summary>
    /// Turns an object into a grabbable door.
    /// </summary>
    public class GrabbleDoor : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// Current translation.
        /// </summary>
        private float position;

        /// <summary>
        /// Coroutine which detects the movment.
        /// </summary>
        private Coroutine coroutine;

        #endregion

        #region Methods

        /// <summary>
        /// Starts door manipulation.
        /// </summary>
        public void StartHolding()
        {
            coroutine = StartCoroutine(UpdateCoroutine());
        }

        /// <summary>
        /// Coroutine which detects users movment to open or close the door.
        /// </summary>
        /// <returns></returns>
        private IEnumerator UpdateCoroutine()
        {
            Transform hand = (VRInteractionMapper.Instance as VRInteractionMapper).GetControllerTransform(ControllerType.RightHandController);

            position = hand.position.x;

            yield return null;

            while (true)
            {
                float delta = hand.transform.position.x - position;

                transform.localPosition = new Vector3(Mathf.Clamp(transform.localPosition.x + delta, -1.5f, 0), transform.localPosition.y, transform.localPosition.z);

                position = hand.transform.position.x;
                yield return null;
            }
        }

        /// <summary>
        /// Stops door manipulation.
        /// </summary>
        public void StopHolding()
        {
            StopCoroutine(coroutine);
        }

        #endregion
    }
}