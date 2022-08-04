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

using umi3d.cdk.interaction.selection.zoneselection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace umi3d.cdk.interaction.selection.projector
{
    /// <summary>
    /// Projector for Selectable.
    /// Identifies the Selectable type and interact with it
    /// </summary>
    public class SelectableProjector : IProjector<Selectable>
    {
        /// <inheritdoc/>
        public void Project(Selectable selectable, AbstractController controller)
        {
            var pointerEventData = new PointerEventData(EventSystem.current) { clickCount = 1 };
            selectable.OnPointerEnter(pointerEventData);
        }

        /// <summary>
        /// Deselect an object
        /// </summary>
        /// <param name="selectable"></param>
        public void Release(Selectable selectable)
        {
            var pointerEventData = new PointerEventData(EventSystem.current);
            if (selectable != null) //protects against cases when UI element is destroyed but not deselected
            {
                selectable.OnPointerExit(pointerEventData);
                if (!(selectable is InputField)) //keep keyboard focus on input fields
                    selectable.OnDeselect(pointerEventData);
            }
        }

        /// <inheritdoc/>
        public void Release(Selectable selectable, AbstractController controller)
        {
            Release(selectable);
        }

        /// <summary>
        /// Triggers the interaction associated to the selectable for a pick interaction
        /// </summary>
        /// <param name="selectable"></param>
        /// <param name="controller"></param>
        public void Pick(Selectable selectable, AbstractController controller)
        {
            switch (selectable)
            {
                case Button button:
                    button.Interact();
                    break;

                case Toggle toggle:
                    toggle.Interact();
                    break;

                case Dropdown dropdown:
                    dropdown.Interact();
                    break;

                case InputField inputfield:
                    inputfield.Interact(controller.transform);
                    break;

                case Scrollbar scrollbar:
                    scrollbar.Interact(controller.transform);
                    break;

                case Slider slider:
                    slider.Interact(controller.transform);
                    break;
            }
        }

        /// <summary>
        /// Triggers the UI actions associated to the selectable for a long press interaction
        /// </summary>
        /// <param name="selectable"></param>
        /// <param name="controller"></param>
        public void Press(Selectable selectable, AbstractController controller)
        {
            switch (selectable)
            {
                case Button button:
                    button.Press();
                    currentlyPressedButton = button;
                    break;
            }
        }

        public Button currentlyPressedButton;
    }

    public static class UIProjection
    {
        #region Button
        public static void Interact(this Button button)
        {
            ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.submitHandler);
        }

        public static void Press(this Button button)
        {
            ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerDownHandler);
        }
        #endregion

        #region Toggle
        public static void Interact(this Toggle toggle)
        {
            toggle.isOn = !toggle.isOn;
        }
        #endregion

        #region Dropdown
        public static void Interact(this Dropdown dropdown)
        {
            dropdown.Show();
        }
        #endregion

        #region InputField
        public static void Interact(this InputField inputField, Transform controllerTransform)
        {
            RaySelectionZone<Selectable> raycastHelper = new RaySelectionZone<Selectable>(controllerTransform);
            var closestAndRaycastHit = raycastHelper.GetClosestAndRaycastHit();

            if (inputField == null)
                return;

            int lenght = inputField.text.Length;

            // If the text of the input is already fully selected.
            if (inputField.selectionAnchorPosition == lenght && inputField.selectionFocusPosition == 0 && lenght > 0)
            {
                Text text = inputField.textComponent;
                RectTransform textTransform = text.GetComponent<RectTransform>();

                if (textTransform != null)
                {
                    Vector3 localClickedPoint = text.transform.InverseTransformPoint(closestAndRaycastHit.raycastHit.point);

                    float tmp = 0;
                    int i = 1;

                    foreach (char c in text.text)
                    {
                        if (text.font.GetCharacterInfo(c, out CharacterInfo info, text.fontSize))
                        {
                            tmp += info.advance;

                            if (localClickedPoint.x + Mathf.Abs(textTransform.rect.x) > tmp)
                            {
                                i = Mathf.Clamp(i + 1, 0, lenght);
                                inputField.caretPosition = i;
                            }
                            else
                            {
                                if (i == 1)
                                    if (localClickedPoint.x + Mathf.Abs(textTransform.rect.x) < 0)
                                        inputField.caretPosition = 0;
                                    else
                                        inputField.caretPosition = 1;
                                break;
                            }
                        }
                    }
                    return;
                }
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(null, null); // if the textfield was already selected, we have to unselect it before selecting again to make sure the text will be fully selected
                inputField.Select();
            }
        }
        #endregion

        #region Scrollbar
        public static void Interact(this Scrollbar scrollbar, Transform controllerTransform)
        {
            RaySelectionZone<Selectable> raycastHelper = new RaySelectionZone<Selectable>(controllerTransform);
            var closestAndRaycastHit = raycastHelper.GetClosestAndRaycastHit();

            Vector3[] corners = new Vector3[4];
            scrollbar.transform.GetComponent<RectTransform>().GetWorldCorners(corners);

            Vector3 upPosition = 0.5f * (corners[1] + corners[2]);
            Vector3 downPosition = 0.5f * (corners[0] + corners[3]);

            float Maxdist = Vector3.Distance(upPosition, downPosition);
            float Hitdistance = Vector3.Distance(upPosition, closestAndRaycastHit.raycastHit.point);

            scrollbar.value = Mathf.InverseLerp(Maxdist * 0.9f, 0.1f, Hitdistance);
        }
        #endregion

        #region Slider
        public static void Interact(this Slider slider, Transform controllerTransform)
        {
            RaySelectionZone<Selectable> raycastHelper = new RaySelectionZone<Selectable>(controllerTransform);
            var closestRaycastHit = raycastHelper.GetClosestAndRaycastHit();

            Vector3[] localCorners = new Vector3[4];
            RectTransform sliderRectTransform = slider.transform.GetComponent<RectTransform>();

            sliderRectTransform.GetLocalCorners(localCorners);
            Vector3 localHitPoint = sliderRectTransform.InverseTransformPoint(closestRaycastHit.raycastHit.point);
            localHitPoint.x = Mathf.Clamp(localHitPoint.x, localCorners[0].x, localCorners[3].x);

            float newValue = (float)System.Math.Round(slider.minValue + ((localHitPoint.x + localCorners[3].x) / (localCorners[3].x - localCorners[0].x)) * (slider.maxValue - slider.minValue), 1);
            slider.value = newValue;
        }
        #endregion
    }

}