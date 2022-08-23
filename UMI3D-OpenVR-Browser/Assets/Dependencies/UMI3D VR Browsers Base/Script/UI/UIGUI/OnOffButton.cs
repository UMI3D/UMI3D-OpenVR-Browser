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
using UnityEngine.Events;

namespace umi3dVRBrowsersBase.ui.playerMenu
{
    public class OnOffButton : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_onButton = null;
        [SerializeField]
        private GameObject m_offButton = null;

        public bool IsOn { get; private set; }

        [SerializeField]
        [Tooltip("Event raised when this element is clicked")]
        private UnityEvent onClicked = new UnityEvent();

        public UnityEvent OnClicked { get => onClicked; }

        public void Toggle(bool value)
        {
            IsOn = value;
            m_onButton.SetActive(IsOn ? true : false);
            m_offButton.SetActive(IsOn ? false : true);
        }

        public void Toggle()
        {
            Toggle(!IsOn);
        }

        public void Clicked()
            => OnClicked.Invoke();
    }
}

