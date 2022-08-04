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
    public class DefaultClickableButtonElement : AbstractClientInteractableElement, IClickableElement, IHoverableElement
    {
        #region Fields

        [SerializeField]
        [Tooltip("Event raised when this element is clicked")]
        private UnityEvent onClicked = new UnityEvent();
        private UnityEvent onHoverEnter = new UnityEvent();
        private UnityEvent onHoverExit = new UnityEvent();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public UnityEvent OnClicked { get => onClicked; }
        public UnityEvent OnHoverEnter => onHoverEnter;
        public UnityEvent OnHoverExit => onHoverExit;

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
        /// Is this element currently hovered ?
        /// </summary>
        private bool isHovered = false;

        #endregion

        protected void OnEnable()
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
        public virtual void Click(ControllerType controllerType)
        {
            onClicked?.Invoke();

            if (btnRenderer != null && pressedMaterial != null)
            {
                StartCoroutine(SetPressFeedback());
            }
        }

        public override void Interact(VRController controller)
        {
            Click(controller.type);
        }

        /// <summary>
        /// Displays press feedback during a certain time.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator SetPressFeedback()
        {
            btnRenderer.material = pressedMaterial;
            yield return new WaitForSeconds(.3f);

            if (isSelected && selectMaterial != null)
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

        public virtual void HoverEnter(ControllerType controller)
        {
            if (btnRenderer != null && !isSelected && hoverMaterial != null)
            {
                btnRenderer.material = hoverMaterial;
            }
            isHovered = true;
            onHoverEnter.Invoke();
        }

        public virtual void HoverExit(ControllerType controller)
        {
            if (btnRenderer != null && !isSelected && defaultMaterial != null)
            {
                btnRenderer.material = defaultMaterial;
            }
            isHovered = false;
            onHoverExit.Invoke();
        }

        public virtual bool IsHovered(ControllerType controller) => isHovered;

        public override void Select(VRController controller)
        {
            if (!isSelected)
            {
                isSelected = true;

                if (btnRenderer != null && selectMaterial != null)
                {
                    btnRenderer.material = selectMaterial;
                }
            }
        }

        public override void Deselect(VRController controller)
        {
            if (isSelected)
            {
                isSelected = false;
                if (btnRenderer != null && defaultMaterial != null)
                {
                    btnRenderer.material = defaultMaterial;
                }
            }
        }

        /// <summary>
        /// Force the change for renderer <br/>
        /// Warning: The visual feedback state should be reset after use 
        /// </summary>
        public void ForceSelectionHighlight()
        {
            if (btnRenderer != null && selectMaterial != null)
            {
                btnRenderer.material = selectMaterial;
            }
        }

        /// <summary>
        /// Force the change for renderer <br/>
        /// Warning: The visual feedback state should be reset after use 
        /// </summary>
        public void ForceDeselectionHighlight()
        {
            if (btnRenderer != null && defaultMaterial != null)
                {
                    btnRenderer.material = defaultMaterial;
                }
        }
    }

}
