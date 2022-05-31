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
using UnityEngine.Events;

namespace umi3dVRBrowsersBase.ui
{
    /// <summary>
    /// Makes a gameobject clickable by a user ray.
    /// </summary>
    public class DefaultClickableButton : MonoBehaviour, IClickableElement
    {
        #region Fields

        [SerializeField]
        [Tooltip("Event raised when this element is clicked")]
        private UnityEvent onClicked = new UnityEvent();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public UnityEvent OnClicked { get => onClicked; }

        [SerializeField]
        [Tooltip("Renderer of this object")]
        private Renderer btnRenderer;

        [SerializeField]
        [Tooltip("Default object material")]
        private Material defaultMaterial;

        [SerializeField]
        [Tooltip("Hover feedback material")]
        private Material hoverMaterial;

        [SerializeField]
        [Tooltip("Press feedback material")]
        private Material pressedMaterial;

        [SerializeField]
        [Tooltip("Select feedback material")]
        private Material selectMaterial;

        /// <summary>
        /// Is this element in selected mode ?
        /// </summary>
        public bool IsSelected { get; private set; } = false;

        /// <summary>
        /// Is this element currently hovered ?
        /// </summary>
        private bool isHovered = false;

        #endregion

        private void OnEnable()
        {

            if (btnRenderer != null && defaultMaterial != null)
            {
                btnRenderer.material = defaultMaterial;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="controller"></param>
        public void Click(ControllerType controller)
        {
            onClicked?.Invoke();

            if (btnRenderer != null && pressedMaterial != null)
            {
                StartCoroutine(SetPressFeedback());
            }
        }

        /// <summary>
        /// Displays press feedback during a certain time.
        /// </summary>
        /// <returns></returns>
        private IEnumerator SetPressFeedback()
        {
            btnRenderer.material = pressedMaterial;
            yield return new WaitForSeconds(.3f);

            if (IsSelected && selectMaterial != null)
            {

                btnRenderer.material = selectMaterial;
            }
            else if (isHovered && hoverMaterial != null)
            {

                btnRenderer.material = hoverMaterial;
            }
            else if (defaultMaterial != null)
            {

                btnRenderer.material = defaultMaterial;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void HoverEnter()
        {
            if (btnRenderer != null && !IsSelected && hoverMaterial != null)
            {
                btnRenderer.material = hoverMaterial;
            }
            isHovered = true;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void HoverExit()
        {
            if (btnRenderer != null && !IsSelected && defaultMaterial != null)
            {
                btnRenderer.material = defaultMaterial;
            }
            isHovered = false;
        }

        /// <summary>
        /// Selects this element.
        /// </summary>
        public void Select()
        {
            if (!IsSelected)
            {
                IsSelected = true;

                if (btnRenderer != null && selectMaterial != null)
                {
                    btnRenderer.material = selectMaterial;
                }
            }
        }

        /// <summary>
        /// Unselects this element if it was selected.
        /// </summary>
        public void UnSelect()
        {
            if (IsSelected)
            {
                IsSelected = false;
                if (btnRenderer != null && defaultMaterial != null)
                {
                    btnRenderer.material = defaultMaterial;
                }
            }
        }
    }

}
