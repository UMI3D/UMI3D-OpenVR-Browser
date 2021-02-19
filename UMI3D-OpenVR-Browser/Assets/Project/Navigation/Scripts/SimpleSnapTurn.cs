/*
Copyright 2019 - 2021 Inetum
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

using Valve.VR;
using UnityEngine;
using System.Collections;

namespace BrowserQuest.Navigation
{
    /// <summary>
    /// Enables users to perform a snap rotation. For now only enabled on Oculus Rift.
    /// </summary>
    public class SimpleSnapTurn : MonoBehaviour
    {
        /// <summary>
        /// Rotation angle.
        /// </summary>
        public float snapAngle = 45.0f;

        /// <summary>
        /// Action used to rotate left.
        /// </summary>
        public SteamVR_Action_Boolean snapLeftAction = SteamVR_Input.GetBooleanAction("SnapTurnLeft");

        /// <summary>
        /// Action used to rotate right.
        /// </summary>
        public SteamVR_Action_Boolean snapRightAction = SteamVR_Input.GetBooleanAction("SnapTurnRight");

        /// <summary>
        /// Player's transform.
        /// </summary>
        public Transform player;

        /// <summary>
        /// Rotation speed. Increase this value for a quicker rotation.
        /// </summary>
        public float rotationSpeed = 10f;

        /// <summary>
        /// Is player rotating ?
        /// </summary>
        private bool isRotating = false;

        public PlayerMenuManager leftMenuManager;

        public PlayerMenuManager rightMenuManager;

        private void Update()
        {
            if (HeadsetManager.Instance.CurrentHeadSetType != HeadsetManager.HeadsetType.OculusRift)
                return;

            if (isRotating)
                return;

            if (!leftMenuManager.IsDisplaying)
            {
                if (snapLeftAction.GetStateDown(SteamVR_Input_Sources.LeftHand))
                {
                    StartCoroutine(RotationCoroutine(-snapAngle));
                } else if (snapRightAction.GetStateDown(SteamVR_Input_Sources.LeftHand))
                {
                    StartCoroutine(RotationCoroutine(snapAngle));
                }
            }
            if (!rightMenuManager.IsDisplaying)
            {
                if (snapLeftAction.GetStateDown(SteamVR_Input_Sources.RightHand))
                {
                    StartCoroutine(RotationCoroutine(-snapAngle));
                }
                else if (snapRightAction.GetStateDown(SteamVR_Input_Sources.RightHand))
                {
                    StartCoroutine(RotationCoroutine(snapAngle));
                }
            }
        }

        /// <summary>
        /// Performs a smooth rotation along player.up axis.
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        IEnumerator RotationCoroutine(float angle)
        {
            isRotating = true;

            Quaternion targetRotation = Quaternion.Euler(player.eulerAngles.x, player.eulerAngles.y + angle, player.eulerAngles.z);
            
            while (Quaternion.Angle(player.rotation, targetRotation) > .5f)
            {
                Quaternion smoothRotation = Quaternion.Lerp(player.rotation, targetRotation, rotationSpeed);
                player.rotation = smoothRotation;
                yield return null;
            }

            isRotating = false;
        }
    }
}