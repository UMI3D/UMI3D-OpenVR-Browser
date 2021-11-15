/*
Copyright 2019 - 2021 Inetum
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

public class ServerSessionEntry : MonoBehaviour
{
    public Button button;
    public Image background;

    public Text sessionNameText;
    public Text playerCountText;
    public Text sessionTimeText;

    public Sprite defaultBackground;
    public Sprite selectedBackground;

    bool isSelected = false;

    BeardedManStudios.Forge.Networking.MasterServerResponse.Server data;

    public string SessionIp { get => data.Address; }
    public string SessionPort { get => ((int)data.Port).ToString(); }

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

    public void Select()
    {
        isSelected = true;

        background.sprite = selectedBackground;
    }

    public void UnSelect()
    {
        isSelected = false;

        background.sprite = defaultBackground;
    }
}
