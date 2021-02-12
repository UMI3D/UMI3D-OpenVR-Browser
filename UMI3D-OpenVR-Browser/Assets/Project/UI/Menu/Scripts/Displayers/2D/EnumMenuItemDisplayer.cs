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
using UnityEngine.UI;
using umi3d.cdk.menu;
using umi3d.cdk.menu.view;

public class EnumMenuItemDisplayer : AbstractDropDownInputDisplayer
{
    public Text label;
    public int displayedAmount = 4;
    public GameObject itemTemplate;
    public GameObject viewport;
    public Vector3 minPos;
    public Vector3 maxPos;

    public Color selectedColor;
    public Color unselectedColor;

    private bool isDisplayed = false;
    private List<GameObject> itemDisplayers = new List<GameObject>();
    private int cursor = 0;

    class EnumItem
    {
        public Image image;
        public Text text;

        public Color selectedColor;
        public Color unSelectedColor;

        public void Select()
        {
            image.color = selectedColor;
            text.color = selectedColor;
        }

        public void UnSelect()
        {
            image.color = unSelectedColor;
            text.color = unSelectedColor;
        }
    }

    EnumItem currentSelectedChoice;

    public override void Display(bool forceUpdate = false)
    {
        if (isDisplayed && !forceUpdate)
            return;

        if (!isDisplayed)
        {
            foreach(string s in menuItem.options)
            {
                GameObject item = Instantiate(itemTemplate, viewport.transform);
                item.SetActive(true);
                Text text = item.GetComponentInChildren<Text>();
                text.text = s;
                item.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    if (s != menuItem.GetValue())
                    {
                        menuItem.NotifyValueChange(s);

                        currentSelectedChoice?.UnSelect();
                        currentSelectedChoice = new EnumItem { image = item.GetComponentInChildren<Image>(), text = text, selectedColor = selectedColor, unSelectedColor = unselectedColor };
                        currentSelectedChoice.Select();
                    }
                }); 

                itemDisplayers.Add(item);

                if (s == menuItem.GetValue())
                {
                    currentSelectedChoice = new EnumItem { image = item.GetComponentInChildren<Image>(), text = text, selectedColor = selectedColor, unSelectedColor = unselectedColor };
                    currentSelectedChoice.Select();
                }
            }
        }

        label.text = menuItem.Name;
        ArrangeDisplayers();
        isDisplayed = true;
    }


    public void Next()
    {
        cursor++;
        cursor = Mathf.Min(cursor, itemDisplayers.Count - displayedAmount);
        ArrangeDisplayers();
    }

    public void Previous()
    {
        cursor--;
        cursor = Mathf.Max(cursor, 0);
        ArrangeDisplayers();
    }

    private void ArrangeDisplayers()
    {
        for (int i = 0; i < itemDisplayers.Count; i++)
        {
            GameObject item = itemDisplayers[i];
            if (i < cursor)
            {
                //left
                item.SetActive(false);
            }
            else if (i <= cursor + displayedAmount)
            {
                //middle
                item.SetActive(true);
                item.transform.localPosition = Vector3.Lerp(minPos, maxPos, Mathf.InverseLerp(cursor, cursor + displayedAmount, i));
            }
            else
            {
                //right
                item.SetActive(false);
            }
        }
    }


    public override void Hide()
    {
        foreach (GameObject g in itemDisplayers) Destroy(g);
        itemDisplayers.Clear();
        this.gameObject.SetActive(false);
        isDisplayed = false;
    }

    public override int IsSuitableFor(AbstractMenuItem menu)
    {
        return (menu is AbstractEnumInputMenuItem<string>) ? 2 : 0;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(viewport.transform.TransformPoint(minPos), viewport.transform.TransformPoint(maxPos));
    }
}
