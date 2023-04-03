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
using inetum.unityUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.baseBrowser.emotes;
using umi3dVRBrowsersBase.ui.watchMenu;
using UnityEngine;
using UnityEngine.EventSystems;

namespace umi3dVRBrowsersBase.ui
{
    public class EmoteMenu : SingleBehaviour<EmoteMenu>
    {
        public Camera Camera;
        public Sprite ClapIcon;
        public Sprite HelloIcon;
        public Sprite NoIcon;
        public Sprite YesIcon;

        [HideInInspector]
        public Canvas_C Canvas;
        [HideInInspector]
        public Color_C Background;

        [HideInInspector]
        public Text_C WindowName;
        [HideInInspector]
        public CloseButton Close;
        [HideInInspector]
        public Separator_C Separator;

        [HideInInspector]
        public EmoteButton EmoteButton1;
        [HideInInspector]
        public EmoteButton EmoteButton2;
        [HideInInspector]
        public EmoteButton EmoteButton3;
        [HideInInspector]
        public EmoteButton EmoteButton4;

        public static event Action<bool> EmoteButtonStatusChanged;

        protected override void Awake()
        {
            Canvas = new Canvas_C();
            Background = new Color_C() { Color = new Color(0.5215687f, 0.7686275f, 0.7686275f) };
            
            WindowName = new Text_C("Reactions");
            var windowNameSize = Background.RectT.sizeDelta;
            windowNameSize /= 1.5f;
            windowNameSize.y = 13f;
            WindowName.RectT.sizeDelta = windowNameSize;
            WindowName.RectT.localPosition = new Vector3(0f, 44f, 0f);
            WindowName.TextPro.fontSize = 10f;
            WindowName.TextPro.alignment = TMPro.TextAlignmentOptions.Center;

            Close = new CloseButton();
            Close.Button.RectT.localPosition = new Vector3(43f, 43, 0);
            Close.Clicked += Hide;

            Separator = new Separator_C();
            Separator.RectT.sizeDelta = new Vector2(100f, .5f);
            Separator.RectT.localPosition = new Vector3(0f, 36f, 0f);

            EmoteButton1 = new EmoteButton();
            EmoteButton2 = new EmoteButton();
            EmoteButton3 = new EmoteButton();
            EmoteButton4 = new EmoteButton();

            EmoteButton1.Name.Text = "Hi!";
            EmoteButton2.Name.Text = "Applaud";
            EmoteButton3.Name.Text = "Yes!";
            EmoteButton4.Name.Text = "No!";

            EmoteButton1.Emote.Sprite = HelloIcon;
            EmoteButton2.Emote.Sprite = ClapIcon;
            EmoteButton3.Emote.Sprite = YesIcon;
            EmoteButton4.Emote.Sprite = NoIcon;

            EmoteButton1.Button.RectT.localPosition = new Vector3(-25f, 15f, 0f);
            EmoteButton2.Button.RectT.localPosition = new Vector3(25f, 15f, 0f);
            EmoteButton3.Button.RectT.localPosition = new Vector3(-25f, -25f, 0f);
            EmoteButton4.Button.RectT.localPosition = new Vector3(25f, -25f, 0f);

            EmoteButton1.Clicked += Hide;
            EmoteButton2.Clicked += Hide;
            EmoteButton3.Clicked += Hide;
            EmoteButton4.Clicked += Hide;

            this.gameObject.Add(Canvas.Go);
            Canvas.Go.Add(Background.Go);
            Background.Go.Add(WindowName.Go);
            Background.Go.Add(Close.Button.Go);
            Background.Go.Add(Separator.Go);
            Background.Go.Add(EmoteButton1.Button.Go);
            Background.Go.Add(EmoteButton2.Button.Go);
            Background.Go.Add(EmoteButton3.Button.Go);
            Background.Go.Add(EmoteButton4.Button.Go);

            Canvas.RectT.localPosition = Vector3.zero;
            Canvas.RectT.rotation = Quaternion.Euler(0f, 180f, 0f);
        }

        private void Start()
        {
            if (EventSystem.current == null) UnityEngine.Debug.LogError($"Event System null.");
            Canvas.Scale = .005f;

            EmoteManager.Instance.EmotesLoaded += EmoteConfigReceived;
        }

        protected void EmoteConfigReceived(List<EmoteManager.Emote> emotes)
        {
            UnityEngine.Debug.Log("<color=green>TODO: </color>" + $"emote received");
            foreach (var emote in emotes)
            {
                if (emote.Label.StartsWith("Hi", StringComparison.CurrentCultureIgnoreCase)) EmoteButton1.Clicked += () => EmoteManager.Instance.PlayEmote(emote);
                if (emote.Label.StartsWith("Applaud", StringComparison.CurrentCultureIgnoreCase)) EmoteButton2.Clicked += () => EmoteManager.Instance.PlayEmote(emote);
                if (emote.Label.StartsWith("Yes", StringComparison.CurrentCultureIgnoreCase)) EmoteButton3.Clicked += () => EmoteManager.Instance.PlayEmote(emote);
                if (emote.Label.StartsWith("No", StringComparison.CurrentCultureIgnoreCase)) EmoteButton4.Clicked += () => EmoteManager.Instance.PlayEmote(emote);
            }
        }

        [ContextMenu("Hide")]
        public void Hide()
        {
            Canvas.Go.SetActive(false);
            EmoteButtonStatusChanged.Invoke(false);
        }
        [ContextMenu("Display")]
        public void Display()
        {
            Canvas.Go.SetActive(true);
            UpdateCanvasPosition();
            EmoteButtonStatusChanged.Invoke(true);
        }

        [ContextMenu("Look")]
        protected void UpdateCanvasPosition()
        {
            if (Camera == null) return;

            var CanvasSize = Canvas.ConcreteSize;

            Vector3 fakeForward = Camera.transform.forward;
            fakeForward.y = 0.0f;
            fakeForward.Normalize();

            Vector3 CameraPosition = Camera.transform.position;

            Vector3 MenuPosition = new Vector3
            (
                CameraPosition.x,
                CameraPosition.y - CanvasSize.y / 10f,
                CameraPosition.z
            ) + fakeForward * 1f;

            transform.localPosition = MenuPosition;

            transform.LookAt(CameraPosition);
        }
    }
}
