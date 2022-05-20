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
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.ui.playerMenu
{
    public partial class ScrollView
    {
        private enum ScrollDirection { Vertical, Horizontal, Both }

        [SerializeField]
        private ScrollRect m_scrollRect = null;
        [SerializeField]
        private Scrollbar m_verticalScrollBar = null;
        [SerializeField]
        private Scrollbar m_horizontalScrollBar = null;
        [SerializeField]
        private Button m_previousVertical = null;
        [SerializeField]
        private Button m_nextVertical = null;
        [SerializeField]
        private Button m_previousHorizontal = null;
        [SerializeField]
        private Button m_nextHorizontal = null;

        private ScrollDirection m_scrollDirection
        {
            get
            {
                if (m_scrollRect.horizontal && m_scrollRect.vertical) return ScrollDirection.Both;
                if (m_scrollRect.horizontal) return ScrollDirection.Horizontal;
                else return ScrollDirection.Vertical;
            }
        }
        private bool m_isVerticalScrollable
        {
            get
            {
                if (m_scrollDirection == ScrollDirection.Horizontal) return false;
                return m_verticalScrollBar.size < 1f;
            }
        }
        private bool m_isHorizontalScrollable
        {
            get
            {
                if (m_scrollDirection == ScrollDirection.Vertical) return false;
                return m_horizontalScrollBar.size < 1f;
            }
        }
    }

    public partial class ScrollView
    {
        public void VerticalIncrement() => Increment(m_verticalScrollBar);
        public void VerticalDecrement() => Decrement(m_verticalScrollBar);
        public void HorizontalIncrement() => Increment(m_horizontalScrollBar);
        public void HorizontalDecrement() => Decrement(m_horizontalScrollBar);
        public void UpdateStatus() => StartCoroutine(UpdateStatusImpl());
        public void OnPreviousVerticalButtonClicked() => OnVerticalHorizontalButtonClicked(VerticalDecrement);
        public void OnNextVerticalButtonClicked() => OnVerticalHorizontalButtonClicked(VerticalIncrement);
        public void OnPreviousHorizontalButtonClicked() => OnVerticalHorizontalButtonClicked(HorizontalDecrement);
        public void OnNextHorizontalButtonClicked() => OnVerticalHorizontalButtonClicked(HorizontalIncrement);
    }

    public partial class ScrollView
    {
        private void Increment(Scrollbar scrollbar)
        {
            scrollbar.value = Mathf.Clamp(scrollbar.value + m_scrollRect.scrollSensitivity / 10f, 0f, 1f);
        }
        private void Decrement(Scrollbar scrollbar)
        {
            scrollbar.value = Mathf.Clamp(scrollbar.value - m_scrollRect.scrollSensitivity / 10f, 0f, 1f);
        }

        private void UpdateVerticalOrHorizontalStatus(bool isScrollable, Button previousButton, Button nextButton, float normalizedPosition)
        {
            GameObject previous = previousButton.transform.GetChild(0).gameObject;
            GameObject next = nextButton.transform.GetChild(0).gameObject;
            if (!isScrollable)
            {
                previousButton.enabled = false;
                nextButton.enabled = false;
                previous.gameObject.SetActive(false);
                next.gameObject.SetActive(false);
            }
            else if (normalizedPosition < 0.01f)
            {
                previousButton.enabled = false;
                nextButton.enabled = true;
                previous.gameObject.SetActive(false);
                next.gameObject.SetActive(true);
            }
            else if (Mathf.Approximately(normalizedPosition, 1f))
            {
                previousButton.enabled = true;
                nextButton.enabled = false;
                previous.gameObject.SetActive(true);
                next.gameObject.SetActive(false);
            }
            else
            {
                previousButton.enabled = true;
                nextButton.enabled = true;
                previous.gameObject.SetActive(true);
                next.gameObject.SetActive(true);
            }
        }

        private void UpdateButtonStatus()
        {
            Vector2 normalizedPosition = m_scrollRect.normalizedPosition;
            switch (m_scrollDirection)
            {
                case ScrollDirection.Vertical:
                    UpdateVerticalOrHorizontalStatus(m_isVerticalScrollable, m_previousVertical, m_nextHorizontal, normalizedPosition.y);
                    break;
                case ScrollDirection.Horizontal:
                    UpdateVerticalOrHorizontalStatus(m_isHorizontalScrollable, m_previousHorizontal, m_nextHorizontal, normalizedPosition.x);
                    break;
                case ScrollDirection.Both:
                    UpdateVerticalOrHorizontalStatus(m_isVerticalScrollable, m_previousVertical, m_nextHorizontal, normalizedPosition.y);
                    UpdateVerticalOrHorizontalStatus(m_isHorizontalScrollable, m_previousHorizontal, m_nextHorizontal, normalizedPosition.x);
                    break;
            }
        }
        private IEnumerator UpdateStatusImpl()
        {
            yield return null;
            UpdateButtonStatus();
        }

        private void OnVerticalHorizontalButtonClicked(Action action)
        {
            action();
            UpdateStatus();
        }
    }

    public partial class ScrollView : MonoBehaviour
    {
        private void Start()
        {
            UpdateStatus();
        }
    }
}