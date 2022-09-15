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
using umi3dVRBrowsersBase.interactions.input;
using UnityEngine;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.ui.playerMenu
{
    /// <summary>
    /// Responsible of displaying an action of the current projected tool. Handled by <see cref="ActionBindingMenu"/>.
    /// </summary>
    public class ActionBindingEntry : MonoBehaviour
    {
        #region Fields

        [SerializeField]
        [Tooltip("Label which displays action's name")]
        private Text actionNameLabel;

        [SerializeField]
        [Tooltip("Label responsible to display the number of the associated controller input")]
        private Text actionBindingNumber;

        /// <summary>
        /// Element whoch enables user to perform a drag and drop to change the binding.
        /// </summary>
        [SerializeField]
        private ActionBindingEntryInput inputDisplayer;

        /// <summary>
        /// Associated input.
        /// </summary>
        [HideInInspector]
        public AbstractVRInput input;

        #endregion

        #region Methods

        /// <summary>
        /// Initiates the entry's fields.
        /// </summary>
        /// <param name="input"></param>
        public void Init(AbstractVRInput input)
        {
            this.input = input;
            SetBindingDisplay();
        }

        /// <summary>
        /// Changes <see cref="actionBinding.input"/> 's action to <see cref="this.input"/>. 
        /// </summary>
        /// <param name="actionBinding"></param>
        internal void SwitchInputTo(ActionBindingEntry actionBinding)
        {
            (VRInteractionMapper.Instance as VRInteractionMapper).GetController((actionBinding.input.controller as VRController).type).ChangeInputMapping(actionBinding.input, input);

            AbstractVRInput tmp = this.input;
            this.input = actionBinding.input;
            actionBinding.input = tmp;

            SetBindingDisplay();
            actionBinding.SetBindingDisplay();
        }

        /// <summary>
        /// Updates binding display when <see cref="input"/> is set.
        /// </summary>
        private void SetBindingDisplay()
        {
            PlayerMenuManager.StartCoroutine(SetBindingDisplayCoroutine());
        }

        /// <summary>
        /// Inits the display once were are sure the data is set up.
        /// </summary>
        /// <returns></returns>
        private IEnumerator SetBindingDisplayCoroutine()
        {
            yield return null;
            if (actionNameLabel == null || actionNameLabel.text == null) yield break;

            actionNameLabel.text = input.CurrentInteraction()?.name;

            if (input is BooleanInput boolInput)
            {
                actionBindingNumber.text = ((int)boolInput.vrInput.action).ToString();
                inputDisplayer.SetBackground(ActionBindingType.Button);
            }
            else if (input is ManipulationInput manipulationInput)
            {
                actionBindingNumber.text = ((int)manipulationInput.activationButton.action).ToString();
                inputDisplayer.SetBackground(ActionBindingType.Button);
            }
            else
            {
                actionBindingNumber.text = string.Empty;
                inputDisplayer.SetBackground(ActionBindingType.NotBound);
            }
        }

        #endregion
    }
}