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

namespace umi3dVRBrowsersBase.ui.playerMenu
{
    public partial class Tool_ToolboxButton
    {
        public Image CustomButtonIcon => m_customIconButton.GetComponent<Image>();
        public bool IsCustom { get; set; } = false;

        [SerializeField]
        protected GameObject m_toolboxButton = null;
        [SerializeField]
        protected GameObject m_toolButton = null;
        [SerializeField]
        protected GameObject m_customIconButton = null;
    }

    public partial class Tool_ToolboxButton : MonoBehaviour
    {
        public enum ToolboxButtonType { Tool, Toolbox, Custom }
        public void SetCustomIcon(Texture2D texture)
        {
            CustomButtonIcon.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            SetButtonType(ToolboxButtonType.Custom);
            IsCustom = true;
        }

        public void SetButtonType(ToolboxButtonType type)
        {
            if (IsCustom) return;
            switch (type)
            {
                case ToolboxButtonType.Tool:
                    m_toolboxButton.SetActive(false);
                    m_toolButton.SetActive(true);
                    m_customIconButton.SetActive(false);
                    break;
                case ToolboxButtonType.Toolbox:
                    m_toolboxButton.SetActive(true);
                    m_toolButton.SetActive(false);
                    m_customIconButton.SetActive(false);
                    break;
                case ToolboxButtonType.Custom:
                    m_toolboxButton.SetActive(false);
                    m_toolButton.SetActive(false);
                    m_customIconButton.SetActive(true);
                    break;
            }
        }
    }
}