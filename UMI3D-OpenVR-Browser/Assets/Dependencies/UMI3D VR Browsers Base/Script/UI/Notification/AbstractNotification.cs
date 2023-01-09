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
using umi3d.cdk;
using umi3d.common;
using UnityEngine;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.ui.notification
{
    /// <summary>
    /// Base class to display an UMI3D Notification.
    /// </summary>
    public abstract class AbstractNotification : MonoBehaviour
    {
        #region Fields

        [SerializeField]
        [Tooltip("Label to display the title of the notification")]
        private Text titleLabel;

        [SerializeField]
        [Tooltip("Label to display the content of the notification")]
        private Text titleContent;

        [SerializeField]
        [Tooltip("Label to display the image associated to the notification if it has one")]
        private Image image;

        [SerializeField]
        [Tooltip("Button to close the notification before its display time")]
        private Button destroyButton;

        #endregion

        #region Methods

        /// <summary>
        /// Inits the notification.
        /// </summary>
        public virtual void Init(NotificationDto dto)
        {
            titleLabel.text = dto.title;
            titleContent.text = dto.content;

            if (dto.icon2D != null)
                SetIcon(dto.id, dto.icon2D);

            StartCoroutine(SetDurationTime(dto.duration));

            if (destroyButton != null)
                destroyButton.onClick.AddListener(() => this.gameObject.SetActive(false));
        }

        /// <summary>
        /// Destroyes this object after <paramref name="duration"/> seconds.
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        private IEnumerator SetDurationTime(float duration)
        {
            yield return new WaitForSeconds(duration);
            Destroy(this.gameObject);
        }

        /// <summary>
        /// Set parent of this 
        /// </summary>
        /// <param name="parent"></param>
        public void SetParent(Transform parent, Vector3 localPosition, Quaternion localRotation)
        {
            if (parent != null)
            {
                transform.SetParent(parent);
                transform.localPosition = localPosition;
                transform.localRotation = localRotation;
            }
        }

        /// <summary>
        /// Sets icons of the notification.
        /// </summary>
        /// <param name="icon"></param>
        public void SetIcon(ulong id, ResourceDto iconDto)
        {
            if (iconDto == null)
                return;

            if (iconDto.variants.Count == 0)
                return;

            Texture2D res = null;
            FileDto fileToLoad = UMI3DEnvironmentLoader.Parameters.ChooseVariant(iconDto.variants);

            if (fileToLoad != null)
            {
                string url = fileToLoad.url;
                string ext = fileToLoad.extension;
                string authorization = fileToLoad.authorization;
                IResourcesLoader loader = UMI3DEnvironmentLoader.Parameters.SelectLoader(ext);

                if (loader != null)
                {
                    //UMI3DResourcesManager.LoadFile(
                    //    id,
                    //    fileToLoad,
                    //    loader.UrlToObject,
                    //    loader.ObjectFromCache,
                    //    (o) =>
                    //    {
                    //        res = o as Texture2D;
                    //        image.sprite = Sprite.Create(res, new Rect(0.0f, 0.0f, res.width, res.height), new Vector2(0.5f, 0.5f));
                    //    },
                    //    (Umi3dException error) =>
                    //    {
                    //        Debug.LogWarning($"Icon not loadable : {url} [{error.errorCode}:{error.Message}]");
                    //    },
                    //    loader.DeleteObject
                    //    );
                }
                else
                    Debug.LogWarning("No loader was found to load this icon " + url);
            }
            else
            {
                Debug.LogWarning("Impossible to load icon");
            }
        }

        #endregion
    }
}