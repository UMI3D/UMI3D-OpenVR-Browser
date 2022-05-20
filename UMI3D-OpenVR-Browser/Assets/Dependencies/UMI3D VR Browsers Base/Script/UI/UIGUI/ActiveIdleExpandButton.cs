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
    public partial class ActiveIdleExpandButton
    {
        public bool IsExpand { get; private set; } = false;
        public bool IsActive { get; private set; } = false;
        public UnityEvent StateChanged { get; private set; } = new UnityEvent();
        public UnityEvent OnClicked { get => onClicked; }
    }

    public partial class ActiveIdleExpandButton
    {
        [SerializeField]
        private GameObject m_activeCollapsed;
        [SerializeField]
        private GameObject m_activeExpand;
        [SerializeField]
        private GameObject m_idleCollapsed;
        [SerializeField]
        private GameObject m_idleExpand;

        [SerializeField]
        [Tooltip("Event raised when this element is clicked")]
        private UnityEvent onClicked = new UnityEvent();
    }

    public partial class ActiveIdleExpandButton : MonoBehaviour
    {
        public void Expand(bool value)
        {
            if (IsExpand == value) return;
            IsExpand = value;
            m_activeCollapsed.SetActive((IsActive) ? !value : false);
            m_activeExpand.SetActive((IsActive) ? value : false);
            m_idleCollapsed.SetActive((IsActive) ? false : !value);
            m_idleExpand.SetActive((IsActive) ? false : value);
            StateChanged.Invoke();
        }

        public void SetActive(bool value)
        {
            if (IsActive == value) return;
            IsActive = value;
            m_activeCollapsed.SetActive((IsExpand) ? false : value);
            m_activeExpand.SetActive((IsExpand) ? value : false);
            m_idleCollapsed.SetActive((IsExpand) ? false : !value);
            m_idleExpand.SetActive((IsExpand) ? !value : false);
            StateChanged.Invoke();
        }

        public void Toggle(bool value)
        {
            //IsOn = value;
            //m_onButton.SetActive(IsOn ? true : false);
            //m_offButton.SetActive(IsOn ? false : true);
        }

        public void Clicked()
            => OnClicked.Invoke();
    }
}

