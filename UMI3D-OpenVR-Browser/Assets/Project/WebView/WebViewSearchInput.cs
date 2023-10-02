using OpenVRBrowser.WebView;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VoltstroStudios.UnityWebBrowser.Shared;

namespace OpenVRBrowser.WebView
{
    public class WebViewSearchInput : MonoBehaviour
    {
        [SerializeField]
        private WebView webView = null;

        [SerializeField]
        private InputField url = null;

        /// <summary>
        /// Regex to validate an url
        /// </summary>
        Regex validateDateRegex;

        private void Start()
        {
            validateDateRegex = new Regex("^https?:\\/\\/(?:www\\.)?[-a-zA-Z0-9@:%._\\+~#=]{1,256}\\.[a-zA-Z0-9()]{1,6}\\b(?:[-a-zA-Z0-9()@:%_\\+.~#?&\\/=]*)$");
        }

        public void SearchUrl()
        {
            if (validateDateRegex.IsMatch(url.text))
                webView.browser.NavigateUrl(url.text);
            else if (url.text.EndsWith(".com") || url.text.EndsWith(".net") || url.text.EndsWith(".fr") || url.text.EndsWith(".org"))
                webView.browser.NavigateUrl("http://" + url.text);
            else
                webView.browser.NavigateUrl("https://www.google.com/search?q=" + url.text);
        }
    }
}

