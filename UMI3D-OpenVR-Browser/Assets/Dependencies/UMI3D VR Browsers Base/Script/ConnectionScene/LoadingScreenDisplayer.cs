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
using umi3dVRBrowsersBase.ui.playerMenu;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.connection
{
    public class LoadingScreenDisplayer : PersistentSingleBehaviour<LoadingScreenDisplayer>
    {
        /// <summary>
        /// Sphere used to make the screen black;
        /// </summary>
        [SerializeField]
        private MeshRenderer loadingSphere;

        private Camera cam;

        [SerializeField]
        private LayerMask loadingCullingMask;

        private int defaultCullingMask;

        [SerializeField]
        GameObject loadingLabel;
        [SerializeField]
        Text LoadingText;

        /// <summary>
        /// Event raised when loading screen is displayed.
        /// </summary>
        public static UnityEvent OnLoadingScreenDislayed = new UnityEvent();

        /// <summary>
        /// Event raised when loading screen is hidden.
        /// </summary>
        public static UnityEvent OnLoadingScreenHidden = new UnityEvent();

        private bool isVisible = false;

        private void Start()
        {
            cam = Camera.main;
            Debug.Assert(cam != null, "Impossible to find a camera");

            defaultCullingMask = cam.cullingMask;

            Hide();
            DontDestroyOnLoad(loadingSphere.gameObject);

            umi3d.cdk.collaboration.UMI3DCollaborationClientServer.onProgress.AddListener(NewProgress);
        }

        Progress _progress = null;
        void NewProgress(Progress progress)
        {
            if (_progress != null)
            {
                _progress.OnCompleteUpdated -= OnCompleteUpdated;
                _progress.OnFailedUpdated -= OnFailedUpdated;
                _progress.OnStatusUpdated -= OnStatusUpdated;
            }
            _progress = progress;

            void OnCompleteUpdated(float i) { OnProgressChange(_progress.progressPercent / 100f); }
            void OnFailedUpdated(float i) { }
            void OnStatusUpdated(string i) { LoadingText.text = _progress.currentState; }

            _progress.OnCompleteUpdated += OnCompleteUpdated;
            _progress.OnFailedUpdated += OnFailedUpdated;
            _progress.OnStatusUpdated += OnStatusUpdated;

            OnProgressChange(_progress.progressPercent / 100f);
            LoadingText.text = _progress.currentState;
        }

        /// <summary>
        /// Displays loading screen
        /// </summary>
        public void Display()
        {
            if (ConnectionMenuManager.instance != null)
            {
                if (PlayerMenuManager.Exists)
                    loadingSphere.transform.position = PlayerMenuManager.Instance.PlayerCameraTransform.position;

                loadingSphere.enabled = true;
            }
            else
            {
                loadingLabel.SetActive(true);

                cam.cullingMask = loadingCullingMask.value;
                cam.clearFlags = CameraClearFlags.SolidColor;

                if (PlayerMenuManager.Exists)
                {
                    loadingLabel.transform.position = PlayerMenuManager.Instance.PlayerCameraTransform.position + PlayerMenuManager.Instance.PlayerCameraTransform.forward * 1f;
                    loadingLabel.transform.LookAt(PlayerMenuManager.Instance.PlayerCameraTransform.position);
                    loadingLabel.transform.Rotate(0, 180, 0);
                }
            }

            if (!isVisible)
            {
                OnLoadingScreenDislayed?.Invoke();
                isVisible = true;
            }
        }

        /// <summary>
        /// Hides loading screen
        /// </summary>
        public void Hide()
        {
            isVisible = false;

            loadingSphere.enabled = false;
            cam.cullingMask = defaultCullingMask;
            cam.clearFlags = CameraClearFlags.Skybox;
            loadingLabel.SetActive(false);

            OnLoadingScreenHidden?.Invoke();
        }

        private void OnProgressChange(float val)
        {
            if (val >= 0 && val < 1)
            {
                Display();
            }
            else Hide();
        }
    }
}