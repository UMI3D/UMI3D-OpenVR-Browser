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

using TMPro;
using umi3d.cdk.interaction;
using umi3dBrowsers.interaction.selection.zoneselection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace umi3dBrowsers.interaction.selection.projector
{
    /// <summary>
    /// Projector for Selectable.
    /// Identifies the Selectable type and interact with it.
    /// </summary>
    /// /// Does not really projects the UMI3D interactions but act like it is projecting the client own's interactions.
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
                if(!(selectable is InputField || selectable is TMP_InputField)) //keep keyboard focus on input fields
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
        public void Pick(Selectable selectable, AbstractController controller, PointerEventData eventData)
        {
            switch (selectable)
            {
                case Button button:
                    button.Click(eventData);
                    break;

                case Toggle toggle:
                    toggle.Click(eventData);
                    break;

                case Dropdown dropdown:
                    dropdown.Click(eventData);
                    break;

                case InputField inputfield:
                    inputfield.Click(controller.transform);
                    break;

                case TMP_InputField tmp_input:
                    tmp_input.Click(controller.transform);
                    break;

                case Scrollbar scrollbar:
                    scrollbar.Click(controller.transform);
                    break;

                case Slider slider:
                    slider.Click(controller.transform);
                    break;
            }
        }

        /// <summary>
        /// Triggers the UI actions associated to the selectable for a long press interaction
        /// </summary>
        /// <param name="selectable"></param>
        /// <param name="controller"></param>
        public void PressDown(Selectable selectable, AbstractController controller, PointerEventData eventData)
        {
            switch (selectable)
            {
                case Button button:
                    button.PressDown(eventData);
                    currentlyPressedButton = button;
                    break;
            }
        }

        /// <summary>
        /// Button that is currenlty pressed
        /// </summary>
        public Button currentlyPressedButton;
    }

    /// <summary>
    /// Define extension methods that are used for interactions with <see cref="Selectable"/> objects
    /// </summary>
    public static class UIProjection
    {
        #region Button

        public static void Click(this Button button, PointerEventData pointerEventData)
        {
            ExecuteEvents.Execute(button.gameObject, pointerEventData, ExecuteEvents.submitHandler);
        }

        public static void PressDown(this Button button, PointerEventData pointerEventData)
        {
            ExecuteEvents.Execute(button.gameObject, pointerEventData, ExecuteEvents.pointerDownHandler);
        }

        #endregion Button

        #region Toggle

        public static void Click(this Toggle toggle, PointerEventData pointerEventData)
        {
            pointerEventData.button = PointerEventData.InputButton.Left;
            toggle.OnPointerClick(pointerEventData);
        }

        #endregion Toggle

        #region Dropdown

        public static void Click(this Dropdown dropdown, PointerEventData pointerEventData)
        {
            dropdown.OnPointerClick(pointerEventData);
        }

        #endregion Dropdown

        #region InputField

        public static void Click(this InputField inputField, Transform controllerTransform)
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

        public static void Click(this TMP_InputField inputField, Transform controllerTransform)
        {
            if (inputField == null) return;

            var closestAndRaycastHit = new RaySelectionZone<Selectable>(controllerTransform).GetClosestAndRaycastHit();
            int lenght = inputField.text.Length;

            // If the text of the input is already fully selected.
            if (inputField.selectionAnchorPosition == lenght && inputField.selectionFocusPosition == 0 && lenght > 0)
            {
                TMP_Text text = inputField.textComponent;
                RectTransform textTransform = text.GetComponent<RectTransform>();
                if (textTransform == null) return;

                Vector3 localClickedPoint = text.transform.InverseTransformPoint(closestAndRaycastHit.raycastHit.point);
                float tmpCaretPosition = 0;
                int i = 1;

                for (int j = 0; j < lenght; ++j)
                {
                    var characterInfo = text.textInfo.characterInfo[j];
                    tmpCaretPosition += characterInfo.xAdvance - characterInfo.origin;

                    // If the current character is before laser point.
                    if (localClickedPoint.x + Mathf.Abs(textTransform.rect.x) > tmpCaretPosition)
                    {
                        i = Mathf.Clamp(i + 1, 0, lenght);
                        inputField.caretPosition = i;
                    }
                    else
                    {
                        if (i == 1)
                        {
                            if (localClickedPoint.x + Mathf.Abs(textTransform.rect.x) < 0) inputField.caretPosition = 0;
                            else inputField.caretPosition = 1;
                        }
                        EventSystem.current.SetSelectedGameObject(null, null);
                        inputField.ActivateInputField();
                        inputField.Select();
                        break;
                    }
                }
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(null, null); // if the textfield was already selected, we have to unselect it before selecting again to make sure the text will be fully selected
                inputField.ActivateInputField();
                inputField.Select();
            }
        }

        #endregion InputField

        #region Scrollbar

        public static void Click(this Scrollbar scrollbar, Transform controllerTransform)
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

        #endregion Scrollbar

        #region Slider

        public static void Click(this Slider slider, Transform controllerTransform)
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

        #endregion Slider
    }
}