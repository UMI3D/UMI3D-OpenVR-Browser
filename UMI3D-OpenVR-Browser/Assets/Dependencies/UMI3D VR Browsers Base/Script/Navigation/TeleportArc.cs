﻿/*
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
using System.Collections.Generic;
using UnityEngine;

namespace umi3dVRBrowsersBase.navigation
{
    /// <summary>
    /// Displays a preview of a user's teleportation.
    /// </summary>
    public class TeleportArc : MonoBehaviour
    {
        #region Fields

        [Tooltip("Object to used to display the arc")]
        public GameObject stepDisplayerPrefab;

        [Tooltip("Object to display impact point")]
        public GameObject impactPoint;

        [Tooltip("Object to display impact point can not be used to teleport")]
        public GameObject errorPoint;

        [Tooltip("Where teleportation ray starts")]
        public Transform rayStartPoint;

        [Tooltip("ray speed")]
        public float raySpeed = 6;

        [Tooltip("Max angle authorized to teleport")]
        public float maxVerticalAngle = 45f;

        /// <summary>
        /// Arc discrete subdivision length.
        /// </summary>
        public float stepLength = 0.1f;

        [Tooltip("Arc max lenght")]
        public float arcMaxLength = 100;

        /// <summary>
        /// Time spent between each update (in seconds).
        /// </summary>
        public float updateRate = 0.1f;

        [Tooltip("Layer used for navmesh")]
        public LayerMask navmeshLayer;

        /// <summary>
        /// Coroutine which updates the arc.
        /// </summary>
        private Coroutine updateRoutine = null;

        /// <summary>
        /// Objects which display teleportation arc.
        /// </summary>
        private List<GameObject> displayers = new List<GameObject>();

        #endregion

        #region Methods

        protected virtual void Awake()
        {
            for (int i = 0; i < arcMaxLength / stepLength + 1; i++)
            {
                GameObject disp = Instantiate(stepDisplayerPrefab, this.transform);
                disp.SetActive(false);
                displayers.Add(disp);
            }
        }

        /// <summary>
        /// Displays teleportation arc.
        /// </summary>
        [ContextMenu("Display")]
        public void Display()
        {
            if (updateRoutine != null)
                return;

            updateRoutine = StartCoroutine(UpdateArc());
        }

        /// <summary>
        /// Hides teleportation arc.
        /// </summary>
        [ContextMenu("Hide")]
        public void Hide()
        {
            if (updateRoutine == null)
                return;

            impactPoint.SetActive(false);
            errorPoint.SetActive(false);
            foreach (GameObject g in displayers)
                g.SetActive(false);

            StopCoroutine(updateRoutine);
            updateRoutine = null;
        }

        /// <summary>
        /// Tries to find impact point.
        /// </summary>
        /// <returns></returns>
        public Vector3? GetPointedPoint()
        {
            if (impactPoint.activeSelf)
                return impactPoint.transform.position;
            else
                return null;
        }

        /// <summary>
        /// Gets pointed point.
        /// </summary>
        /// <param name="distanceFromStartAlongArc"></param>
        /// <returns></returns>
        private Vector3 GetArcPoint(float distanceFromStartAlongArc)
        {
            return Physics.gravity * distanceFromStartAlongArc * distanceFromStartAlongArc
                + rayStartPoint.forward * raySpeed * distanceFromStartAlongArc
                + rayStartPoint.position;
        }

        /// <summary>
        /// Udpates arc display.
        /// </summary>
        /// <returns></returns>
        private IEnumerator UpdateArc()
        {
            while (true)
            {
                Vector3 previousArcPoint = rayStartPoint.position;
                int stepCount = 0;
                for (stepCount = 1; stepCount < arcMaxLength / stepLength; stepCount++)
                {
                    Vector3 point = GetArcPoint(stepCount * stepLength);
                    Vector3 nextPoint = GetArcPoint((stepCount + 1) * stepLength);

                    RaycastHit hit;
                    if (Physics.Raycast(previousArcPoint, point - previousArcPoint, out hit, (nextPoint - point).magnitude, navmeshLayer))
                    {
                        TeleportArea area = hit.transform.GetComponent<TeleportArea>();

                        if (area != null && Vector3.Angle(hit.normal, Vector3.up) <= maxVerticalAngle)
                        {
                            impactPoint.SetActive(true);
                            impactPoint.transform.position = hit.point;
                            impactPoint.transform.LookAt(hit.point + hit.normal);
                            errorPoint.SetActive(false);
                        }
                        else
                        {
                            Debug.DrawRay(previousArcPoint, point - previousArcPoint, Color.red, 10);

                            impactPoint.SetActive(false);
                            errorPoint.SetActive(true);
                            errorPoint.transform.position = hit.point;
                            errorPoint.transform.LookAt(hit.point + hit.normal);
                        }
                        break;
                    }
                    else
                    {
                        impactPoint.SetActive(false);
                        errorPoint.SetActive(false);
                    }

                    GameObject disp = displayers[stepCount];
                    disp.transform.position = point;
                    if (!disp.activeSelf)
                        disp.SetActive(true);
                    previousArcPoint = point;
                }

                if (stepCount < arcMaxLength / stepLength)
                {
                    for (int i = stepCount; i < arcMaxLength / stepLength; i++)
                    {
                        displayers[i].SetActive(false);
                    }
                }
                else
                {
                    impactPoint.SetActive(false);
                    errorPoint.SetActive(false);
                }

                yield return new WaitForSeconds(updateRate);
            }
        }

        #endregion
    }
}