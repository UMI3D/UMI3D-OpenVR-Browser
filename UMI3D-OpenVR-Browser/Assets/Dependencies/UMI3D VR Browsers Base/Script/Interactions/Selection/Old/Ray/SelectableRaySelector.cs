/*
Copyright 2019 - 2022 Inetum

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

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace umi3dVRBrowsersBase.selection
{
    /// <summary>
    /// Ray selector for <see cref="Selectable"/>.
    /// </summary>
    public class SelectableRaySelector : RaySelector<Selectable>
    {
        #region Fields

        /// <summary>
        /// Event system used for UI interaction
        /// </summary>
        [SerializeField]
        private EventSystem eventSystem = null;

        /// <summary>
        /// Selectable pointed at on last frame
        /// </summary>
        private Selectable lastSelectable = null;


        #endregion

        #region Methods

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
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
            var pointerEventData = new PointerEventData(eventSystem);
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

                    if (!laser.Hovering)
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
            else if (laser.Hovering)
            {
                laser.OnHoverExit(this.gameObject.GetInstanceID());
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        [ContextMenu("Pick")]
        public override void Select()
        {
            if (isClosestPointedObjectOfTypeT() && activated)
            {
                RaycastHit? closestSelectableHit = GetClosestPointedObject();
                Selectable closestSelectable = closestSelectableHit.Value.transform.GetComponent<Selectable>();

                switch (closestSelectable)
                {
                    case Button button:
                        ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.submitHandler);
                        break;
                    case Toggle toggle:
                        toggle.isOn = !toggle.isOn;
                        break;
                    case Dropdown dropdown:
                        dropdown.Show();
                        break;
                    case InputField inputField:
                        SelectInputField(inputField, closestSelectableHit.Value.point);
                        break;
                    case Scrollbar scrollbar:
                        var corners = new Vector3[4];
                        closestSelectable.transform.GetComponent<RectTransform>().GetWorldCorners(corners);

                        Vector3 upPosition = 0.5f * (corners[1] + corners[2]);
                        Vector3 downPosition = 0.5f * (corners[0] + corners[3]);

                        float maxdist = Vector3.Distance(upPosition, downPosition);
                        float hitdistance = Vector3.Distance(upPosition, closestSelectableHit.Value.point);

                        scrollbar.value = Mathf.InverseLerp(maxdist * 0.9f, 0.1f, hitdistance);
                        break;
                    case Slider slider:
                        var localCorners = new Vector3[4];
                        RectTransform sliderRectTransform = closestSelectable.transform.GetComponent<RectTransform>();

                        sliderRectTransform.GetLocalCorners(localCorners);
                        Vector3 localHitPoint = sliderRectTransform.InverseTransformPoint(closestSelectableHit.Value.point);
                        localHitPoint.x = Mathf.Clamp(localHitPoint.x, localCorners[0].x, localCorners[3].x);

                        float newValue = (float)System.Math.Round(slider.minValue + ((localHitPoint.x + localCorners[3].x) / (localCorners[3].x - localCorners[0].x)) * (slider.maxValue - slider.minValue), 1);
                        slider.value = newValue;
                        break;
                }
            }
        }

        /// <summary>
        /// Selects an <see cref="InputField"/>.
        /// </summary>
        /// <param name="inputfield"></param>
        /// <param name="hitPoint"></param>
        private void SelectInputField(InputField inputfield, Vector3 hitPoint)
        {
            if (inputfield == null)
                return;

            int lenght = inputfield.text.Length;

            // If the text of the input is already fully selected.
            if (inputfield.selectionAnchorPosition == lenght && inputfield.selectionFocusPosition == 0 && lenght > 0)
            {
                Text text = inputfield.textComponent;
                RectTransform textTransform = text.GetComponent<RectTransform>();

                if (textTransform != null)
                {
                    Vector3 localClickedPoint = text.transform.InverseTransformPoint(hitPoint);

                    float tmp = 0;
                    int i = 1;

                    foreach (char c in text.text)
                    {
                        CharacterInfo info;
                        if (text.font.GetCharacterInfo(c, out info, text.fontSize))
                        {
                            tmp += info.advance;

                            if (localClickedPoint.x + Mathf.Abs(textTransform.rect.x) > tmp)
                            {
                                i = Mathf.Clamp(i + 1, 0, lenght);
                                inputfield.caretPosition = i;
                            }
                            else
                            {
                                if (i == 1)
                                    if (localClickedPoint.x + Mathf.Abs(textTransform.rect.x) < 0)
                                        inputfield.caretPosition = 0;
                                    else
                                        inputfield.caretPosition = 1;
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
                inputfield.Select();
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Update()
        {
            if (activated)
            {
                base.Update();
                HandlePointerEvent();
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void UpdateRayDisplayer()
        {
            RaycastHit? hit = GetClosestPointedObject();
            if (hit != null)
            {
                laser.SetImpactPoint(hit.Value.point);
            }
        }

        #endregion
    }
}