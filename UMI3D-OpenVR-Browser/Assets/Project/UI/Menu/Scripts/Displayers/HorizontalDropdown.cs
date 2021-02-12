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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// This class display an horizontal dropdown
/// </summary>
public class HorizontalDropdown : MonoBehaviour
{
    #region Field

    [Header("Container")]
    /// <summary>
    /// Each choice will be dispayed with this template.
    /// It must contain an Image and a text;
    /// </summary>
    public GameObject dropdownChoiceItemTemplate;

    /// <summary>
    /// Where the choice template will be instantiated
    /// </summary>
    public GameObject choicesContainer;

    [Header("Navigation buttons")]
    /// <summary>
    /// Button to navigate left.
    /// </summary>
    public Button leftButton;

    /// <summary>
    /// Button to navigate right.
    /// </summary>
    public Button rightButton;

    public Sprite navigationLeftButtonEnable;
    public Sprite navigationLeftButtonDisable;

    public Sprite navigationRightButtonEnable;
    public Sprite navigationRightButtonDisable;

    [Header("Colors")]
    /// <summary>
    /// Color for the background selected gameobject.
    /// </summary>
    public Color selectedBackgroundColor;

    /// <summary>
    /// /// <summary>
    /// Color for the font of the selected gameobject.
    /// </summary>
    public Color selectedFontColor;

    /// <summary>
    /// Color for the background unselected gameobjects.
    /// </summary>
    public Color unselectedBackgroundColor;

    /// <summary>
    /// /// <summary>
    /// Color for the font of the unselected gameobjects.
    /// </summary>
    public Color unselectedFontColor;

    [Header("Other parameters")]

    public List<string> choices = new List<string>();
    List<DropdownItem> choiceItems = new List<DropdownItem>();

    /// <summary>
    /// Number of choices displayed at the same time.
    /// </summary>
    public int nbChoicesDisplayed = 4;

    /// <summary>
    /// Index of the current item displayed at the left.
    /// </summary>
    public int indexOfLeftItemDisplayed { get; private set; } = 0;

    /// <summary>
    /// Gameobject associated to the current value.
    /// </summary>
    DropdownItem currentChoice = null;

    bool wasSet = false;

    private string _value;
    /// <summary>
    /// Current value.
    /// </summary>
    public string value
    {
        get
        {
            return _value;
        }
        set
        {
            if (((value != _value) || wasSet) && !string.IsNullOrEmpty(value))
            {
                _value = value;
                
                if (currentChoice != null)
                {
                    currentChoice.label.color = unselectedFontColor;
                    currentChoice.backgroundImage.color = unselectedBackgroundColor;
                }
                currentChoice = choiceItems.Find(i => i.label.text == _value);
                if (currentChoice == null)
                    throw new DropdownValueDoesNotExist(value + " is not part of the choices set with HorizontalDropdown.SetChoices()");

                currentChoice.label.color = selectedFontColor;
                currentChoice.backgroundImage.color = selectedBackgroundColor;

                DisplayItem(choiceItems.IndexOf(currentChoice));

                if (wasSet)
                {
                    wasSet = false;
                }
                else
                {
                    onValueChanged?.Invoke(_value);
                }
               
            }

        }
    }

    /// <summary>
    /// Event raised when value changed.
    /// </summary>

    public StringEvent onValueChanged = new StringEvent();

    [Serializable]
    public class StringEvent : UnityEvent<string> { }

    #endregion

    #region Private class

    private class DropdownItem
    {
        public Image backgroundImage;
        public Text label;
        GameObject item;

        public DropdownItem(GameObject item, string choice)
        {
            this.item = item;
            backgroundImage = item.GetComponent<Image>();
            label = item.GetComponentInChildren<Text>();

            if (label == null || backgroundImage == null)
            {
                throw new ArgumentException(item.name + " must contain an Image and a Text");
            }
            label.text = choice;
        }

        public void Display()
        {
            item.SetActive(true);
        }

        public void Hide()
        {
            item.SetActive(false);
        }

        public void Remove()
        {
            Destroy(item);
        }
    }

    private class DropdownValueDoesNotExist : Exception
    {
        public DropdownValueDoesNotExist(string msg) : base(msg)
        {

        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Sets the different choices of the dropdown (clear the previous choices).
    /// </summary>
    /// <param name="choices"></param>
    public void SetChoices(List<string> choices)
    {
        if (choices.Count == 0)
            return;

        this.choices = new List<string>(choices);
        choiceItems.Clear();

        foreach (Transform t in choicesContainer.transform)
            Destroy(t.gameObject);

        indexOfLeftItemDisplayed = 0;

        //1.Create a item for each choice
        foreach (var choice in this.choices)
        {
            var itemObject = Instantiate(dropdownChoiceItemTemplate, choicesContainer.transform);
            DropdownItem item = new DropdownItem(itemObject, choice);
            item.label.text = choice;
            choiceItems.Add(item);

            if (choices.Count > nbChoicesDisplayed)
            {
                item.Hide();
            }
        }

        wasSet = true;

        //2.Select first choice;
        value = choices[0];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="indexOfElement">index of the item which must be visible</param>
    private void DisplayItem(int indexOfElement)
    {
        if (indexOfElement < indexOfLeftItemDisplayed)
        {
            int offset = indexOfLeftItemDisplayed - indexOfElement;
            indexOfLeftItemDisplayed -= offset;
        } else if (indexOfElement >= indexOfLeftItemDisplayed + nbChoicesDisplayed)
        {
            int offset = indexOfElement - indexOfLeftItemDisplayed;
            indexOfLeftItemDisplayed += offset;
        }

        indexOfLeftItemDisplayed = Mathf.Clamp(indexOfLeftItemDisplayed, 0, choiceItems.Count - nbChoicesDisplayed);

        for (int i = 0; i < choiceItems.Count; i++)
        {
            if (i < indexOfLeftItemDisplayed || i >= indexOfLeftItemDisplayed + nbChoicesDisplayed)
            {
                choiceItems[i].Hide();
            } else
            {
                choiceItems[i].Display();
            }
        }

        if (indexOfLeftItemDisplayed == 0)
        {
            leftButton.GetComponent<Image>().sprite = navigationLeftButtonDisable;
        } else
        {
            leftButton.GetComponent<Image>().sprite = navigationLeftButtonEnable;
        }

        if (indexOfLeftItemDisplayed + nbChoicesDisplayed >= choices.Count)
        {
            rightButton.GetComponent<Image>().sprite = navigationRightButtonDisable;
        }                                                       
        else                                                     
        {                                                        
            rightButton.GetComponent<Image>().sprite = navigationRightButtonEnable;
        }

    }

    public void NavigateLeft()
    {
        if (indexOfLeftItemDisplayed > 0)
            DisplayItem(indexOfLeftItemDisplayed - 1);
    }

    public void NavigateRight()
    {
        if (indexOfLeftItemDisplayed + nbChoicesDisplayed < choices.Count)
            DisplayItem(indexOfLeftItemDisplayed + 1);
    }

    /// <summary>
    /// Select the choice just after the current choice selected (if exists).
    /// </summary>
    public void SelectNextItem()
    {
        if (currentChoice == choiceItems.Last())
            return;
        else
        {
            value = choices[choices.IndexOf(value) + 1];
        }
    }

    /// <summary>
    /// Select the choice just before the current choice selected (if exists).
    /// </summary>
    public void SelectPreviousItem()
    {
        if (currentChoice == choiceItems.First())
            return;
        else
        {
            value = choices[choices.IndexOf(value) - 1];
        }
    }


    #endregion
}
