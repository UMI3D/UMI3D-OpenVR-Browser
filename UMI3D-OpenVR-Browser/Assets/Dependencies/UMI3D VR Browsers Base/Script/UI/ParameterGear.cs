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

using inetum.unityUtils;
using MathNet.Numerics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using umi3d.cdk.interaction;
using umi3dVRBrowsersBase.interactions;
using umi3dVRBrowsersBase.ui.playerMenu;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WebSocketSharp;

namespace umi3dVRBrowsersBase.ui
{
    /// <summary>
    /// 3D Object to show users its associated UMI3D Entity has UMI3D Parameters, by clicking on this gear, opens <see cref="playerMenu.PlayerMenuManager"/> to edit them.
    /// </summary>
    public class ParameterGear : AbstractClientInteractableElement, ITriggerableElement
    {
        #region Fields

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public UnityEvent OnTriggered { get; private set; } = new UnityEvent();

        /// <summary>
        /// Is the gear currently hovered ?
        /// </summary>
        public bool IsHovered { get; private set; } = false;

        public bool IsDisplayed => isActiveAndEnabled;

        /// <summary>
        /// <see cref="Interactable"/> which contains the parameters.
        /// </summary>
        private Interactable currentAssociatedInteractable;
        public Interactable CurrentAssociatedInteractable
        {
            get => currentAssociatedInteractable;
            set
            {
                currentAssociatedInteractable = value;

                if (value == null)
                {
                    container = null;
                }
                else
                {
                    container = InteractableContainer.containers.Find(c => c.Interactable == currentAssociatedInteractable);
                    Debug.Assert(container != null, "No container found for an Interactable, should not happen.");
                }
            }
        }

        /// <summary>
        /// Container associated to <see cref="CurrentAssociatedInteractable"/>.
        /// </summary>
        private InteractableContainer container;

        [SerializeField]
        [Tooltip("Should this gear display on top of every other objects ?")]
        private bool displayOnTopOfEverything;

        [SerializeField]
        [Tooltip("Gear image")]
        private Image gearImage;

        [SerializeField]
        [Tooltip("Default background")]
        private Sprite defaultSprite;

        [SerializeField]
        [Tooltip("Background for hover feedback")]
        private Sprite hoverSprite;

        [SerializeField]
        [Tooltip("Background for press feedback")]
        private Sprite pressSprite;

        [SerializeField]
        [Tooltip("player needed for distance.")]
        private Transform player;

        /// <summary>
        /// Name of the shader property which enable or disable Z-depth test.
        /// </summary>
        private const string shaderTestMode = "unity_GUIZTestMode";

        #endregion

        #region Methods

        private void Start()
        {
            if (displayOnTopOfEverything)
                DisplayOnTopOfEverything();

            PlayerMenuManager.Instance.onMenuClose.AddListener(Hide);

            Hide();
        }

