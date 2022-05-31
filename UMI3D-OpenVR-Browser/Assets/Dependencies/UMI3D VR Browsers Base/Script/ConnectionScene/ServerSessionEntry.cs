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
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.connection
{
    /// <summary>
    /// Reponsible for displaying a session returned by a master server.
    /// </summary>
    public class ServerSessionEntry : MonoBehaviour
    {
        #region Fields

        [Tooltip("Button to join the session")]
        public Button button;

        [Tooltip("Background of the session item")]
        public Image background;

        [Tooltip("Label to display session name")]
        public Text sessionNameText;

        [Tooltip("Label to display number of players in the session")]
        public Text playerCountText;

        [Tooltip("Label to display when the session started")]
        public Text sessionTimeText;

        [Tooltip("Background used when the item is not selected")]
        public Sprite defaultBackground;

        [Tooltip("Background used when the item is selected")]
        public Sprite selectedBackground;

        [Tooltip("Data received by master server")]
        private BeardedManStudios.Forge.Networking.MasterServerResponse.Server data;

        /// <summary>
        /// Ip of the session.
        /// </summary>
        public string SessionIp { get => data.Address; }

        /// <summary>
        /// Port of the session.
        /// </summary>
        public string SessionPort { get => ((int)data.Port).ToString(); }

        #endregion

        #region Methods

        /// <summary>
        /// Sets up the UI.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="panel"></param>
        public void Setup(BeardedManStudios.Forge.Networking.MasterServerResponse.Server data, ConnectToSessionPanel panel)
        {
            this.data = data;

            sessionNameText.text = data.Name;
            playerCountText.text = data.PlayerCount.ToString();
            sessionTimeText.text = "--:--";

            button.onClick.AddListener(() =>
            {
                panel.OnSelectionChanged(this);
            });
        }

        /// <summary>
        /// Selects the item.
        /// </summary>
        public void Select()
        {
            background.sprite = selectedBackground;
        }

        /// <summary>
        /// Unselects the UI?
        /// </summary>
        public void UnSelect()
        {
            background.sprite = defaultBackground;
        }

        #endregion
    }
}