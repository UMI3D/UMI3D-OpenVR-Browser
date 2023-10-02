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
using System.Text.RegularExpressions;
using umi3d.cdk;
using umi3d.common;
using umi3d.common.interaction;
using umi3dVRBrowsersBase.ui.keyboard;
using UnityEngine;
using UnityEngine.UI;

namespace OpenVRBrowser.WebView
{
    public class WebView : AbstractUMI3DWebView
    {
        public static bool IsWebViewFocused { get; private set; } = false;

        #region Fields

        [SerializeField]
        private RectTransform textureTransform = null;

        [SerializeField]
        public RuntimeWebBrowserBasic browser = null;

        [SerializeField]
        private RectTransform container = null;

        [SerializeField]
        private RectTransform bottomBarContainer = null;

        [SerializeField]
        private RectTransform topBarContainer = null;

        [SerializeField]
        Canvas canvas = null;

        [SerializeField]
        private CustomInputWithKeyboard searchField;

        [SerializeField]
        private Keyboard keyboard;

        private ulong id;

        private string previousUrl;

        private bool useSearchInput = false;

        /// <summary>
        /// Regex to check if a string is an url or not.
        /// </summary>
        private Regex validateURLRegex = new Regex("^https?:\\/\\/(?:www\\.)?[-a-zA-Z0-9@:%._\\+~#=]{1,256}\\.[a-zA-Z0-9()]{1,6}\\b(?:[-a-zA-Z0-9()@:%_\\+.~#?&\\/=]*)$");

        #endregion

        #region Methods

        protected virtual void Awake()
        {
            canvas.worldCamera = Camera.main;
            canvas.transform.localRotation = Quaternion.identity;
            canvas.transform.localPosition = Vector3.zero;

            canvas.GetComponent<CanvasScaler>().dynamicPixelsPerUnit = 3;

            searchField.SetKeyboard(keyboard);

            keyboard.Hide();

            browser.browserClient.OnUrlChanged += (url) =>
            {
                if (url == previousUrl)
                {
                    return;
                }

                previousUrl = url;

                searchField.text = url;

                var request = new WebViewUrlChangedRequestDto
                {
                    url = url,
                    webViewId = id
                };

                UMI3DClientServer.SendRequest(request, true);
            };

            searchField.OnSelectEvent.AddListener(() => {
                useSearchInput = true;
            });
        }

        public override void Init(UMI3DWebViewDto dto)
        {
            base.Init(dto);

            id = dto.id;
        }

        protected override void OnCanInteractChanged(bool canInteract)
        {
            browser.disableMouseInputs = !canInteract;
            browser.disableKeyboardInputs = !canInteract;

            bottomBarContainer.gameObject.SetActive(canInteract);
            topBarContainer.gameObject.SetActive(canInteract);
        }

        protected override void OnSizeChanged(Vector2 size)
        {
            container.localScale = new Vector3(size.x, size.y, 1);

            Vector3[] corners = new Vector3[4];

            textureTransform.GetWorldCorners(corners);

            bottomBarContainer.position = (corners[0] + corners[3]) / 2f;
            topBarContainer.position = (corners[1] + corners[2]) / 2f;

            topBarContainer.localScale = new Vector3(topBarContainer.localScale.x,
                topBarContainer.localScale.y / container.localScale.y, topBarContainer.localScale.z);

            bottomBarContainer.localScale = new Vector3(bottomBarContainer.localScale.x,
                bottomBarContainer.localScale.y / container.localScale.y, bottomBarContainer.localScale.z);
        }

        protected override async void OnTextureSizeChanged(Vector2 size)
        {
            while (!browser.browserClient.ReadySignalReceived && !browser.browserClient.IsConnected)
            {
                await UMI3DAsyncManager.Yield();
            }

            await UMI3DAsyncManager.Yield();

            try
            {
                browser.browserClient.Resize(new VoltstroStudios.UnityWebBrowser.Shared.Resolution((uint)size.x, (uint)size.y));
            }
            catch (Exception ex)
            {
                Debug.LogError("Impossible to resize WebView.");
                Debug.LogException(ex);
            }
        }

        protected override async void OnUrlChanged(string url)
        {
            while (!browser.browserClient.ReadySignalReceived && !browser.browserClient.IsConnected)
            {
                await UMI3DAsyncManager.Yield();
            }

            await UMI3DAsyncManager.Yield();

            try
            {
                Debug.Log("URL " + url);
                browser.browserClient.LoadUrl(url);
            }
            catch (Exception ex)
            {
                Debug.LogError("Impossible to load url " + url);
                Debug.LogException(ex);

                await UMI3DAsyncManager.Delay(5000);

                browser.browserClient.LoadUrl(url);
            }
        }

        public void EnterText(string text)
        {
            if (!useSearchInput)
                Debug.Log("TODO");
        }

        public void DeleteCharacter()
        {
            if (!useSearchInput)
                Debug.Log("TODO");
        }

        public void EnterCharacter()
        {
            if (!useSearchInput)
                Debug.Log("TODO");
        }

        public void GoBack()
        {
            browser.browserClient.GoBack();
        }

        public void GoForward()
        {
            browser.browserClient.GoForward();
        }

        public void GoHome()
        {
            browser.browserClient.LoadUrl("https://www.google.com/");
        }

        public void Search()
        {
            Debug.Assert(searchField != null);

            if (validateURLRegex.IsMatch(searchField.text))
                browser.browserClient.LoadUrl(searchField.text);
            else if (searchField.text.EndsWith(".com") || searchField.text.EndsWith(".net") || searchField.text.EndsWith(".fr") || searchField.text.EndsWith(".org"))
                browser.browserClient.LoadUrl("http://" + searchField.text);
            else
                browser.browserClient.LoadUrl("https://www.google.com/search?q=" + searchField.text);
        }

        #endregion
    }
}