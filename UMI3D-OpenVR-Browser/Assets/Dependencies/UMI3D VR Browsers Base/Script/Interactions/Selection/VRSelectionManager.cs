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
        /// Controller to manage selection for
        /// </summary>
        [Header("Controller")]
        public VRController controller;

        [Header("Selectors")]
        public InteractableVRSelector interactableSelector;

        public SelectableVRSelector selectableSelector;
        public ElementVRSelector elementSelector;

        [Header("Cursor")]
        public AbstractCursor pointingCursor;

        public AbstractCursor grabCursor;

        /// <summary>
        /// Last selected object info <br/>
        /// Null when no object
        /// </summary>
        public SelectionIntentData LastSelectedInfo;

        /// <summary>
        /// Last intent selector used
        /// </summary>
        private IIntentSelector LastSelectorUsed;

        public void Update()
        {
            // Retrieves propositions from selectors
            var possibleSelec = new List<SelectionIntentData>();
            if (interactableSelector.activated)
                possibleSelec.AddRange(interactableSelector.GetIntentDetections());

            if (selectableSelector.activated)
                possibleSelec.AddRange(selectableSelector.GetIntentDetections());

            if (elementSelector.activated)
                possibleSelec.AddRange(elementSelector.GetIntentDetections());

            if (possibleSelec.Count == 0) //no selection intent detected
            {
                if (LastSelectorUsed != null
                    && LastSelectorUsed.IsSelecting()
                    && LastSelectedInfo != null)
                {
                    LastSelectorUsed.Select(null); //make the selector remember it cannot select something this time
                    LastSelectedInfo = null;
                    LastSelectorUsed = null;
                }
            }
            else if (TrySelect(possibleSelec, DetectionOrigin.PROXIMITY))
                return;
            else if (TrySelect(possibleSelec, DetectionOrigin.POINTING))
                return;
        }

        /// <summary>
        /// Give the order to the selector to select the best proposition if possible <br/>
        /// </summary>
        /// <param name="possibleSelec"></param>
        /// <param name="origin"></param>
        /// <returns>True if a selection has been attempted</returns>
        private bool TrySelect(List<SelectionIntentData> possibleSelec, DetectionOrigin origin)
        {
            var possibleSelecPointed = possibleSelec.Where(x => x.detectionOrigin == origin);
            if (possibleSelecPointed.Any())
            {
                if (possibleSelecPointed.Count() == 1)
                    StartSelection(possibleSelecPointed.First());
                else
                {
                    possibleSelecPointed.ToList().Sort(Compare);
                    StartSelection(possibleSelecPointed.Last());
                }
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
            IIntentSelector appropriateSelector;
            // Disambiguation already occured

            if (preferedObjectData is SelectionIntentData<Selectable>)
                appropriateSelector = selectableSelector;
            else if (preferedObjectData is SelectionIntentData<AbstractClientInteractableElement>)
                appropriateSelector = elementSelector;
            else if (preferedObjectData is SelectionIntentData<InteractableContainer>)
                appropriateSelector = interactableSelector;
            else
                throw new System.Exception("Unrecognized selectable object. No selector is available.");

            // selector switching, deselection is normally handled when the selector remains the sam
            if (LastSelectedInfo != null && LastSelectorUsed != appropriateSelector)
                LastSelectorUsed?.Select(null); //make the selector remember it cannot select something this time

            appropriateSelector.Select(preferedObjectData);
            LastSelectorUsed = appropriateSelector;
            LastSelectedInfo = preferedObjectData;
        }

        /// <summary>
        /// Compare selection data between them from a same detection origin.<br/>
        /// Preference: Client Element > Selectable > Interactable
        /// </summary>
        /// <param name="data1"></param>
        /// <param name="data2"></param>
        /// <returns>1 if data1 if more important than data2, -1 otherwise. data1 is always favored in the equal case.</returns>
        protected int Compare(SelectionIntentData data1, SelectionIntentData data2)
        {
            //! This method could be improved by comparing the distances, but it would assume the detector used
            if (data1 is SelectionIntentData<AbstractClientInteractableElement>)
                return 1;
            else if (data2 is SelectionIntentData<AbstractClientInteractableElement>) //if data1 is not c_element
                return -1;
            else if (data1 is SelectionIntentData<Selectable>)
                return 1;
            else if (data2 is SelectionIntentData<Selectable>)
                return -1;
            else if (data1 is SelectionIntentData<InteractableContainer>)
                return 1;
            else if (data2 is SelectionIntentData<InteractableContainer>)
                return -1;
            return 0;
        }
    }
}