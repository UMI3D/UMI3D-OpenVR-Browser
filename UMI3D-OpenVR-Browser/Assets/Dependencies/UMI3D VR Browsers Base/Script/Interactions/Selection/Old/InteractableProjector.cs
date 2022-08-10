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

using System.Collections;
using System.Collections.Generic;
using umi3d.cdk.interaction;
using UnityEngine;

namespace umi3dVRBrowsersBase.selection
{
    /// <summary>
    /// Handler to project <see cref="Interactable"/>.
    /// </summary>
    public class InteractableProjector : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// Time to wait before auto selecting (in seconds).
        /// </summary>
        public float delayBeforeAutoSelect = 2;

        /// <summary>
        /// Autorelease mode ?
        /// </summary>
        public bool autoRelease;

        /// <summary>
        /// Time to wait before au deslecting (in seconds)
        /// </summary>
        public float delayBeforeAutoDeselect = 10;

        /// <summary>
        /// Interactables selected after a long hover.
        /// </summary>
        private List<Interactable> interactablesAutoSelectedAfterDelay = new List<Interactable>();

        /// <summary>
        /// Interactables selected after a long hover.
        /// </summary>
        private List<Interactable> interactablesAutoSelected = new List<Interactable>();

        /// <summary>
        /// Coroutine which handles auto selection.
        /// </summary>
        private Coroutine autoSelect = null;

        #endregion

        #region Methods

        /// <summary>
        /// Is <paramref name="interactable"/> currently projected ?
        /// </summary>
        /// <param name="interactable"></param>
        /// <returns></returns>
        public bool IsProjected(Interactable interactable)
        {
            return InteractionMapper.Instance.IsToolSelected(interactable.dto.id);
        }

        /// <summary>
        /// Handles a <see cref="InteractableContainer"/> hover.
        /// </summary>
        /// <param name="hoveredObject"></param>
        /// <param name="hoveredObjectId"></param>
        /// <param name="onHoverExit"></param>
        /// <param name="controller"></param>
        public void OnHover(InteractableContainer hoveredObject, ulong hoveredObjectId, Interactable.Event onHoverExit, AbstractController controller)
        {
            autoSelect = StartCoroutine(AutoSelect(hoveredObject, hoveredObjectId, onHoverExit, controller));

            void cancelSelection(Interactable x)
            {
                if (autoSelect != null)
                    StopCoroutine(autoSelect);
                onHoverExit.RemoveListener(cancelSelection);
            }

            onHoverExit.AddListener(cancelSelection);
        }

        /// <summary>
        /// Handles the auto selection.
        /// </summary>
        /// <param name="hoveredObject"></param>
        /// <param name="hoveredObjectId"></param>
        /// <param name="onHoverExit"></param>
        /// <param name="controller"></param>
        /// <returns></returns>
        private IEnumerator AutoSelect(InteractableContainer hoveredObject, ulong hoveredObjectId, Interactable.Event onHoverExit, AbstractController controller)
        {
            Interactable hoveredInter = hoveredObject.Interactable;
            if (hoveredInter.dto.interactions == null)
                throw new System.Exception("No interaction to select");
            if (hoveredInter.dto.interactions.Count <= 0)
                throw new System.Exception("No interaction to select");
            if (AbstractInteractionMapper.Instance.IsToolSelected(hoveredObject.Interactable.id))
                yield break;

            if (AbstractInteractionMapper.Instance.SelectTool(hoveredInter.dto.id, true, hoveredObjectId, new AutoProjectOnHover() { controller = controller }))
            {
                interactablesAutoSelected.Add(hoveredInter);

                void release(Interactable inter)
                {
                    if (inter == hoveredInter)
                    {
                        if (AbstractInteractionMapper.Instance.IsToolSelected(hoveredInter.dto.id))
                            AbstractInteractionMapper.Instance.ReleaseTool(hoveredInter.dto.id);
                        interactablesAutoSelected.Remove(hoveredInter);
                        onHoverExit.RemoveListener(release);
                    }
                }

                onHoverExit.AddListener(release);
                yield break;
            }

            yield return new WaitForSeconds(delayBeforeAutoSelect);
            yield return new WaitForEndOfFrame();

            //at this point the coroutine has not been stopped.
            if (AbstractInteractionMapper.Instance.SelectTool(hoveredInter.dto.id, true, hoveredObjectId, new AutoProjectOnHover() { controller = controller }))
            {
                interactablesAutoSelectedAfterDelay.Add(hoveredObject.Interactable);
                if (autoRelease)
                    StartCoroutine(WaitAndRelease(hoveredObject.Interactable));
            }
        }

        /// <summary>
        /// Waits before releasing an <see cref="Interactable"/>.
        /// </summary>
        /// <param name="interactableToRelease"></param>
        /// <returns></returns>
        private IEnumerator WaitAndRelease(Interactable interactableToRelease)
        {
            float clock = 0;

            if (!AbstractInteractionMapper.Instance.IsToolSelected(interactableToRelease.dto.id))
            {
                throw new System.Exception("Internal error");
            }

            AbstractController controller = AbstractInteractionMapper.Instance.GetController(interactableToRelease.id);
            if (controller == null)
                throw new System.Exception("Internal error");

            while (clock < delayBeforeAutoDeselect)
            {
                if (controller.Interacting)
                    clock = 0;
                else
                    clock += Time.deltaTime;

                yield return new WaitForEndOfFrame();
            }

            interactablesAutoSelectedAfterDelay.Remove(interactableToRelease);
            if (AbstractInteractionMapper.Instance.IsToolSelected(interactableToRelease.dto.id))
                AbstractInteractionMapper.Instance.ReleaseTool(interactableToRelease.dto.id);
        }

        /// <summary>
        /// Projects an <see cref="Interactable"/> on a controller.
        /// </summary>
        /// <param name="interactable"></param>
        /// <param name="hoveredObjectId"></param>
        /// <param name="reason"></param>
        public void Project(Interactable interactable, ulong hoveredObjectId, InteractionMappingReason reason)
        {
            if (autoSelect != null)
                StopCoroutine(autoSelect);
            AbstractInteractionMapper.Instance.SelectTool(interactable.dto.id, true, hoveredObjectId, reason);
        }

        /// <summary>
        /// Unproject every auto-selected interactables.
        /// </summary>
        public void UnProjectAll()
        {
            var interactablesToRelease = new List<Interactable>(interactablesAutoSelectedAfterDelay);
            interactablesToRelease.AddRange(interactablesAutoSelected);

            foreach (Interactable inter in interactablesToRelease)
            {
                if (!AbstractInteractionMapper.Instance.IsToolSelected(inter.dto.id))
                {
                    throw new System.Exception("Internal error");
                }
                else
                {
                    interactablesAutoSelectedAfterDelay.Remove(inter);
                    AbstractInteractionMapper.Instance.ReleaseTool(inter.dto.id);
                }
            }
        }

        #endregion
    }
}