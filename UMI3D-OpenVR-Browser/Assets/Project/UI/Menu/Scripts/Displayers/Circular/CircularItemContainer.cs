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
using umi3d.cdk.menu.view;
using umi3d.cdk.menu;
using UnityEngine.UI;

public class CircularItemContainer : AbstractMenuDisplayContainer, ICircularDisplayer
{
    public Transform viewport;
    public List<GameObject> quarters;
    public PlayerMenuManager.MaterialGroup materialGroup;

    /// <summary>
    /// Label for the menu
    /// </summary>
    public Text circularMenuLabel;

    public Vector3 hoveredScale = new Vector3(1.22f, 1, 1.22f);
    Vector3 normalScale;

    private int currentQuarter = 0;
    public int CurrentQuarter
    {
        get
        {
            return currentQuarter;
        }
        set
        {
            currentQuarter = Mathf.Clamp(value, 0, quarters.Count);
            for (int i = 0; i < quarters.Count; i++)
            {
                quarters[i].SetActive(currentQuarter == i);
            }
        }
    }

    public bool IsContentDisplayed { get; set; } = false;

    public ScreenInputDisplayer screenDisplayer { get; set; }

    private List<AbstractDisplayer> containedElements = new List<AbstractDisplayer>();

    private AbstractMenuDisplayContainer virtualContainer;

    public override AbstractDisplayer this[int i] { get => containedElements[i]; set => containedElements[i] = value; }

    public ScreenInputDisplayer associatedScreenInputDisplayer;

    protected virtual void Awake()
    {
        virtualContainer = this;
    }


    public override void Clear()
    {
        base.Clear();
        foreach (var displayer in containedElements)
        {
            displayer.Clear();
            Destroy(displayer.gameObject);
        }


        containedElements.Clear();

        RemoveAll();
    }

    public override void Collapse(bool forceUpdate = false)
    {
        foreach(AbstractDisplayer disp in virtualContainer)
        {
            disp.Hide();
        }
    }

    public override bool Contains(AbstractDisplayer element)
    {
        return containedElements.Contains(element);
    }

    public override int Count()
    {
        return containedElements.Count;
    }

    public override AbstractMenuDisplayContainer CurrentMenuDisplayContainer()
    {
        return this;
    }

    public override void Display(bool forceUpdate = false)
    {
        this.gameObject.SetActive(true);

        name = menu.Name + " " + menu.GetType();

        if (parent != null && screenDisplayer != null)
            screenDisplayer.playerMenuManager.SetToolboxAndToolLabel(parent?.menu?.Name);

        circularMenuLabel.text = menu.Name;
        isDisplayed = true;
    }

    public override void Expand(bool forceUpdate = false)
    {
        ExpandAs(this, forceUpdate);
    }

    public override void ExpandAs(AbstractMenuDisplayContainer container, bool forceUpdate = false)
    {
        foreach(AbstractDisplayer disp in virtualContainer)
        {
            disp.Hide();
            disp.transform.SetParent(this.transform);
        }
        virtualContainer = container;
        if (!isDisplayed || forceUpdate)
        {
            Display(forceUpdate);
        }
        foreach (AbstractDisplayer disp in containedElements)
        {
            if (disp is ICircularDisplayer circularDisplayer)
                circularDisplayer.screenDisplayer = associatedScreenInputDisplayer;
            disp.transform.SetParent(viewport);
            disp.Display(forceUpdate);
        }

    }

    public override int GetIndexOf(AbstractDisplayer element)
    {
        return containedElements.IndexOf(element);
    }

    public override void Hide()
    {
        this.gameObject.SetActive(false);
        isDisplayed = false;
    }

    public override void Insert(AbstractDisplayer element, bool updateDisplay = true)
    {
        containedElements.Add(element);
        element.gameObject.SetActive(true);
        element.Hide();
    }

    public override void Insert(AbstractDisplayer element, int index, bool updateDisplay = true)
    {
        containedElements.Insert(index, element);
        element.Hide();
    }

    public override void InsertRange(IEnumerable<AbstractDisplayer> elements, bool updateDisplay = true)
    {
        foreach (AbstractDisplayer e in elements)
            Insert(e);
    }

    public override int IsSuitableFor(AbstractMenuItem menu)
    {
        return 1;
    }

    public override bool Remove(AbstractDisplayer element, bool updateDisplay = true)
    {
        if (element == null)
            return false;
        bool ok = containedElements.Remove(element);
        if (updateDisplay)
            Display();

        element.transform.SetParent(this.transform);
        element.gameObject.SetActive(false);
        return ok;
    }

    public override int RemoveAll()
    {
        List<AbstractDisplayer> elements = new List<AbstractDisplayer>(containedElements);
        int count = 0;
        foreach (AbstractDisplayer element in elements)
            if (Remove(element, false)) count++;
        return count;
    }

    public override bool RemoveAt(int index, bool updateDisplay = true)
    {
        containedElements.RemoveAt(index);
        return true;
    }

    protected override IEnumerable<AbstractDisplayer> GetDisplayers()
    {
        return containedElements;
    }

    public void OnHoverExit()
    {
        foreach (GameObject q in quarters)
        {
            MeshRenderer rnd = q.GetComponent<MeshRenderer>();
            Material[] m = rnd.materials;
            m[materialGroup.materialIndex] = materialGroup.enabled;
            rnd.materials = m;
        }
        transform.localScale = normalScale;
    }

    public void OnHoverEnter()
    {
        foreach (GameObject q in quarters)
        {
            MeshRenderer rnd = q.GetComponent<MeshRenderer>();
            Material[] m = rnd.materials;
            m[materialGroup.materialIndex] = materialGroup.hoverred;
            rnd.materials = m;
        }
        normalScale = transform.localScale;
        transform.localScale = hoveredScale;
    }

    public void OnSelect()
    {
        return;
    }

    public override void Select()
    {
        foreach (AbstractDisplayer disp in containedElements)
        {
            if (disp is ICircularDisplayer circularDisplayer)
                circularDisplayer.screenDisplayer = associatedScreenInputDisplayer;
        }
        base.Select();

        PlayerMenuManager playerMenuManager = screenDisplayer.playerMenuManager;

        if (containedElements.Count > 0)
        {
            playerMenuManager.DisplayParametersPanel();
            playerMenuManager.toolAndToolBoxDisplayDepth--;
        }
            
        playerMenuManager.SetNavigationButtonMaterials();
        playerMenuManager.EnableBackButton(true);
        playerMenuManager.SetToolboxAndToolLabel(menu.Name);
    }
}
