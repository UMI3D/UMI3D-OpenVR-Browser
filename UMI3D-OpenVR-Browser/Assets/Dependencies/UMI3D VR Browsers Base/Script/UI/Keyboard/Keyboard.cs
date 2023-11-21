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
using TMPro;
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
        private TMP_InputField previewField;

        private InputField editedField;

        /// <summary>
        /// Relevant if <see cref="previewField"/> is not set by default.
        /// </summary>
        [SerializeField]
        private Text editedText;

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

        [Serializable]
        public class StringEvent : UnityEvent<String>
        { }

        [Space]

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

        #endregion Fields

        #region Event

        [Space]

        /// <summary>
        /// Event sent each time a key is entered.
        /// </summary>
        public StringEvent onCharacterEntered = new();

        /// <summary>
        /// Delete key is pressed.
        /// </summary>
        public UnityEvent onDeleteCharacter = new();

        /// <summary>
        /// Enter key is pressed.
        /// </summary>
        public UnityEvent onEnterCharacter = new();

        #endregion

        #region Methods

        private void Awake()
        {
            if (isSingleton) Instance = this;
        }

        private void Start()
        {
            foreach (KeyboardLetter letter in symbolsRoot.GetComponentsInChildren<KeyboardLetter>())
            {
                letter.onPressed.AddListener(() =>
                {
                    OnCharacterAdded(isUpperCase ? letter.symbol.ToUpper() : letter.symbol.ToLower());
                    letter.button.OnPointerUp(new PointerEventData(EventSystem.current));


                });
                letter.onHoverEnter.AddListener(() => hoverSource.Play());
            }

            upperKey.onPressed.AddListener(ToggleUpperCase);
            letterToSymbolKey.onPressed.AddListener(SwitchToSymbols);
            symbolToLetterKey.onPressed.AddListener(SwitchToLetters);
            deleteKey.onPressed.AddListener(Delete);
            deleteKey.onPressed.AddListener(() => onDeleteCharacter?.Invoke());
            enterKey.onPressed.AddListener(ClosePopUp);
            enterKey.onPressed.AddListener(() => onEnterCharacter?.Invoke());
            SwitchToLetters();

            SetPreviousInputField();
        }

        /// <summary>
        /// Hide mobile and sy system keyboard  caret.
        /// </summary>
        private void SetPreviousInputField()
        {
            previewField.shouldHideMobileInput = true;
            previewField.shouldHideSoftKeyboard = true;

            previewField.customCaretColor = true;
            previewField.caretColor = Color.red;
            previewField.caretWidth = 3;
        }

        private void SetEditedInputField()
        {
            if (editedField != null)
                editedField.shouldHideMobileInput = true;
        }

        private void BindEditedField(InputField inputField)
        {
            if (editedField != null)
                editedField = inputField;
            SetEditedInputField();
        }

        #region Character Management

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

        #endregion

        /// <summary>
        /// Adds a character to the current text.
        /// </summary>
        /// <param name="character"></param>
        private void OnCharacterAdded(string character)
        {
            int previousCaretPosition = previewField.caretPosition;

            if (IsTextFullySelected())
            {
                previewField.text = character;
                SetCaret(previewField, 1);
            }
            else if (previousCaretPosition != previewField.text.Length)
            {
                previewField.text = previewField.text.Substring(0, previousCaretPosition) + character + previewField.text.Substring(previousCaretPosition, previewField.text.Length - previousCaretPosition);
                SetCaret(previewField, previousCaretPosition + 1);
            }
            else
            {
                previewField.text += character;
                SetCaret(previewField, previousCaretPosition + 1);
            }

            OnValueChanged.Invoke(previewField.text);
            if (editedField != null)
            {
                editedField.text = previewField.text;
                editedField.ForceLabelUpdate();
            }

            pressSource.Play();

            onCharacterEntered?.Invoke(character);
        }

        /// <summary>
        /// Deletes the last character or the full text if it is selected.
        /// </summary>
        private void Delete()
        {
            int length = previewField.text.Length;
            int carretPosition = previewField.caretPosition;

            if (length > 0)
            {
                if (IsTextFullySelected())
                {
                    previewField.text = string.Empty;
                    StartCoroutine(SetCarretInInputField(previewField, 0));
                }
                else if (carretPosition != length && carretPosition > 0)
                {
                    previewField.text = previewField.text.Substring(0, carretPosition - 1) + previewField.text.Substring(carretPosition, length - carretPosition);
                    StartCoroutine(SetCarretInInputField(previewField, Mathf.Clamp(carretPosition - 1, 0, length)));
                }
                else if (carretPosition > 0)
                {
                    previewField.text = previewField.text.Substring(0, length - 1);
                    StartCoroutine(SetCarretInInputField(previewField, carretPosition - 1));
                }
                else
                    return;

                OnValueChanged.Invoke(previewField.text);

                if (editedField != null)
                {
                    editedField.text = previewField.text;
                    editedField.ForceLabelUpdate();
                }
            }
        }

        /// <summary>
        /// Dispays the keyboard with a base <see cref="InputField"/> displayed at the same time.
        /// </summary>
        /// <param name="inputField"></param>
        /// <param name="onEditFinished"></param>
        public void OpenKeyboard(InputField inputField, Action<string> onEditFinished)
        {
            BindEditedField(inputField);

            OpenKeyboard(inputField.text, onEditFinished, null);
        }

        /// <summary>
        /// Opens the keyboard to edit a text input.
        /// </summary>
        /// <param name="editedText"></param>
        /// <param name="onEditFinished">Callback called when the edition ends, string parameter is the final value entered by users.</param>
        public void OpenKeyboard(string editedText, Action<string> onEditFinished, Action onEditCanceled)
        {
            if (WasClosedLastFrame) return;

            root.SetActive(true);

            previewField.ActivateInputField();
            previewField.text = editedText;

            this.onEditFinished = onEditFinished;
            this.onEditCanceled = onEditCanceled;

            IsOpen = true;
        }

        /// <summary>
        /// Open the keyboard simply, just to listen to key inputs.
        /// </summary>
        public void OpenKeyboard()
        {
            if (WasClosedLastFrame) return;

            root.SetActive(true);

            previewField.text = string.Empty;

            this.onEditFinished = null;
            this.onEditCanceled = null;

            IsOpen = true;
        }

        /// <summary>
        /// Opens the keyboard at a certain position to edit a text input
        /// </summary>
        /// <param name="editedText"></param>
        /// <param name="onEditFinished"></param>
        /// <param name="position"></param>
        /// <param name="normal"></param>
        public void OpenKeyboard(string editedText, Action<string> onEditFinished, Action onEditCanceled, Vector3 position, Vector3 normal)
        {
            transform.SetPositionAndRotation(position, Quaternion.LookRotation(normal, Vector3.up));

            OpenKeyboard(editedText, onEditFinished, onEditCanceled);
        }

        /// <summary>
        /// Selects an <see cref="InputField"/> once it is possible.
        /// </summary>
        /// <returns></returns>
        private IEnumerator WaitAndSelectField()
        {
            while (EventSystem.current.alreadySelecting) yield return null;

            var navMode = new Navigation() { mode = Navigation.Mode.None };
            editedField.navigation = navMode;

            previewField.Select();
        }

        private IEnumerator SetCarretInInputField(TMP_InputField field, int position)
        {
            yield return new WaitForEndOfFrame();

            previewField.selectionFocusPosition = position;
            previewField.selectionAnchorPosition = position;
            field.caretPosition = position;
            field.ForceLabelUpdate();
        }

        private void SetCaret(TMP_InputField field, int position)
        {
            field.caretPosition = position;
            field.ForceLabelUpdate();
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
            root.SetActive(false);
            setCaretPosition = false;

            WasClosedLastFrame = true;
            StartCoroutine(ResetWasClosedLastFrame());
        }

        /// <summary>
        /// Coroutine to reset <see cref="WasClosedLastFrame"/> at the end of the next frame.
        /// </summary>
        /// <returns></returns>
        private IEnumerator ResetWasClosedLastFrame()
        {
            yield return null;

            IsOpen = false;

            yield return new WaitForEndOfFrame();

            WasClosedLastFrame = false;
        }

        /// <summary>
        /// Is the edited text fully selected by a user ?
        /// </summary>
        /// <returns></returns>
        private bool IsTextFullySelected() => previewField.selectionAnchorPosition == previewField.text.Length && previewField.selectionFocusPosition == 0;

        /// <summary>
        /// Sets preview field to empty text
        /// </summary>
        public void ResetPreviewField()
        {
            previewField.text = string.Empty;
        }
        #endregion Methods
    }
}