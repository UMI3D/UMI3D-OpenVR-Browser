/*
Copyright 2019 - 2021 Inetum
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

using UnityEngine;

namespace umi3dBrowsers.interaction.selection.intentdetector.method
{
    /// <summary>
    /// Serialized parameters for IntenSelect detectors
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "IntenSelectParameters", menuName = "UMI3D/Selection/Detector parameters/IntenSelect")]
    public class IntenSelectParameters : AbstractMethodParameters
    {
        /// <summary>
        /// Corrective term according to de Haan et al. 2005
        /// </summary>
        /// The value should be comprised between 0 (strictly) and 1.
        /// A value closer to 1 will tend to a classic conic field while a smaller value will bend the cone.
        [SerializeField, Tooltip("Corrective term according to de Haan et al. 2005. 0<k<=1.")]
        public float corrective_k = 4 / 5;

        /// <summary>
        /// Cone angle in degrees, correspond to the half of the full angle at its apex
        /// </summary>
        [SerializeField, Tooltip("Cone angle in degrees, correspond to the half of the full angle at its apex.")]
        public float coneAngle = 15;

        /// <summary>
        /// Rate of decay of the score at each step
        /// </summary>
        /// Should be set such as <see cref="stickinessRate"/> + <see cref="snappinessRate"/> = 1.
        /// Set to 0.5 based on the original study.
        [SerializeField, Tooltip("Rate of decay of the score at each step. Should sum up to 1 with the snapiness rate.")]
        public float stickinessRate = 0.5f;

        /// <summary>
        /// Rate of increase of the score at each step
        /// </summary>
        /// See <see cref="stickinessRate"/>.
        [SerializeField, Tooltip("Rate of increase of the score at each step. Should sum up to 1 with the stickiness rate.")]
        public float snappinessRate = 0.5f;

        /// <summary>
        /// Maximum score before provoking a reset of the detector
        /// </summary>
        /// Exist mainly for security reason, the score should never been reached in real use cases.
        [Header("Score boundaries"), Tooltip("aximum score before provoking a reset of the detector.")]
        [SerializeField]
        public float scoreMax = 70;

        /// <summary>
        /// Minimum score for an object to remain considered
        /// </summary>
        /// Exist mainly for security reason, the score should never been reached in real use cases.
        [SerializeField, Tooltip("Minimum score for an object to remain considered.")]
        public float scoreMin = -10;
    }
}