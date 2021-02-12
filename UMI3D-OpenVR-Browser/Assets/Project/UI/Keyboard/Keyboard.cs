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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Keyboard : MonoBehaviour
{
    public bool isUppercase = false;
    public bool isLetterKeyboard = true;
    public GameObject letters;
    public GameObject symbols;
    public GameObject numbers;
    public GameObject capsLockKey;
    public GameObject switchKey;

    public EventSystem eventSystem;


    // Use this for initialization
    void Start()
    {
        Text switchText = switchKey.GetComponentInChildren<Text>();
        switchText.text = isLetterKeyboard == true ? "?$=" : "ABC";
        symbols.SetActive(!isLetterKeyboard);
        letters.SetActive(isLetterKeyboard);
        capsLockKey.SetActive(isLetterKeyboard);

        if (isUppercase == true)
        {
            Image image = capsLockKey.GetComponentInChildren<Image>();
            image.color = new Color32(65, 105, 225, 255);
        }

        Button[] letterKeys = letters.GetComponentsInChildren<Button>();
        foreach (Button key in letterKeys)
        {
            Text letterText = key.GetComponentInChildren<Text>();
            key.onClick.AddListener(() =>
            {
                AddText(letterText.text);
            });

            if (isUppercase == true)
            {
                letterText.text = letterText.text.ToUpper();
            }
        }

        Button[] symbolKeys = symbols.GetComponentsInChildren<Button>();
        foreach (Button key in symbolKeys)
        {
            Text symbolText = key.GetComponentInChildren<Text>();
            key.onClick.AddListener(() =>
            {
                AddText(symbolText.text);
            });
        }

        Button[] numberKeys = numbers.GetComponentsInChildren<Button>();
        foreach (Button key in numberKeys)
        {
            Text numberText = key.GetComponentInChildren<Text>();
            key.onClick.AddListener(() =>
            {
                AddText(numberText.text);
            });
        }
    }

    public void AddText(string text)
    {        
        if (InputFieldKeyboardSelector.Instance.lastInputField != null)
        {
            InputFieldKeyboardSelector.Instance.lastInputField.text += text;
            InputFieldKeyboardSelector.Instance.lastInputField.caretPosition = InputFieldKeyboardSelector.Instance.lastInputField.text.Length;
        }
        
    }

    public void Clear()
    {

        if (InputFieldKeyboardSelector.Instance.lastInputField != null)
        {
            InputFieldKeyboardSelector.Instance.lastInputField.text = "";
            InputFieldKeyboardSelector.Instance.lastInputField.caretPosition = 0;
        }
        
    }

    public void Backspace()
    {
        if ((InputFieldKeyboardSelector.Instance.lastInputField != null) &&  (InputFieldKeyboardSelector.Instance.lastInputField.text.Length > 0))
        {
            InputFieldKeyboardSelector.Instance.lastInputField.text = InputFieldKeyboardSelector.Instance.lastInputField.text.Remove(InputFieldKeyboardSelector.Instance.lastInputField.text.Length - 1);
        }        
    }

    public void Newline()
    {        
        if (InputFieldKeyboardSelector.Instance.lastInputField != null)
        {
            InputFieldKeyboardSelector.Instance.lastInputField.text += "\n";
            InputFieldKeyboardSelector.Instance.lastInputField.caretPosition++;
        }        
    }

    public void CapsLock()
    {
        isUppercase = !isUppercase;
        Image image = capsLockKey.GetComponentInChildren<Image>();
        image.color = isUppercase == true ? new Color32(65, 105, 225, 255) : new Color32(48, 47, 55, 255);

        Button[] keys = letters.GetComponentsInChildren<Button>();
        foreach (Button key in keys)
        {
            Text textObject = key.GetComponentInChildren<Text>();

            if (isUppercase == true)
            {
                textObject.text = textObject.text.ToUpper();
            }
            else
            {
                textObject.text = textObject.text.ToLower();
            }
        }
    }

    // Switch between letters and symbols
    public void Switch()
    {
        isLetterKeyboard = !isLetterKeyboard;
        Text textObject = switchKey.GetComponentInChildren<Text>();
        textObject.text = isLetterKeyboard == true ? "?$=" : "ABC";
        symbols.SetActive(!isLetterKeyboard);
        capsLockKey.SetActive(isLetterKeyboard);
        letters.SetActive(isLetterKeyboard);
    }
}