        /// <summary>
        /// Display image on top of everything. 
        /// </summary>
        /// Solution found on this thread <see href="https://answers.unity.com/questions/878667/world-space-canvas-on-top-of-everything.html"/>.
        private void DisplayOnTopOfEverything()
        {
            Material material = gearImage.materialForRendering;

            if (material != null)
            {
                var materialCopy = new Material(material);
                materialCopy.SetInt(shaderTestMode, (int)UnityEngine.Rendering.CompareFunction.Always);
                gearImage.material = materialCopy;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="controller"></param>
        public void Trigger(ControllerType controllerType)
        {
            OnTriggered?.Invoke();

            PlayerMenuManager.Instance.OpenParameterMenu(controllerType, menuAsync: true);

            if (gameObject.activeInHierarchy)
                StartCoroutine(ClickAnimation());
        }

        /// <summary>
        /// Displays a click feedback for a certain time.
        /// </summary>
        /// <returns></returns>
        private IEnumerator ClickAnimation()
        {
            gearImage.sprite = pressSprite;
            yield return new WaitForSeconds(.15f);

            if (IsHovered)
                gearImage.sprite = hoverSprite;
            else
                gearImage.sprite = defaultSprite;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void HoverEnter()
        {
            IsHovered = true;
            gearImage.sprite = hoverSprite;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void HoverExit()
        {
            IsHovered = false;
            gearImage.sprite = defaultSprite;
        }

        /// <summary>
        /// Displays the parameter gear for an interactable.
        /// </summary>
        /// <param name="interactable"></param>
        /// <param name="position">World position of the gear</param>
        /// <param name="normal">World normal of the gear</param>
        public void Display(Interactable interactable, Vector3 position, Vector3 normal, Vector2 rayDirection)
        {
            isInteractableSelected = true;

            gameObject.SetActive(true);

            this.CurrentAssociatedInteractable = interactable;

            this.transform.position = position;
            this.transform.rotation = Quaternion.LookRotation(normal, Vector3.up);
            this.transform.localRotation *= Quaternion.Euler(0, 0, Vector3.SignedAngle(transform.up, -rayDirection, transform.forward));
        }

        /// <summary>
        /// Displays the parameter gear for an interactable. Use an interactable container and a look at point and compute the adequate position.
        /// </summary>
        /// <param name="interactableContainer"></param>
        /// <param name="lookAtPoint">World position of the point the object is looked at.</param>
        public void Display(InteractableContainer interactableContainer, Vector3 lookAtPoint)
        {
            isInteractableSelected = true;

            Vector3 rootPosition;
            Vector3 normal;
            Vector3 rayDirection;
            if (interactableContainer.TryGetComponent(out MeshCollider collider) && collider.convex)
            {
                rootPosition = collider.ClosestPoint(lookAtPoint);
                rayDirection = (rootPosition - lookAtPoint).normalized;
                normal = -rayDirection;
            }
            else
            {
                Ray ray = new Ray(lookAtPoint, interactableContainer.transform.position - lookAtPoint);
                (RaycastHit[] hits, int hitCount) hitsInfo = umi3d.common.Physics.RaycastAll(ray);

                if (hitsInfo.hitCount == 0) // happens is the center of the object is outside of the mesh
                {
                    rootPosition = interactableContainer.transform.position;
                    rayDirection = (rootPosition - lookAtPoint).normalized;
                    normal = -rayDirection;
                }
                else
                {
                    //TODO : remove try catch later for a better test
                    try
                    {
                        Collider icCollider = interactableContainer.GetComponentInChildren<Collider>();
                        RaycastHit[] hits = hitsInfo.hits.SubArray(0, hitsInfo.hitCount);
                        float closestDist = hits.Where(x => x.collider == icCollider).Min(x => x.distance);
                        RaycastHit closest = Array.Find(hits, x => x.distance == closestDist);
                        rootPosition = closest.point;
                        normal = closest.normal;
                    }
                    catch
                    {
                        rootPosition = interactableContainer.transform.position;
                        normal = (lookAtPoint - rootPosition).normalized;
                    }

                    rayDirection = (rootPosition - lookAtPoint).normalized;
                }
            }

            Display(interactableContainer.Interactable, rootPosition, normal, rayDirection);
        }

        /// <summary>
        /// Hides the parameter gear.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            this.CurrentAssociatedInteractable = null;
            isInteractableSelected = false;
        }

        public float delayBeforeHiding = 1f;
        public bool isInteractableSelected;

        public void HideWithDelay()
        {
            isInteractableSelected = false;
            float time = 0f;
            IEnumerator HideCoroutine()
            {
                while (time < delayBeforeHiding)
                {
                    time += Time.deltaTime;
                    yield return null;
                }

                if (!isInteractableSelected)
                {
                    Hide();
                }
            }

            CoroutineManager.Instance.AttachCoroutine(HideCoroutine());
            //StartCoroutine(HideCoroutine());
        }

        public override void Select(VRController controller)
        {
            isSelected = true;
        }

        public override void Deselect(VRController controller)
        {
            isSelected = false;
        }

        [Tooltip("Used for gear scale.")]
        public float minDistance = 2f;
        [Tooltip("Used for gear scale.")]
        public float maxDistance = 6f;
        [Tooltip("Used for gear scale.")]
        public float minScale = 0.15f;
        [Tooltip("Used for gear scale.")]
        public float maxScale = .5f;

        

        private void Update()
        {
            if (container == null && gameObject.activeInHierarchy)
            {
                Hide();
            }

            if (IsDisplayed)
            {
                var distance = Vector3.Distance(transform.position, player.position);
                var scale = Mathf.Lerp(minScale, maxScale, Mathf.InverseLerp(minDistance, maxDistance, distance));
                transform.localScale = new Vector3(scale, scale, scale);
            }
        }

        #endregion
    }
}

