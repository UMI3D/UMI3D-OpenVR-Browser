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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.tutorial
{
    /// <summary>
    /// Display a pop up for users.
    /// </summary>
    public class TutorialDialogueBox : MonoBehaviour
    {
        #region Fields

        [SerializeField]
        [Tooltip("Label to display current line content")]
        private Text text;

        [SerializeField]
        [Tooltip("Button to fgo to the next line")]
        private Button nextButton;

        [SerializeField]
        [Tooltip("Text associated to nextButton")]
        private Text textButton;

        [SerializeField]
        [Tooltip("If a line as an image, its caption")]
        private Text imageDescription;

        [SerializeField]
        [Tooltip("If line has an image, where it is displayed")]
        private Image image;

        [SerializeField]
        [Tooltip("All dialogue lines")]
        private List<DialogueLine> lines;

        /// <summary>
        /// Current line displayed.
        /// </summary>
        private int currentLine = -1;

        #endregion

        #region Methods

        private void Start()
        {
            nextButton.onClick.AddListener(Next);
            Next();
        }

        /// <summary>
        /// Moves to the next dialogue line.
        /// </summary>
        private void Next()
        {
            if (currentLine >= 0 && currentLine < lines.Count)
                lines[currentLine].onValidLine?.Invoke();

            if (currentLine < lines.Count - 1)
            {
                currentLine++;
                DialogueLine line = lines[currentLine];

                bool hasImage = (line.image != null && line.image != default);

                if (hasImage)
                {
                    imageDescription.text = line.text;
                    image.sprite = line.image;
                }
                else
                {
                    text.text = line.text;
                }

                text.gameObject.SetActive(!hasImage);
                imageDescription.gameObject.SetActive(hasImage);
                image.gameObject.SetActive(hasImage);

                textButton.text = line.nextButtonText;
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        #endregion
    }

    /// <summary>
    /// Stores all data about a dialogue line.
    /// </summary>
    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(3, 10)]
        public string text;

        public string nextButtonText = "Next";

        public UnityEvent onValidLine;

        public Sprite image;
    }
}
