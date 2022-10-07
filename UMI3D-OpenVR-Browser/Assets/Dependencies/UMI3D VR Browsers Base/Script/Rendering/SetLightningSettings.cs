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

using UnityEngine;

namespace umi3dVRBrowsersBase.rendering
{
    /// <summary>
    /// Resets lighting settings when entering an environment.
    /// </summary>
    public class SetLightningSettings : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// Reference to singleton.
        /// </summary>
        private static SetLightningSettings instance;

        /// <summary>
        /// Default skybox of an environment.
        /// </summary>
        public Material defaultSkyboxMat;

        /// <summary>
        /// Skybox used in connection scene.
        /// </summary>
        public Material connectionSkyboxMat;

        /// <summary>
        /// Ambient color of connection scene.
        /// </summary>
        [ColorUsage(true, true)]
        public Color connectionAmbientColor;

        #endregion

        #region Methods

        private void Start()
        {
            instance = this;
            SetConnectionSceneSettings();
        }

        /// <summary>
        /// Resets environements which were added for the connection scene to match with the default server desktop scene.
        /// </summary>
        public static void ResetLightningSettings()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
            RenderSettings.ambientIntensity = 1;

#if PLATFORM_ANDROID
            RenderSettings.reflectionIntensity = 0;
#endif
            RenderSettings.fog = false;
            RenderSettings.skybox = instance?.defaultSkyboxMat;
        }

        /// <summary>
        /// Sets lightning settings for connection scene.
        /// </summary>
        private void SetConnectionSceneSettings()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = connectionAmbientColor;
            RenderSettings.fog = true;
            RenderSettings.skybox = instance?.connectionSkyboxMat;
        }

        #endregion
    }
}
