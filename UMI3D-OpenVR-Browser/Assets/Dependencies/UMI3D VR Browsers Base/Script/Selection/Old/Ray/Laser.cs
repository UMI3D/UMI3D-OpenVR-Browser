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

namespace umi3dVRBrowsersBase.selection
{
    /// <summary>
    /// Display a laser using a cylinder (has to be configured directly in the scene).
    /// </summary>
    public class Laser : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// Is an element currently hovered ?
        /// </summary>
        public bool Hovering { get { return tokens.Count > 0; } }

        /// <summary>
        /// Object which displays a laser.
        /// </summary>
        public GameObject laserObject;

        /// <summary>
        /// Object used to display impact point.
        /// </summary>
        public GameObject impactPoint;

        /// <summary>
        /// Default laser color.
        /// </summary>
        public Color defaultColor;

        /// <summary>
        /// Laser color when an object is hovered.
        /// </summary>
        public Color hoverColor;

        /// <summary>
        /// Laser renderer.
        /// </summary>
        private Renderer laserObjectRenderer;

        /// <summary>
        /// Impact point renderer.
        /// </summary>
        private Renderer impactPointRenderer;

        /// <summary>
        /// List of all current object hovered.
        /// </summary>
        private List<int> tokens = new List<int>();

        #endregion

        #region Methods

        /// <summary>
        /// Notify that the component indentified by the given token is hovering something.
        /// </summary>
        /// <param name="token"></param>
        public void OnHoverEnter(int token)
        {
            if (tokens.Contains(token))
            {
                Debug.LogWarning("This token has already been given.");
                return;
            }
            else if (!Hovering)
                HoverEnterInternal();

            tokens.Add(token);
        }

        /// <summary>
        /// Notify that the component indentified by the given token is not hovering anything anymore.
        /// </summary>
        /// <param name="token"></param>
        public void OnHoverExit(int token)
        {
            if (!tokens.Contains(token))
            {
                return;
            }
            tokens.Remove(token);

            if (tokens.Count == 0)
                HoverExitInternal();
        }

        /// <summary>
        /// Performs hover enter.
        /// </summary>
        protected void HoverEnterInternal()
        {
            if (impactPointRenderer != null)
                impactPointRenderer.material.color = hoverColor;

            if (laserObjectRenderer != null)
                laserObjectRenderer.material.color = hoverColor;
        }

        /// <summary>
        /// Performs hover exit.
        /// </summary>
        protected void HoverExitInternal()
        {
            if (impactPointRenderer != null)
                impactPointRenderer.material.color = defaultColor;
            if (laserObjectRenderer != null)
                laserObjectRenderer.material.color = defaultColor;
            SetInfinitePoint();
        }

        /// <summary>
        /// Makes laser infinite.
        /// </summary>
        public void SetInfinitePoint()
        {
            SetImpactPoint(this.transform.position + this.transform.forward * 500, false);
        }

        /// <summary>
        /// Sets impact point of the laser.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="displayImpact"></param>
        public void SetImpactPoint(Vector3 point, bool displayImpact = true)
        {
            impactPoint.transform.position = point;
            laserObject.transform.localScale = new Vector3(1, Vector3.Distance(this.transform.position, point), 1);
            impactPoint.SetActive(displayImpact);
        }

        protected virtual void Awake()
        {
            laserObjectRenderer = laserObject.GetComponent<Renderer>();
            impactPointRenderer = impactPoint.GetComponent<Renderer>();

            if (impactPointRenderer != null)
                impactPointRenderer.material.color = defaultColor;

            if (laserObjectRenderer != null)
                laserObjectRenderer.material.color = defaultColor;

            SetImpactPoint(this.transform.position + this.transform.forward * 500, false);
        }

        #endregion
    }
}