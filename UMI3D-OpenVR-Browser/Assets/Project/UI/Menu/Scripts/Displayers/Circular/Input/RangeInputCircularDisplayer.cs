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
/// Displays a slider for a circular menu.
/// </summary>
public class RangeInputCircularDisplayer : AbstractRangeInputDisplayer, ICircularDisplayer
{
    #region Fields

    /// <summary>
    /// Label for the menu
    /// </summary>
    public Text circularMenuLabel;

    public Slider slider;
    public Text sliderLabel;
    public Text sliderValue;

    /// <summary>
    /// Canvas to modify the value.
    /// </summary>
    public GameObject content;

    /// <summary>
    /// Manipulator to control the slider with a joystick.
    /// </summary>
    public SliderJoystickManipulator sliderManipulator;

    /// <summary>
    /// Frame rate applied to message emission threough network (high values can cause network flood).
    /// </summary>
    public float networkFramerate = 30;

    /// <summary>
    /// Launched coroutine for network message sending (if any).
    /// </summary>
    protected Coroutine messageSenderCoroutine;

    protected bool valueChanged = false;


    /// <summary>
    /// Different positions for the element in the circular menu.
    /// </summary>
    public List<GameObject> quarters;
    private int _currentQuarter = 0;

    /// <summary>
    /// Current position
    /// </summary>
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

    public Vector3 hoveredScale = new Vector3(1.22f, 1, 1.22f);
    Vector3 normalScale;

    public PlayerMenuManager.MaterialGroup materialGroup;

    #endregion

    #region Methods

    void Start()
    {
        content.SetActive(false);
    }

    /// <summary>
    /// Displays the element in the circularMenu (not the slider directly).
    /// </summary>
    /// <see cref="OnSelect"/>
    /// <param name="forceUpdate"></param>
    public override void Display(bool forceUpdate = false)
    {
        this.gameObject.SetActive(true);
        if (menuItem.continuousRange)
        {
            slider.minValue = menuItem.min;
            slider.maxValue = menuItem.max;
            slider.wholeNumbers = false;
        } else
        {
            slider.minValue = 0;
            slider.maxValue = (menuItem.max - menuItem.min) / menuItem.increment;
        }

        circularMenuLabel.text = menuItem.ToString();

        sliderLabel.text = menuItem.ToString();
        slider.value = menuItem.GetValue();
        
        sliderValue.text = FormatValue(slider.value);
        slider.onValueChanged.AddListener((i) =>
        {
            valueChanged = true;
            sliderValue.text = FormatValue(slider.value);
        });          
    }

    public override void Hide()
    {
        this.gameObject.SetActive(false);
        slider.onValueChanged.RemoveAllListeners();
        if(messageSenderCoroutine != null)
        {
            UnityMainThreadDispatcher.Instance().StopCoroutine(messageSenderCoroutine);
        }   
    }

    public override void Clear()
    {
        base.Clear();
        slider.onValueChanged.RemoveAllListeners();

        if(messageSenderCoroutine != null)
            UnityMainThreadDispatcher.Instance().StopCoroutine(messageSenderCoroutine);
    }

    /// <summary>
    /// Sends message if value changed.
    /// </summary>
    /// <returns></returns>
    protected IEnumerator NetworkMessageSender()
    {
        while (true)
        {
            if (valueChanged)
            {
                NotifyValueChange(slider.value);
                valueChanged = false;
            }
            yield return new WaitForSeconds(1f / networkFramerate);
        }
    }

    /// <summary>
    /// Displays a canvas to enable users to modify the value of this element.
    /// </summary>
    public void OnSelect()
    {
        IsContentDisplayed = true;
        content.SetActive(true);

        messageSenderCoroutine = UnityMainThreadDispatcher.Instance().StartCoroutine(NetworkMessageSender());

        float previousValue = slider.value;

        screenDisplayer.DisplayContent(content, (b) =>
        {
            if (!b)
            {
                slider.value = previousValue;
                NotifyValueChange(slider.value);
            }
                
            IsContentDisplayed = false;
            content.transform.SetParent(this.transform);
            content.SetActive(false);
            UnityMainThreadDispatcher.Instance().StopCoroutine(messageSenderCoroutine);
        },
        sliderManipulator);
    }

    string FormatValue(float f)
    {
        return string.Format("{0:###0.##}", f);
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

    public override void SetMenuItem(AbstractMenuItem menu)
    {
        base.SetMenuItem(menu);
        name = menu.Name + "(" + GetType() + ")";
    }

    public override int IsSuitableFor(AbstractMenuItem menu)
    {
        return (menu is FloatRangeInputMenuItem) ? 3 : 0;
    }

    #endregion
}
