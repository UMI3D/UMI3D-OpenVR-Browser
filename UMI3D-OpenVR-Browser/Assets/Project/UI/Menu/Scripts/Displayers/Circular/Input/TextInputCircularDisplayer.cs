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
using MainThreadDispatcher;
using System.Collections;
using System.Collections.Generic;
using umi3d.cdk.menu;
using umi3d.cdk.menu.view;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays an input field for a circular menu.
/// </summary>
public class TextInputCircularDisplayer : AbstractTextInputDisplayer, ICircularDisplayer
{
    #region Fields 

    /// <summary>
    /// Label for the menu
    /// </summary>
    public Text circularMenuLabel;

    /// <summary>
    /// Canvas to modify the value.
    /// </summary>
    public GameObject content;

    /// <summary>
    /// InputField 
    /// </summary>
    public InputField textfield;

    /// <summary>
    /// Text input label
    /// </summary>
    public Text inputLabel;

    /// <summary>
    /// Frame rate applied to message emission through network (high values can cause network flood).
    /// </summary>
    public float networkFrameRate = 30;

    /// <summary>
    /// Launched coroutine for network message sending (if any).
    /// </summary>
    /// <see cref="NetworkMessageSender"/>
    protected Coroutine messageSenderCoroutine;

    protected bool valueChanged = false;

    public List<GameObject> quarters;
    private int _currentQuarter = 0;
    public int CurrentQuarter
    {
        get
        {
            return _currentQuarter;
        }
        set
        {
            _currentQuarter = Mathf.Clamp(value, 0, quarters.Count);
            for (int i = 0; i < quarters.Count; i++)
            {
                quarters[i].SetActive(_currentQuarter == i);
            }
        }
    }

    public bool IsContentDisplayed { get; set; } = false;

    public ScreenInputDisplayer screenDisplayer { get; set; }

    /// <summary>
    /// Scale when the object is hovered
    /// </summary>
    public Vector3 hoveredScale = new Vector3(1.22f, 1, 1.22f);
    Vector3 normalScale;

    /// <summary>
    /// Manipulator to control the textinput with a joystick.
    /// </summary>
    public TextFieldJoystickManipulator textInputManipulator;

    public PlayerMenuManager.MaterialGroup materialGroup;

    #endregion

    #region Methods

    void Start()
    {
        content.SetActive(false);
    }

    /// <summary>
    /// If the input field value changes, sends the new value to the server.
    /// </summary>
    /// <returns></returns>
    protected IEnumerator NetworkMessageSender()
    {
        while (true)
        {
            if (valueChanged)
            {
                NotifyValueChange(textfield.text);
                valueChanged = false;
            }
            yield return new WaitForSeconds(1f / networkFrameRate);
        }
    }

    /// <summary>
    /// Displays the element in the circularMenu (not the inputField directly).
    /// </summary>
    /// <see cref="OnSelect"/>
    /// <param name="forceUpdate"></param>
    public override void Display(bool forceUpdate = false)
    {
        this.gameObject.SetActive(true);

        circularMenuLabel.text = menuItem.ToString();
        inputLabel.text = circularMenuLabel.text;
        textfield.text = menuItem.GetValue();
        textfield.onValueChanged.AddListener((i) => { valueChanged = true; });
    }

    public override void Clear()
    {
        base.Clear();
        textfield.onValueChanged.RemoveAllListeners();
        if (messageSenderCoroutine != null)
            UnityMainThreadDispatcher.Instance().StopCoroutine(messageSenderCoroutine);
    }

    public override void Hide()
    {
        textfield.onValueChanged.RemoveAllListeners();
        if (messageSenderCoroutine != null)
        {
            UnityMainThreadDispatcher.Instance().StopCoroutine(messageSenderCoroutine);
        }
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// Displays a canvas to enable users to modify the value of this element.
    /// </summary>
    public void OnSelect()
    {
        IsContentDisplayed = true;
        content.SetActive(true);

        messageSenderCoroutine = UnityMainThreadDispatcher.Instance().StartCoroutine(NetworkMessageSender());

        string previousValue = textfield.text;

        screenDisplayer.DisplayContent(content, (b) =>
        {
            if (!b)
            {
                textfield.text = previousValue;
                NotifyValueChange(textfield.text);
            }

            IsContentDisplayed = false;
            content.transform.SetParent(this.transform);
            content.SetActive(false);
            UnityMainThreadDispatcher.Instance().StopCoroutine(messageSenderCoroutine);
        },
        textInputManipulator);
    }

    /// <summary>
    /// Called when the element is not longer hovored in the circular menu.
    /// </summary>
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

    /// <summary>
    /// Called when the element is hovered in the circular menu.
    /// </summary>
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

    public override void SetMenuItem(AbstractMenuItem menu)
    {
        base.SetMenuItem(menu);
        name = menu.Name + "(" + GetType() + ")";
    }

    /// <summary>
    /// State is a the displayer is suitable for an AbstractMenuItem
    /// </summary>
    /// <param name="menu">The Menu to evaluate</param>
    /// <returns></returns>
    public override int IsSuitableFor(AbstractMenuItem menu)
    {
        return (menu is TextInputMenuItem) ? 4 : 0;
    }

    #endregion
}
