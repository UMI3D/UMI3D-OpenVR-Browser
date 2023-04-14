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
using System.Linq;
using umi3d.cdk.interaction;
using umi3dBrowsers.interaction.selection;
using umi3dBrowsers.interaction.selection.cursor;
using umi3dBrowsers.interaction.selection.selector;
using umi3dVRBrowsersBase.interactions.selection.selector;
using umi3dVRBrowsersBase.ui;
using UnityEngine;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.interactions.selection
{
    public class VRSelectionManager : MonoBehaviour
    {
        /// <summary>
        /// Controller to manage selection for.
        /// </summary>
        [Header("Controller"), Tooltip("Controller to manage selection for.")]
        public VRController controller;

        [Header("Selectors"), Tooltip("Selector for interactables.")]
        public InteractableVRSelector interactableSelector;

        [Tooltip("Selector for selectables.")]
        public SelectableVRSelector selectableSelector;

        [Tooltip("Selector for other customized UI.")]
        public ElementVRSelector elementSelector;

        /// <summary>
        /// Cursor associated with pointing selection.
        /// </summary>
        [Header("Cursor"), Tooltip("Cursor associated with pointing selection.")]
        public AbstractPointingCursor pointingCursor;

        /// <summary>
        /// Cursor associated with proximity selection.
        /// </summary>
        [Tooltip("Cursor associated with proximity selection.")]
        public AbstractCursor grabCursor;

        /// <summary>
        /// Last selected object info. <br/>
        /// Null when no object.
        /// </summary>
        public SelectionIntentData LastSelectedInfo;

        /// <summary>
        /// Last intent selector used.
        /// </summary>
        private IIntentSelector LastSelectorUsed;

        public void Update()
        {
            // check that no selector is in a locked state, i.e. an object is being manipulated
            if (interactableSelector.LockedSelector
                || selectableSelector.LockedSelector
                || elementSelector.LockedSelector)
                return;

            // Retrieves propositions from selectors
            var possibleSelec = new List<SelectionIntentData>();
            if (elementSelector.activated)
                possibleSelec.AddRange(elementSelector.GetIntentDetections());  // Preference: Client Element > Selectable > Interactable

            if (selectableSelector.activated)
                possibleSelec.AddRange(selectableSelector.GetIntentDetections());

            if (interactableSelector.activated)
                possibleSelec.AddRange(interactableSelector.GetIntentDetections());

            if (possibleSelec.Count == 0) //no selection intent detected
            {
                if (LastSelectorUsed != null //a selection has already occurred
                    && LastSelectorUsed.IsSelecting() //selection is currently carried on
                    && LastSelectedInfo != null) //selection was occurring next frame
                {
                    LastSelectorUsed.Select(null); //make the selector remember it cannot select something this time
                    LastSelectedInfo = null;
                    LastSelectorUsed = null;
                }
            }
            // selection of proposition
            else if (TrySelect(possibleSelec, DetectionOrigin.PROXIMITY)) // proximity prefered over pointing
                return;
            else if (TrySelect(possibleSelec, DetectionOrigin.POINTING))
                return;
        }

        /// <summary>
        /// Check if a possible selection is available through a detection <paramref name="origin"/> and select the first of this origin.
        /// </summary>
        /// <param name="possibleSelec"></param>
        /// <param name="origin"></param>
        /// <returns>True if a selection has been attempted.</returns>
        private bool TrySelect(List<SelectionIntentData> possibleSelec, DetectionOrigin origin)
        {
            var possibleSelecPointed = possibleSelec.Where(x => x.detectionOrigin == origin);
            if (possibleSelecPointed.Any())
            {
                StartSelection(possibleSelecPointed.First());
                return true;
            }
            return false;
        }

        /// <summary>
        /// Commands the selection with the appropriate selector
        /// </summary>
        /// <param name="preferedObjectData"></param>
        private void StartSelection(SelectionIntentData preferedObjectData)
        {
            // getting the right selector for selection
            IIntentSelector appropriateSelector;
            if (preferedObjectData is SelectionIntentData<Selectable>)
                appropriateSelector = selectableSelector;
            else if (preferedObjectData is SelectionIntentData<AbstractClientInteractableElement>)
                appropriateSelector = elementSelector;
            else if (preferedObjectData is SelectionIntentData<InteractableContainer>)
                appropriateSelector = interactableSelector;
            else
                throw new System.Exception("Unrecognized selectable object. No selector is available.");

            // selector switching between two kind of selectable objects, deselection is normally handled when the selector remains the same
            if (LastSelectedInfo != null && LastSelectorUsed != appropriateSelector)
                LastSelectorUsed?.Select(null); //make the selector remember it cannot select something this time

            // actual selection operation
            appropriateSelector.Select(preferedObjectData);
            LastSelectorUsed = appropriateSelector;
            LastSelectedInfo = preferedObjectData;
        }
    }
}