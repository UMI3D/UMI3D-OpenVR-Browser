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

public class EnvironmentsContainer : AbstractMenuDisplayContainer
{

    private List<AbstractDisplayer> containedDisplayer = new List<AbstractDisplayer>();


    public override AbstractDisplayer this[int i] { get => containedDisplayer[i]; set => containedDisplayer[i] = value; }

    public override void Clear()
    {
        foreach(AbstractDisplayer disp in new List<AbstractDisplayer>(containedDisplayer))
        {
            disp.Clear();
        }
        containedDisplayer.Clear();
    }

    public override void Collapse(bool forceUpdate = false)
    {
        foreach(AbstractDisplayer disp in containedDisplayer)
        {
            disp.Hide();
        }
    }

    public override bool Contains(AbstractDisplayer element)
    {
        return containedDisplayer.Contains(element);
    }

    public override int Count()
    {
        return containedDisplayer.Count;
    }

    public override AbstractMenuDisplayContainer CurrentMenuDisplayContainer()
    {
        return this;
    }

    public override void Display(bool forceUpdate = false)
    {
        this.gameObject.SetActive(true);
        isDisplayed = true;
    }

    public override void Expand(bool forceUpdate = false)
    {
        int n = containedDisplayer.Count;
        int lineCount = Mathf.CeilToInt(Mathf.Sqrt(n));

        for (int i= 0; i<containedDisplayer.Count; i++)
        {
            containedDisplayer[i].transform.position = this.transform.position 
                + (i % lineCount) * 0.67f * this.transform.right 
                + i / lineCount * 0.67f * this.transform.forward;

            containedDisplayer[i].Display();
        }
    }

    public override void ExpandAs(AbstractMenuDisplayContainer container, bool forceUpdate = false)
    {
        throw new System.NotImplementedException();
    }

    public override int GetIndexOf(AbstractDisplayer element)
    {
        return containedDisplayer.IndexOf(element);
    }

    public override void Hide()
    {
        //Collapse();
        this.gameObject.SetActive(false);
    }

    public override void Insert(AbstractDisplayer element, bool updateDisplay = true)
    {
        containedDisplayer.Add(element);
        element.transform.SetParent(this.transform);
    }

    public override void Insert(AbstractDisplayer element, int index, bool updateDisplay = true)
    {
        containedDisplayer.Insert(index, element);
        element.transform.SetParent(this.transform);
    }

    public override void InsertRange(IEnumerable<AbstractDisplayer> elements, bool updateDisplay = true)
    {
        foreach (var e in elements)
            Insert(e, updateDisplay);
    }

    public override int IsSuitableFor(AbstractMenuItem menu)
    {
        return 1;
    }

    public override bool Remove(AbstractDisplayer element, bool updateDisplay = true)
    {
        return containedDisplayer.Remove(element);
    }

    public override int RemoveAll()
    {
        int c = Count();
        containedDisplayer.Clear();
        return c;
    }

    public override bool RemoveAt(int index, bool updateDisplay = true)
    {
        containedDisplayer.RemoveAt(index);
        return true;        
    }

    protected override IEnumerable<AbstractDisplayer> GetDisplayers()
    {
        return containedDisplayer;
    }
}
