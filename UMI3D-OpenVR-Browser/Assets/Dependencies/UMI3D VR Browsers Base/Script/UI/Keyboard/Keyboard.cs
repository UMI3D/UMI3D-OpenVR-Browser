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

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.ui.keyboard
{
    /// <summary>
    /// 3D Keyboard to edit textfields (such as <see cref="CustomInputWithKeyboard"/> and <see cref="CustomInputWithKeyboardEnvironment"/>.
    /// </summary>
    public class Keyboard : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// Reference to singleton if this instance has <see cref="isSingleton"/> checks.
        /// </summary>
        public static Keyboard Instance;

        [SerializeField]
        [Tooltip("Is this instance a singleton ?")]
        private bool isSingleton = false;

        /// <summary>
        /// Is the keyboard displayed ?
        /// </summary>
        public bool IsOpen { get; private set; }

        [SerializeField]
        [Tooltip("Main gameobject of the keyboard")]
        private GameObject root;

        [SerializeField]
        [Tooltip("Audio source to play a key pressed feedback")]
        private AudioSource pressSource;

        [SerializeField]
        [Tooltip("Audio source to play a key hover feedback")]
        private AudioSource hoverSource;

        [SerializeField]
        [Tooltip("Input to display a preview of what users write")]
        private InputField previewField;

        /// <summary>
        /// Relevant if <see cref="previewField"/> is not set by default.
        /// </summary>
        [SerializeField]
        private Text previewText;

        [Header("Keys")]

        [SerializeField]
        [Tooltip("Root of of symbol key")]
        private GameObject symbolsRoot;

        [SerializeField]
        [Tooltip("Root of all letter keys")]
        private GameObject lettersRoot;

        [SerializeField]
        [Tooltip("Root of all lower letter keys")]
        private GameObject lettersLowerRoot;

        [SerializeField]
        [Tooltip("Root of all upper letter keys")]
        private GameObject lettersUpperRoot;

        [SerializeField]
        [Tooltip("Root of all only symbol keys")]
        private GameObject realSymbolsRoot;

        [SerializeField]
        [Tooltip("Key to swith to upper letter")]
        private KeyboardKey upperKey;

        [SerializeField]
        [Tooltip("Key to switch from letter to symbol keys")]
        private KeyboardKey letterToSymbolKey;

        [SerializeField]
        [Tooltip("Key to switch from symbol to letter keys")]
        private KeyboardKey symbolToLetterKey;

        [SerializeField]
        [Tooltip("Key to delete characters")]
        private KeyboardKey deleteKey;

        [SerializeField]
        [Tooltip("Confirmation key")]
        private KeyboardKey enterKey;

        public class StringEvent : UnityEvent<String> { }

        /// <summary>
        /// Event triggered when edited text changes.
        /// </summary>
        public StringEvent OnValueChanged = new StringEvent();

        /// <summary>
        /// Is the keyboard displaying upper letters ?
        /// </summary>
        private bool isUpperCase = false;

        /// <summary>
        /// Should the caret position be updated ?
        /// </summary>
        private bool setCaretPosition = false;

        /// <summary>
        /// Defines what to do when users confirm their edition
        /// </summary>
        private Action<string> onEditFinished;

        /// <summary>
        /// Defines what to do when users cancel their edition
        /// </summary>
        private Action onEditCanceled;

        /// <summary>
        /// Was the keyboard just closed last frame ?
        /// </summary>
        public bool WasClosedLastFrame { get; private set; } = false;

        #endregion

        #region Methods

        private void Awake()
        {
            if (isSingleton)
            {
                Instance = this;
            }
        }

        private void Start()
        {
            foreach (KeyboardLetter letter in symbolsRoot.GetComponentsInChildren<KeyboardLetter>())
            {
                letter.onPressed.AddListener(() =>
                {
                    OnCharacterAdded(isUpperCase ? letter.symbol.ToUpper() : letter.symbol.ToLower());
                });
                letter.onHoverEnter.AddListener(() => hoverSource.Play());
            }

            upperKey.onPressed.AddListener(ToggleUpperCase);
            letterToSymbolKey.onPressed.AddListener(SwitchToSymbols);
            symbolToLetterKey.onPressed.AddListener(SwitchToLetters);
            deleteKey.onPressed.AddListener(Delete);
            enterKey.onPressed.AddListener(ClosePopUp);

            SwitchToLetters();
        }

        private void LateUpdate()
        {
            if (setCaretPosition && previewField.gameObject.activeInHierarchy)
            {
                //Setting carret position in late update is required to prevent from some bugs.
                previewField.caretPosition = previewField.text.Length;
                setCaretPosition = false;
            }
        }

        /// <summary>
        /// Switches keyboard to letter layout.
        /// </summary>
        private void SwitchToLetters()
        {
            realSymbolsRoot.SetActive(false);
            lettersRoot.SetActive(true);
            upperKey.gameObject.SetActive(true);

            letterToSymbolKey.gameObject.SetActive(true);
            symbolToLetterKey.gameObject.SetActive(false);

            isUpperCase = false;
            lettersUpperRoot.SetActive(isUpperCase);
            lettersLowerRoot.SetActive(!isUpperCase);
        }

        /// <summary>
        /// Switches keyboard to symbo layout.
        /// </summary>
        private void SwitchToSymbols()
        {
            realSymbolsRoot.SetActive(true);
            lettersRoot.SetActive(false);
            upperKey.gameObject.SetActive(false);

            letterToSymbolKey.gameObject.SetActive(false);
            symbolToLetterKey.gameObject.SetActive(true);
        }

        /// <summary>
        /// Switches to uppercase layout.
        /// </summary>
        private void ToggleUpperCase()
        {
            isUpperCase = !isUpperCase;

            lettersUpperRoot.SetActive(isUpperCase);
            lettersLowerRoot.SetActive(!isUpperCase);
        }

        /// <summary>
        /// Adds a character to the current text.
        /// </summary>
        /// <param name="character"></param>
        private void OnCharacterAdded(string character)
        {
            int carretPosition = previewField.caretPosition;

            if (IsTextFullySelected())
            {
                previewField.text = character;
                previewField.Select();
                setCaretPosition = true;
            }
            else if (carretPosition != previewField.text.Length)
            {
                previewField.text = previewField.text.Substring(0, carretPosition) + character + previewField.text.Substring(carretPosition, previewField.text.Length - carretPosition);
                previewField.Select();
                previewField.caretPosition++;
            }
            else
            {
                previewField.text += character;
                previewField.Select();
                setCaretPosition = true;
            }

            OnValueChanged.Invoke(previewField.text);
            previewText.text = previewField.text;

            pressSource.Play();
        }

        /// <summary>
        /// Deletes the last character or the full text if it is selected.
        /// </summary>
        private void Delete()
        {
            int lenght = previewField.text.Length;
            int carretPosition = previewField.caretPosition;

            if (lenght > 0)
            {
                if (IsTextFullySelected())
                {
                    previewField.text = string.Empty;
                    previewField.Select();
                    setCaretPosition = true;
                }
                else if (carretPosition != lenght)
                {
                    previewField.text = previewField.text.Substring(0, carretPosition - 1) + previewField.text.Substring(carretPosition, lenght - carretPosition);
                    previewField.Select();
                    previewField.caretPosition = Mathf.Clamp(carretPosition - 1, 0, lenght);
                }
                else
                {
                    previewField.text = previewField.text.Substring(0, lenght - 1);
                    previewField.Select();
                    setCaretPosition = true;
                }

                OnValueChanged.Invoke(previewField.text);
                previewText.text = previewField.text;
            }
        }

        /// <summary>
        /// Dispays the keyboard with a base <see cref="InputField"/> displayed at the same time.
        /// </summary>
        /// <param name="inputField"></param>
        /// <param name="onEditFinished"></param>
        public void OpenKeyboard(InputField inputField, Action<string> onEditFinished)
        {
            previewText.gameObject.SetActive(true);
            this.previewText.text = inputField.text;
            this.previewField = inputField;
            OpenKeyboard(inputField.text, onEditFinished, null, false);
        }

        /// <summary>
        /// Opens the keyboard to edit a text input.
        /// </summary>
        /// <param name="startValue"></param>
        /// <param name="onEditFinished">Callback called when the edition ends, string parameter is the final value entered by users.</param>
        public void OpenKeyboard(string startValue, Action<string> onEditFinished, Action onEditCanceled, bool selectPreviewField)
        {
            if (WasClosedLastFrame)
                return;

            root.SetActive(true);

            previewField.text = startValue;

            if (selectPreviewField)
                StartCoroutine(SelectField(previewField));

            this.onEditFinished = onEditFinished;
            this.onEditCanceled = onEditCanceled;

            IsOpen = true;
        }

        /// <summary>
        /// Selects an <see cref="InputField"/> once it is possible.
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        IEnumerator SelectField(InputField field)
        {
            while (EventSystem.current.alreadySelecting)
                yield return null;

            EventSystem.current.SetSelectedGameObject(null, null);
            field.Select();
        }

        /// <summary>
        /// Opens the keyboard at a certain position to edit a text input
        /// </summary>
        /// <param name="startValue"></param>
        /// <param name="onEditFinished"></param>
        /// <param name="position"></param>
        /// <param name="normal"></param>
        public void OpenKeyboard(string startValue, Action<string> onEditFinished, Action onEditCanceled, Vector3 position, Vector3 normal)
        {
            transform.position = position;
            transform.rotation = Quaternion.LookRotation(normal, Vector3.up);
            previewText.gameObject.SetActive(false);

            OpenKeyboard(startValue, onEditFinished, onEditCanceled, true);
        }

        /// <summary>
        /// Closes the keyboard.
        /// </summary>
        private void ClosePopUp()
        {
            Hide();
            onEditFinished?.Invoke(previewField.text);
        }

        /// <summary>
        /// Cancels the edition and closes the keyboard.
        /// </summary>
        public void CancelAndClose()
        {
            Hide();
            onEditCanceled?.Invoke();
        }

        /// <summary>
        /// Hides keyboard.
        /// </summary>
        public void Hide()
        {
            previewText.gameObject.SetActive(false);

            root.SetActive(false);
            setCaretPosition = false;
            IsOpen = false;

            WasClosedLastFrame = true;
            StartCoroutine(ResetWasClosedLastFrame());
        }

        /// <summary>
        /// Coroutine to reset <see cref="WasClosedLastFrame"/> at the end of the next frame.
        /// </summary>
        /// <returns></returns>
        IEnumerator ResetWasClosedLastFrame()
        {
            yield return null;
            yield return new WaitForEndOfFrame();

            WasClosedLastFrame = false;
        }

        /// <summary>
        /// Is the edited text fully selected by a user ?
        /// </summary>
        /// <returns></returns>
        private bool IsTextFullySelected()
        {
            return previewField.selectionAnchorPosition == previewField.text.Length && previewField.selectionFocusPosition == 0;
        }

        #endregion
    }
}