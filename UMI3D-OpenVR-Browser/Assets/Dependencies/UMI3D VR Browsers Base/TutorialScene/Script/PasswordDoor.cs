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
using umi3dVRBrowsersBase.tutorial.fakeServer;
using UnityEngine;

namespace umi3dVRBrowsersBase.tutorial
{
    /// <summary>
    /// Makes an obejct as a door protected with a password.
    /// </summary>
    public class PasswordDoor : MonoBehaviour
    {
        #region Fields

        [SerializeField]
        [Tooltip("Password to open the door. By default equals to Pomme, french translation of apple, the best fruit in the whole world")]
        private string password = "Pomme";

        [SerializeField]
        [Tooltip("Parameter which enables users to enter a password.")]
        private FakeStringParameter stringParameter;

        /// <summary>
        /// Is the door open ?
        /// </summary>
        private bool isOpened = false;

        /// <summary>
        /// Local coordinate
        /// </summary>
        [SerializeField]
        private Vector3 targetPosition = new Vector3(-29.592f, -1.28f, -29.592f);

        #endregion

        #region Methods

        /// <summary>
        /// Called each time the password set changes.
        /// </summary>
        /// <param name="pass"></param>
        public void OnPasswordChanged(string pass)
        {
            pass = stringParameter.GetValue() as string;

            if (!isOpened)
            {
                if (pass == password)
                    StartCoroutine(OpenDoor());
            }
        }

        /// <summary>
        /// Performs an animation to open the door.
        /// </summary>
        /// <returns></returns>
        private IEnumerator OpenDoor()
        {
            isOpened = true;

            while (Vector3.Distance(transform.localPosition, targetPosition) > .05)
            {
                transform.Translate((targetPosition - transform.localPosition) * Time.deltaTime * 5, Space.Self);
                yield return null;
            }
        }

        #endregion
    }
}