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
using umi3d.cdk;
using UnityEngine;

namespace umi3dVRBrowsersBase.settings
{
    /// <summary>
    /// Stores global information about the environment.
    /// </summary>
    public class EnvironmentSettings : SingleBehaviour<EnvironmentSettings>
    {
        #region Fields

        /// <summary>
        /// Is the environement loaded ?
        /// </summary>
        public bool IsEnvironmentLoaded { get; private set; }

        /// <summary>
        /// Environment loader.
        /// </summary>
        private UMI3DEnvironmentLoader environmentLoader;

        /// <summary>
        /// Class responsible for sound activation.
        /// </summary>
        public AudioSetting audioSetting;

        /// <summary>
        /// Class reponsible for avatar's trackings sending.
        /// </summary>
        public AvatarSetting avatarSetting;

        /// <summary>
        /// Class responsible for microphone activation.
        /// </summary>
        public MicSetting micSetting;

        #endregion

        #region Methods

        protected override void Awake()
        {
            base.Awake();

            audioSetting = new AudioSetting();
            avatarSetting = new AvatarSetting();
            micSetting = new MicSetting();
        }

        protected void Start()
        {
            environmentLoader = UMI3DEnvironmentLoader.Instance;
            Debug.Assert(environmentLoader != null);
            environmentLoader.onEnvironmentLoaded.AddListener(() => IsEnvironmentLoaded = true);
        }

        #endregion
    }
}
