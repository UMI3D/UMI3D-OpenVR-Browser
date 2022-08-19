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
using System.Linq;
using umi3dVRBrowsersBase.interactions;
using umi3dVRBrowsersBase.ui;
using UnityEngine;

namespace umi3dVRBrowsersBase.selection
{
    /// <summary>
    /// Selector for <see cref="ITriggerableElement"/>.
    /// </summary>
    public class VRClickableElementSelector : MonoBehaviour
    {
        #region  Static

        /// <summary>
        /// Stores all current <see cref="ITriggerableElement"/> hovered.
        /// </summary>
        static List<ITriggerableElement> elementsHovered = new List<ITriggerableElement>();

        /// <summary>
        /// Notifies that a <see cref="ITriggerableElement"/> starts being hovered.
        /// </summary>
        /// <param name="element"></param>
        public static void NotifyStartHovering(ITriggerableElement element)
        {
            elementsHovered.Add(element);
        }

        /// <summary>
        /// Notifies that a <see cref="ITriggerableElement"/> stops being hovered.
        /// </summary>
        /// <param name="element"></param>
        public static void NotifyStopHovering(ITriggerableElement element)
        {
            elementsHovered.Remove(element);
        }

        /// <summary>
        /// Is a <see cref="ITriggerableElement"/> currently hovered ? Warning : to be consider as hovered, a <see cref="ITriggerableElement"/> must use <see cref="NotifyStartHovering(ITriggerableElement)"/> and <see cref="NotifyStopHovering(ITriggerableElement)"/>.
        /// </summary>
        /// <returns></returns>
        public static bool IsElementHovered()
        {
            return elementsHovered.Count > 0;
        }

        #endregion

        #region Fields

        [SerializeField]
        [Tooltip("Associated laser")]
        private Laser laser;

        /// <summary>
        /// Action to trigger the selector.
        /// </summary>
        public ActionType action;

        /// <summary>
        /// Associated controller.
        /// </summary>
        public ControllerType controller;

        /// <summary>
        /// Last <see cref="ITriggerableElement"/> clicked on.
        /// </summary>
        private ITriggerableElement lastElement;

        /// <summary>
        /// Stores all element hit each frame.
        /// </summary>
        private RaycastHit[] hits;

        #endregion

        #region Methods

        private void LateUpdate()
        {
            var ray = new Ray(transform.position, transform.forward);

            hits = Physics.RaycastAll(ray);
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            bool clickableFound = false;

            foreach (RaycastHit hit in hits)
            {
                ITriggerableElement clickable = hit.transform.GetComponents<ITriggerableElement>().Where(c => (c is MonoBehaviour mono && mono.enabled)).FirstOrDefault();

                if (clickable != null && clickable != default)
                {
                    clickableFound = true;

                    if (lastElement != clickable)
                    {
                        if (lastElement != null)
                        {
                            laser.OnHoverExit(lastElement.GetHashCode());
                            NotifyStopHovering(lastElement);
                            //LastElement.HoverExit(controller);
                        }

                        lastElement = clickable;
                        NotifyStartHovering(clickable);
                        //Clickable.HoverEnter();

                        laser.OnHoverEnter(clickable.GetHashCode());
                    }

                    laser.SetImpactPoint(hit.point);

                    if (AbstractControllerInputManager.Instance.GetButtonDown(controller, action))
                    {
                        VRInteractionMapper.lastControllerUsedToClick = controller;
                        clickable.Trigger(controller);
                    }

                    break;
                }
            }
            if (!clickableFound && lastElement != null)
            {
                laser.OnHoverExit(lastElement.GetHashCode());
                NotifyStopHovering(lastElement);
                //lastElement.HoverExit();
                lastElement = null;
            }
        }

        #endregion
    }
}