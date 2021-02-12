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

public class CircularMenuContainer : AbstractMenuDisplayContainer
{

    public HemicircularPathConstraint hemicircularPathConstraint;


    protected List<AbstractDisplayer> containedElements = new List<AbstractDisplayer>();


    protected AbstractMenuDisplayContainer virtualContainer = null;


    public ScreenInputDisplayer screenInputDisplayer;


    //done
    public override AbstractDisplayer this[int i]
    {
        get => containedElements[i];
        set
        {
            if ((i < 0) || (i >= Count()))
                throw new System.IndexOutOfRangeException();

            RemoveAt(i);
            Insert(value, i, true);
        }
    }


    protected virtual void Awake()
    {
        virtualContainer = this;
    }


    public override void Clear()
    {
        foreach (AbstractDisplayer disp in containedElements)
            disp.Clear();

        containedElements.Clear();

        this.gameObject.SetActive(false);
        isDisplayed = false;
    }

    public override void Collapse(bool forceUpdate = false)
    {
        this.gameObject.SetActive(false);

        foreach (AbstractDisplayer disp in containedElements)
        {
            disp.Hide();
        }
    }

    

    public override AbstractMenuDisplayContainer CurrentMenuDisplayContainer()
    {
        return virtualContainer;
    }

    public override void Display(bool forceUpdate = false)
    {
        //menuName.text = menu.Name;

        if (isDisplayed && forceUpdate)
            Clear();

        this.gameObject.SetActive(true);
        isDisplayed = true;
    }

    public override void Expand(bool forceUpdate = false)
    {
        ExpandAs(this, forceUpdate);
    }

    public override void ExpandAs(AbstractMenuDisplayContainer container, bool forceUpdate = false)
    {
        if (isExpanded && !forceUpdate)
        {
            return;
        }
        this.gameObject.SetActive(true);

        if (virtualContainer != null && virtualContainer != container)
        {
            foreach (AbstractDisplayer displayer in virtualContainer)
            {
                displayer.Hide();
            }
        }

        virtualContainer = container;
        hemicircularPathConstraint.constraintedObjects.Clear();

        foreach (AbstractDisplayer disp in virtualContainer)
        {
            if (disp is ICircularDisplayer circularDisplayer)
            {
                circularDisplayer.screenDisplayer = screenInputDisplayer;
            }
            hemicircularPathConstraint.constraintedObjects.Add(disp.transform);
            disp.Display();
        }
        hemicircularPathConstraint.Cursor = hemicircularPathConstraint.numberOfItemsInCircle - 1;
        isExpanded = true;
    }

    //done
    public override int GetIndexOf(AbstractDisplayer element)
    {
        return containedElements.IndexOf(element);
    }

    public override void Hide()
    {
        isDisplayed = false;
        foreach (AbstractDisplayer disp in virtualContainer)
        {
            disp.Hide();
        } 
        this.gameObject.SetActive(false);
    }

    public override void Insert(AbstractDisplayer element, bool updateDisplay = true)
    {
        containedElements.Add(element);
        element.Hide();
        if (updateDisplay)
            Display(true);
    }

    public override void Insert(AbstractDisplayer element, int index, bool updateDisplay = true)
    {
        containedElements.Insert(index, element);
        element.Hide();
        if (updateDisplay)
            Display(true);
    }

    

    public override bool Remove(AbstractDisplayer element, bool updateDisplay = true)
    {
        bool r = containedElements.Remove(element);
        if (updateDisplay)
            Display(true);
        return r;
    }


    //done
    public override bool Contains(AbstractDisplayer element)
    {
        return containedElements.Contains(element);
    }

    //done
    public override int Count()
    {
        return containedElements.Count;
    }

    //done
    public override void InsertRange(IEnumerable<AbstractDisplayer> elements, bool updateDisplay = true)
    {
        foreach (AbstractDisplayer e in elements)
            Insert(e, false);

        if (updateDisplay)
            Display(true);
    }

    //done
    public override int IsSuitableFor(AbstractMenuItem menu)
    {
        return 0;
    }

    //done
    public override int RemoveAll()
    {
        int c = Count();
        foreach (AbstractDisplayer d in new List<AbstractDisplayer>(containedElements))
            Remove(d, false);

        return c;
    }

    //done
    public override bool RemoveAt(int index, bool updateDisplay = true)
    {
        if ((index < 0) || (index >= Count()))
            return false;

        return Remove(containedElements[index], updateDisplay);
    }

    //done
    protected override IEnumerable<AbstractDisplayer> GetDisplayers()
    {
        return containedElements;
    }

    public override void Select()
    {
        foreach (AbstractDisplayer disp in virtualContainer)
        {
            if (disp is ICircularDisplayer circularDisplayer)
            {
                circularDisplayer.screenDisplayer = screenInputDisplayer;
            }
        }
        base.Select();

        PlayerMenuManager playerMenuManager = screenInputDisplayer.playerMenuManager;
        playerMenuManager.SetNavigationButtonMaterials();
        playerMenuManager.EnableBackButton(true);
        //playerMenuManager.toolAndToolboxesLabel.text = menu.Name;
        playerMenuManager.SetToolboxAndToolLabel(string.Empty);
    }
}
