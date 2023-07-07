/*
Copyright 2019 - 2023 Inetum

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
using UnityEngine;
using static umi3dVRBrowsersBase.connection.PlayerPrefsManager;

namespace umi3dVRBrowsersBase.settings
{
    public static class SettingPreferences
    {
        #region Resolution
        public const string Umi3dResolution = "umi3d-resolution";
        public enum ResolutionEnum
        {
            Low,
            Medium,
            High,
            Custom
        }
        public enum QualityEnum
        {
            VLow,
            Low,
            Medium,
            High,
            VHigh,
            Ultra
        }
        public enum UIZoom
        {
            Small,
            Medium,
            Large,
            Custom
        }
        [Serializable]
        public class ResolutionData
        {
            public ResolutionEnum GameResolution;
            public string FullScreenResolution;
            public QualityEnum Quality;
            public bool HDR;
            public float RenderScale;
            public UIZoom UISize;
            public float DPI;
            public bool ReduceAnimation;
        }

        public static bool TryGetResolutionData(out ResolutionData data)
        {
            data = JsonUtility.FromJson<ResolutionData>(PlayerPrefs.GetString(Umi3dResolution));
            return data != null;
        }

        public static void StoreResolutionData(ResolutionData data)
        {
            PlayerPrefs.SetString(Umi3dResolution, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }
        #endregion

        #region Audio
        public const string Umi3dAudio = "umi3d-audio";
        [Serializable]
        public class AudioData
        {
            public bool AudioOn;
            public bool MicOn;
        }

        public static bool TryGetAudioData(out AudioData data)
        {
            data = JsonUtility.FromJson<AudioData>(PlayerPrefs.GetString(Umi3dAudio));
            return data != null;
        }
        public static void StoreAudioData(AudioData data)
        {
            PlayerPrefs.SetString(Umi3dAudio, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }
        #endregion

        #region Notification
        public const string Umi3dNotification = "umi3d-notification";
        [Serializable]
        public class NotificationData
        {
            public bool HideNotification;
        }

        public static bool TryGetNotificationData(out NotificationData data)
        {
            data = JsonUtility.FromJson<NotificationData>(PlayerPrefs.GetString(Umi3dNotification));
            return data != null;
        }
        public static void StoreNotificationData(NotificationData data)
        {
            PlayerPrefs.SetString(Umi3dNotification, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }
        #endregion
    }
}