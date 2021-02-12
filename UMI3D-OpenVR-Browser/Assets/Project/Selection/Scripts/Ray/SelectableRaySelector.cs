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

public class SelectableRaySelector : RaySelector<Selectable>
{

    /// <summary>
    /// Event system used for UI interaction
    /// </summary>
    [SerializeField]
    private EventSystem eventSystem = null;


    /// <summary>
    /// Selectable pointed at on last frame
    /// </summary>
    private Selectable lastSelectable = null;


    protected override void DeactivateInternal()
    {
        base.DeactivateInternal();

        if (lastSelectable != null)
            laser.OnHoverExit(this.gameObject.GetInstanceID());
    }

    /// <summary>
    /// Handle custom pointer event raising
    /// </summary>
    private void HandlePointerEvent()
    {

        PointerEventData pointerEventData = new PointerEventData(eventSystem);
        if (!activated)
        {
            if (lastSelectable != null)
            {
                lastSelectable.SendMessage("OnPointerExit", pointerEventData, SendMessageOptions.DontRequireReceiver);
                lastSelectable = null;
                laser.OnHoverExit(this.gameObject.GetInstanceID());
            }
            return;
        }


        RaycastHit? hit = GetClosestOfAllPointedObject();

        if (hit != null)
        {
            Selectable hitSelectable = hit.Value.transform.gameObject.GetComponent<Selectable>();

            if (hitSelectable != null)
            {
                if (hitSelectable != lastSelectable)
                    hitSelectable.SendMessage("OnPointerEnter", pointerEventData);

                if ((lastSelectable != null) && (hitSelectable != lastSelectable))
                {
                    lastSelectable.SendMessage("OnPointerExit", pointerEventData, SendMessageOptions.DontRequireReceiver);                    
                }

                if (!laser.hovering)
                    laser.OnHoverEnter(this.gameObject.GetInstanceID());
            }
            else if (lastSelectable != null)
            {
                lastSelectable.SendMessage("OnPointerExit", pointerEventData, SendMessageOptions.DontRequireReceiver);
                laser.OnHoverExit(this.gameObject.GetInstanceID());
                lastSelectable = null;
            }

            lastSelectable = hitSelectable;

        }
        else if (lastSelectable != null)
        {
            lastSelectable.SendMessage("OnPointerExit", pointerEventData, SendMessageOptions.DontRequireReceiver);            
            lastSelectable = null;
            laser.OnHoverExit(this.gameObject.GetInstanceID());
        }
        else if (laser.hovering)
        {
            laser.OnHoverExit(this.gameObject.GetInstanceID());
        }
    }

    [ContextMenu("Pick")]
    public override void Select()
    {
        if (isClosestPointedObjectOfTypeT() && activated)
        {
            RaycastHit? closestSelectableHit = GetClosestPointedObject();
            Selectable closestSelectable = closestSelectableHit.Value.transform.GetComponent<Selectable>();
            switch (closestSelectable.GetType().Name)
            {
                case "Button":
                    Button button = (Button)closestSelectable;
                    ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.submitHandler);
                    break;
                case "Toggle":
                    Toggle toggle = (Toggle)closestSelectable;
                    toggle.isOn = !toggle.isOn;
                    break;
                case "Dropdown":
                    Dropdown dropdown = (Dropdown)closestSelectable;
                    dropdown.Show();
                    break;
                case "InputField":
                    InputField inputfield = (InputField)closestSelectable;
                    inputfield.Select();
                    break;
                case "Scrollbar":
                    Scrollbar scrollbar = (Scrollbar)closestSelectable;
                    Vector3[] corners = new Vector3[4];
                    closestSelectable.transform.GetComponent<RectTransform>().GetWorldCorners(corners);

                    Vector3 upPosition = 0.5f * (corners[1] + corners[2]);
                    Vector3 downPosition = 0.5f * (corners[0] + corners[3]);

                    float Maxdist = Vector3.Distance(upPosition, downPosition);
                    float Hitdistance = Vector3.Distance(upPosition, closestSelectableHit.Value.point);

                    scrollbar.value = Mathf.InverseLerp(Maxdist * 0.9f, 0.1f, Hitdistance);
                    break;
                case "Slider":
                    Slider slider = closestSelectable as Slider;
                    Vector3[] localCorners = new Vector3[4];
                    RectTransform sliderRectTransform = closestSelectable.transform.GetComponent<RectTransform>();

                    sliderRectTransform.GetLocalCorners(localCorners);
                    Vector3 localHitPoint = sliderRectTransform.InverseTransformPoint(closestSelectableHit.Value.point);
                    localHitPoint.x = Mathf.Clamp(localHitPoint.x, localCorners[0].x, localCorners[3].x);

                    float newValue = (float) System.Math.Round(slider.minValue + ((localHitPoint.x + localCorners[3].x) / ( localCorners[3].x - localCorners[0].x)) * (slider.maxValue - slider.minValue), 1);
                    slider.value = newValue;

                    break;
            }
        }
    }


    protected override void Update()
    {
        if (activated)
        {
            base.Update();
            HandlePointerEvent();
        }
    }

    protected override void UpdateRayDisplayer()
    {
        RaycastHit? hit = GetClosestPointedObject();
        if (hit != null)
        {
            laser.SetImpactPoint(hit.Value.point);
        }
    }

}
