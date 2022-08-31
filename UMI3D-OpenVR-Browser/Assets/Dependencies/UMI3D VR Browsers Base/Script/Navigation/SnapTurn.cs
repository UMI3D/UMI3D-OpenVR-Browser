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

using umi3dVRBrowsersBase.interactions;
using UnityEngine;
using UnityEngine.Events;

namespace umi3dVRBrowsersBase.navigation
{
    public class SnapTurn : MonoBehaviour
    {
        #region Fields

        [SerializeField]
        private float snapTurnAngle = 45;

        [SerializeField]
        UnityEvent onSnapTurnStart = new UnityEvent();

        #endregion

        #region Methods

        private void Update()
        {
            if (AbstractControllerInputManager.Instance.GetLeftSnapTurn(interactions.ControllerType.LeftHandController)
                || AbstractControllerInputManager.Instance.GetLeftSnapTurn(interactions.ControllerType.RightHandController))
            {
                transform.Rotate(0, -snapTurnAngle, 0);
                onSnapTurnStart?.Invoke();
            }
            else if (AbstractControllerInputManager.Instance.GetRightSnapTurn(interactions.ControllerType.LeftHandController)
                || AbstractControllerInputManager.Instance.GetRightSnapTurn(interactions.ControllerType.RightHandController))
            {
                transform.Rotate(0, snapTurnAngle, 0);
                onSnapTurnStart?.Invoke();
            }
        }

        #endregion
    }
}