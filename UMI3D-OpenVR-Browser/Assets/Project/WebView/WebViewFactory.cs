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
using umi3d.cdk;
using UnityEngine;

namespace OpenVRBrowser.WebView
{
    public class WebViewFactory : AbstractWebViewFactory
    {
        public GameObject template = null;

        private static int inPort = 5560;
        private static int outPort = 5561;

        private static float lastTimeWebViewCreated = 0;

        /// <summary>
        /// Delay in seconds between web view creation.
        /// </summary>
        private readonly float creationDelay = 3f;

        public override async Task<AbstractUMI3DWebView> CreateWebView()
        {
            while (lastTimeWebViewCreated != 0 && lastTimeWebViewCreated + creationDelay > Time.time)
            {
                await UMI3DAsyncManager.Yield();
            }

            lock (this)
            {
                lastTimeWebViewCreated = Time.time;

                GameObject go = Instantiate(template);
                WebView view = go.GetComponent<WebView>();

                return view;
            }
        }

        /// <summary>
        /// Returns unique in and out ports for WebViews.
        /// </summary>
        /// <returns></returns>
        public (int, int) GetPorts()
        {
            return (inPort+=2, outPort+=2);
        }
    }
}