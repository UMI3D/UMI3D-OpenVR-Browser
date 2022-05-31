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

using System;
using System.Collections.Generic;
using umi3dVRBrowsersBase.interactions;
using umi3dVRBrowsersBase.interactions.input;
using UnityEngine;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.ui.playerMenu
{
    /// <summary>
    /// Manages the UI to enable users to modify their action bindings.
    /// </summary>
    public class ActionBindingMenu : MonoBehaviour
    {
        #region Fields

        [Header("Controller inputs")]
        [SerializeField]
        [Tooltip("Sprite to describe controller inputs")]
        private Sprite controllerSprite;

        [SerializeField]
        [Tooltip("Image used to show users input numbers on the controller")]
        private Image controllerImage;

        [SerializeField]
        [Tooltip("List of all inputs and their local position in controller Image")]
        private List<InputPosition> inputs = new List<InputPosition>();

        [SerializeField]
        [Tooltip("Font used to display the number associated to each controller input")]
        private Font font;

        [Header("Scroll view")]
        [SerializeField]
        [Tooltip("Scrollview which displays all actions (not parameters) of the current projected tool")]
        private ScrollRect actionScrollView;

        [SerializeField]
        [Tooltip("Content element of actionScrollView")]
        private Transform actionScrollViewContent;

        [SerializeField]
        [Tooltip("Button to scroll up")]
        private Button scrollUpButton;

        [SerializeField]
        [Tooltip("Button to scroll down")]
        private Button scrollDownButton;

        [SerializeField]
        [Tooltip("Prefab used to display a tool action")]
        private GameObject actionEntry;

        /// <summary>
        /// List of all element instanciated to display an action and its associated binding.
        /// </summary>
        private Dictionary<ControllerType, List<GameObject>> displayers = new Dictionary<ControllerType, List<GameObject>>();

        /// <summary>
        /// Is this menu displayed ?
        /// </summary>
        public bool IsOpen { get; private set; }

        #endregion

        #region Methods

        private void Awake()
        {
            RectTransform scrollView = actionScrollView.GetComponent<RectTransform>();
            RectTransform entryRect = actionEntry.GetComponent<RectTransform>();
            Debug.Assert(scrollView != null && entryRect != null);

            float increment = entryRect.rect.height / scrollView.rect.height;

            scrollUpButton.onClick.AddListener(() =>
            {
                actionScrollView.verticalNormalizedPosition = Mathf.Clamp(actionScrollView.verticalNormalizedPosition + increment, 0, 1);
            });

            scrollDownButton.onClick.AddListener(() =>
            {
                actionScrollView.verticalNormalizedPosition = Mathf.Clamp(actionScrollView.verticalNormalizedPosition - increment, 0, 1);
            });

            SetControllerDisplayer();
        }

        /// <summary>
        /// Inits the different fields.
        /// </summary>
        private void InitFields()
        {
            if (displayers.Count == 0)
                foreach (ControllerType type in Enum.GetValues(typeof(ControllerType)))
                {
                    displayers.Add(type, new List<GameObject>());
                }
        }

        /// <summary>
        /// Displays indications to show users which number is associated to each controller input.
        /// </summary>
        private void SetControllerDisplayer()
        {
            Debug.Assert(controllerSprite != null, "Add an image to show users where are their inputs on controllers");

            foreach (InputPosition i in inputs)
            {
                var go = new GameObject("Input-" + i.nb.ToString());
                go.transform.SetParent(this.controllerImage.transform);
                go.transform.localScale = Vector3.one;

                Text txt = go.AddComponent<Text>();
                txt.fontSize = 20;
                txt.font = font;
                txt.text = i.nb.ToString();
                txt.transform.localPosition = i.position;
            }
        }

        /// <summary>
        /// Clears all <see cref="ActionBindingEntry"/> added previously for a given <paramref name="controller"/>;
        /// </summary>
        public void ClearList(ControllerType controller)
        {
            if (displayers.Count == 0) InitFields();

            ClearScrollView(controller, true);

            displayers[controller].Clear();

            IsOpen = false;
        }

        /// <summary>
        /// Adds a new <see cref="ActionBindingEntry"/> to represent <paramref name="input"/>.
        /// </summary>
        /// <param name="input"></param>
        public void AddToList(AbstractVRInput input)
        {
            if (displayers.Count == 0) InitFields();

            GameObject go = Instantiate(actionEntry, actionScrollViewContent.transform);
            go.SetActive(true);
            ActionBindingEntry bindingEntry = go.GetComponent<ActionBindingEntry>();
            Debug.Assert(bindingEntry != null);
            bindingEntry.Init(input);
            go.SetActive(false);

            displayers[(input.controller as VRController).type].Add(go);
        }

        /// <summary>
        /// Displays all current binding for a given <paramref name="controller"/>.
        /// </summary>
        /// <param name="type"></param>
        public void DisplayCurrentBinding(ControllerType controller)
        {
            IsOpen = true;

            ClearScrollView(controller, false);

            foreach (GameObject entry in displayers[controller])
            {
                entry.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Destroys all children of the scrollview.
        /// </summary>
        private void ClearScrollView(ControllerType controller, bool destroy)
        {
            if (displayers.Count == 0) InitFields();

            GameObject tmp;
            for (int i = 0; i < displayers[controller].Count; i++)
            {
                tmp = displayers[controller][i].gameObject;

                if (destroy)
                    Destroy(tmp);
                else if (tmp.activeInHierarchy)
                    tmp.SetActive(false);
            }
        }

        #endregion
    }

    /// <summary>
    /// Associates a number and its display position.
    /// </summary>
    [Serializable]
    public class InputPosition
    {
        public int nb;

        public Vector2 position;
    }
}