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

using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace OpenVRBrowser.WebView
{
    /// <summary>
    /// Manages screen touches for <see cref="UnityGeckoWebView"/>.
    /// </summary>
    public class WebViewInput : UnityEngine.UI.Selectable, IPointerMoveHandler
    {
        #region Fields

        [SerializeField, Tooltip("Rect Transform associated to webview raw image transform.")]
        private RectTransform rawImageRectTransform;

        [SerializeField]
        private WebView webView;

        [SerializeField, Tooltip("Simulate a click when a short pointer down is detected ?")]
        private bool simulateClick = false;

        /// <summary>
        /// Event triggered when a a pointer down event is detected. Pointer coordinates in pixels.
        /// </summary>
        public UnityEvent<Vector2> onPointerDown = new();

        /// <summary>
        /// Event triggered when a a pointer up event is detected. Pointer coordinates in pixels.
        /// </summary>
        public UnityEvent<Vector2> onPointerUp = new();

        #region Data

        /// <summary>
        /// World coordinates of raw image corners.
        /// </summary>
        Vector3[] coordinates = new Vector3[4];

        Vector3 up, right;

        /// <summary>
        /// Last time a up trigger was performed.
        /// </summary>
        private float lastUp = 0;

        /// <summary>
        /// Time to consider that a trigger is a click.
        /// </summary>
        private const int clickTime = 200;

        #endregion

        #endregion

        #region Methods

        protected override void Awake()
        {
            Debug.Assert(rawImageRectTransform != null, "RecTransform can not be null");
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            OnPointerDown(eventData.pointerCurrentRaycast.worldPosition);
        }

        public async void OnPointerDown(Vector3 worldHitPoint)
        {
            rawImageRectTransform.GetWorldCorners(coordinates);

            up = (coordinates[1] - coordinates[0]);
            right = (coordinates[3] - coordinates[0]);

            Vector3 localPosition = WorldToLocal(worldHitPoint);

            onPointerDown.Invoke(localPosition);

            if (simulateClick)
            {
                await Task.Delay(clickTime);

                if (Time.time - lastUp < clickTime / 1000f)
                {
                    webView.OnClick(localPosition, VoltstroStudios.UnityWebBrowser.Shared.Events.MouseEventType.Down);
                    await Task.Delay(40);
                    webView.OnClick(localPosition, VoltstroStudios.UnityWebBrowser.Shared.Events.MouseEventType.Up);
                }
                else
                {
                    webView.OnClick(localPosition, VoltstroStudios.UnityWebBrowser.Shared.Events.MouseEventType.Down);
                }
            }
            else
            {
                webView.OnClick(localPosition, VoltstroStudios.UnityWebBrowser.Shared.Events.MouseEventType.Down);
            }
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (coordinates[0] == coordinates[2])
                rawImageRectTransform.GetWorldCorners(coordinates);

            OnPointerMove(eventData.pointerCurrentRaycast.worldPosition);
        }

        public void OnPointerMove(Vector3 worldHitPoint)
        {
            Vector3 localPosition = WorldToLocal(worldHitPoint);

            webView.OnPointerMove(localPosition);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            OnPointerUp(eventData.pointerCurrentRaycast.worldPosition);
        }

        public void OnPointerUp(Vector3 worldHitPoint)
        {
            Vector3 localPosition = WorldToLocal(worldHitPoint);
            webView.OnClick(localPosition, VoltstroStudios.UnityWebBrowser.Shared.Events.MouseEventType.Up);

            onPointerUp.Invoke(localPosition);

            lastUp = Time.time;
        }

        private Vector3 WorldToLocal(Vector3 worldPosition)
        {
            Vector3 localPosition = worldPosition - coordinates[0];

            localPosition.x = Vector3.Dot(localPosition, right.normalized) / right.magnitude * webView.width;
            localPosition.y = webView.height - Vector3.Dot(localPosition, up.normalized) / up.magnitude * webView.height;

            return localPosition;
        }

        #endregion
    }
}