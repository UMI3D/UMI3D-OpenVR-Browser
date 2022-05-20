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
using UnityEngine;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.connection
{
    /// <summary>
    /// Displays items in a carousel.
    /// </summary>
    public class SliderDisplayer : MonoBehaviour
    {
        #region Fields

        [Tooltip("Number of element displayed at the same time")]
        public int nbOfElementsDisplayed = 4;

        [SerializeField]
        [Tooltip("Button to move right in the carousel")]
        private Button nextButton;

        [SerializeField]
        [Tooltip("Button to move left in the carousel")]
        private Button previousButton;

        [SerializeField]
        [Tooltip("Where element will be spawn")]
        private GameObject container;

        public GameObject Container { get => container; }

        [Tooltip("Not required, prefab which is be used to display an element.")]
        public GameObject baseElement;

        private List<GameObject> elements = new List<GameObject>();
        public int NbOfElements { get => elements.Count; }

        /// <summary>
        /// Index of the current left item visible.
        /// </summary>
        private int currentLeftElementDisplayed = 0;

        #endregion

        #region Methods

        private void Start()
        {
            nextButton.onClick.AddListener(MoveNext);
            previousButton.onClick.AddListener(MovePrevious);
        }

        /// <summary>
        /// Displays only items which must be displayed.
        /// </summary>
        private void UpdateDisplay()
        {
            for (int i = 0; i < elements.Count; i++)
            {
                elements[i].SetActive((i >= currentLeftElementDisplayed) && (i < currentLeftElementDisplayed + nbOfElementsDisplayed));
            }

            previousButton.gameObject.SetActive((currentLeftElementDisplayed > 0));
            nextButton.gameObject.SetActive((currentLeftElementDisplayed + nbOfElementsDisplayed) < elements.Count);
        }

        /// <summary>
        /// Moves right in the carousel.
        /// </summary>
        private void MoveNext()
        {
            if (currentLeftElementDisplayed + nbOfElementsDisplayed < elements.Count)
            {
                currentLeftElementDisplayed++;
                UpdateDisplay();
            }
        }

        /// <summary>
        /// Moves left in the carousel.
        /// </summary>
        private void MovePrevious()
        {
            if (currentLeftElementDisplayed > 0)
            {
                currentLeftElementDisplayed--;
                UpdateDisplay();
            }
        }

        /// <summary>
        /// Adds an element to the carousel.
        /// </summary>
        /// <param name="obj"></param>
        public void AddElement(GameObject obj)
        {
            elements.Add(obj);
            obj.transform.SetParent(container.transform);
            UpdateDisplay();
        }

        /// <summary>
        /// Removes an element from the carousel.
        /// </summary>
        /// <param name="obj"></param>
        public void RemoveElement(GameObject obj)
        {
            if (elements.Contains(obj))
            {
                elements.Remove(obj);
                Destroy(obj);
            }
            else
            {
                Debug.Log("Impossible to delete this element because it does not exit for the slider.");
            }
        }

        /// <summary>
        /// Clears all carousel content.
        /// </summary>
        public void Clear()
        {
            foreach (GameObject elt in elements)
                Destroy(elt);
            elements.Clear();
            UpdateDisplay();
        }

        #endregion
    }
}