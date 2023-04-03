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
using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.cdk.interaction;
using umi3d.common.interaction;
using umi3dVRBrowsersBase.interactions.input;
using umi3dVRBrowsersBase.ui.playerMenu;
using UnityEngine;

namespace umi3dVRBrowsersBase.interactions
{
    public partial class VRController
    {
        public override List<AbstractUMI3DInput> inputs
        {
            get
            {
                int newHash = manipulationInputs.GetHashCode() + booleanInputs.GetHashCode();
                if ((lastComputedInputs != null) && (newHash == inputhash)) return lastComputedInputs;
                else
                {
                    var buffer = new List<AbstractUMI3DInput>();
                    buffer.AddRange(manipulationInputs);
                    buffer.AddRange(booleanInputs);
                    buffer.AddRange(menuInputs);
                    buffer.AddRange(formInputs);
                    lastComputedInputs = buffer;
                    inputhash = newHash;
                    return buffer;
                }
            }
        }

        private int inputhash = 0;
        private List<AbstractUMI3DInput> lastComputedInputs = null;
        private List<MenuInput> menuInputs = new List<MenuInput>();
        private List<FormMenuInput> formInputs = new List<FormMenuInput>();
        /// <summary>
        /// Instantiated float parameter inputs.
        /// </summary>
        /// <see cref="FindInput(AbstractParameterDto, bool)"/>
        protected List<FloatParameterInput> floatParameterInputs = new List<FloatParameterInput>();
        /// <summary>
        /// Instantiated float range parameter inputs.
        /// </summary>
        /// <see cref="FindInput(AbstractParameterDto, bool)"/>
        protected List<FloatRangeParameterInput> floatRangeParameterInputs = new List<FloatRangeParameterInput>();
        /// <summary>
        /// Instantiated int parameter inputs.
        /// </summary>
        /// <see cref="FindInput(AbstractParameterDto, bool)"/>
        protected List<IntParameterInput> intParameterInputs = new List<IntParameterInput>();
        /// <summary>
        /// Instantiated bool parameter inputs.
        /// </summary>
        /// <see cref="FindInput(AbstractParameterDto, bool)"/>
        protected List<BooleanParameterInput> boolParameterInputs = new List<BooleanParameterInput>();
        /// <summary>
        /// Instantiated string parameter inputs.
        /// </summary>
        /// <see cref="FindInput(AbstractParameterDto, bool)"/>
        protected List<StringParameterInput> stringParameterInputs = new List<StringParameterInput>();
        /// <summary>
        /// Instantiated string enum parameter inputs.
        /// </summary>
        /// <see cref="FindInput(AbstractParameterDto, bool)"/>
        protected List<StringEnumParameterInput> stringEnumParameterInputs = new List<StringEnumParameterInput>();

        [Header("Manipulations")]
        public List<ManipulationInput> manipulationInputs = new List<ManipulationInput>();
        [Header("Other")]
        public List<BooleanInput> booleanInputs = new List<BooleanInput>();
        [Tooltip("Input used by default for an hold event")]
        public BooleanInput HoldInput;

        #region Clear

        /// <summary>
        /// Clear <paramref name="inputs"/> and apply <paramref name="action"/> on each element of <paramref name="inputs"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputs"></param>
        /// <param name="action"></param>
        public static void ClearInputs<T>(ref List<T> inputs, Action<T> action)
            where T : AbstractUMI3DInput
        {
            inputs.ForEach(action);
            inputs = new List<T>();
        }

        /// <summary>
        /// Clear all menus and the projected tools.
        /// </summary>
        public override void Clear()
        {
            ReleaseCurrentTool();

            Action<AbstractUMI3DInput> action = (input) =>
            {
                input.Dissociate();
                Destroy(input);
            };

            ClearInputs(ref menuInputs, action);
            ClearInputs(ref formInputs, action);
            ClearInputs(ref booleanInputs, input =>
            {
                if (!input.IsAvailable()) input.Dissociate();
            });
            ClearInputs(ref manipulationInputs, input =>
            {
                if (!input.IsAvailable()) input.Dissociate();
            });

            ClearParameters(action);
        }

        protected void ClearParameters(Action<AbstractUMI3DInput> action)
        {
            ClearInputs(ref floatParameterInputs, action);
            ClearInputs(ref floatRangeParameterInputs, action);
            ClearInputs(ref intParameterInputs, action);
            ClearInputs(ref boolParameterInputs, action);
            ClearInputs(ref stringParameterInputs, action);
            ClearInputs(ref stringEnumParameterInputs, action);
        }

        #endregion

        #region Find Inputs

        protected AbstractUMI3DInput FindInput<T>(List<T> inputs, System.Predicate<T> predicate, GameObject gO = null) where T : AbstractUMI3DInput, new()
        {
            T input = inputs.Find(predicate);
            if (input == null) AddInput(inputs, out input, gO);
            return input;
        }
        protected AbstractVRInput FindVRInput<T>(List<T> inputs, System.Predicate<T> predicate, GameObject gO = null) where T : AbstractVRInput, new()
        {
            T input = inputs.Find(predicate);
            if (input == null) AddInput(inputs, out input, gO);
            return input;
        }
        protected void AddInput<T>(List<T> inputs, out T input, GameObject gO) where T : AbstractUMI3DInput, new()
        {
            if (gO != null) input = gO.AddComponent<T>();
            else input = new T();

            //if (input is EventInteraction keyMenuInput) keyMenuInput.bone = interactionBoneType;
            //else if (input is FormMenuInput formInput) formInput.bone = interactionBoneType;
            //else if (input is LinkInteraction linkInput) linkInput.bone = interactionBoneType;
            input.Init(this);
            input.Menu = ObjectMenu.menu;
            inputs.Add(input);
        }

        #endregion

        #region Find Interactions

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="form"></param>
        /// <param name="unused"></param>
        /// <returns></returns>
        public override AbstractUMI3DInput FindInput(FormDto form, bool unused = true)
            => FindInput(formInputs, i => i.IsAvailable() || !unused, this.gameObject);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="link"></param>
        /// <param name="unused"></param>
        /// <returns></returns>
        public override AbstractUMI3DInput FindInput(LinkDto link, bool unused = true)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="unused"></param>
        /// <param name="tryToFindInputForHoldableEvent"></param>
        /// <returns></returns>
        public override AbstractUMI3DInput FindInput(EventDto evt, bool unused = true, bool tryToFindInputForHoldableEvent = false)
        {
            AbstractVRInput res = null;

            if (HoldInput != null && tryToFindInputForHoldableEvent && HoldInput.IsAvailable())
                res = HoldInput;

            if (res == null)
            {
                foreach (BooleanInput input in booleanInputs)
                {
                    if (input.IsAvailable() || !unused)
                    {
                        res = input;
                        break;
                    }
                }
            }

            if (res == null) res = FindVRInput(menuInputs, i => i.IsAvailable() || !unused, this.gameObject);

            PlayerMenuManager.Instance.CtrlToolMenu.AddBinding(res);

            return res;
        }

        #endregion

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="param"></param>
        /// <param name="unused"></param>
        /// <returns></returns>
        public override AbstractUMI3DInput FindInput(AbstractParameterDto param, bool unused = true)
        {
            if (param is FloatRangeParameterDto) return FindInput(floatRangeParameterInputs, i => i.IsAvailable(), this.gameObject);
            else if (param is FloatParameterDto) return FindInput(floatParameterInputs, i => i.IsAvailable(), this.gameObject);
            else if (param is IntegerParameterDto) return FindInput(intParameterInputs, i => i.IsAvailable());
            else if (param is IntegerRangeParameterDto) throw new System.NotImplementedException();
            else if (param is BooleanParameterDto) return FindInput(boolParameterInputs, i => i.IsAvailable(), this.gameObject);
            else if (param is StringParameterDto) return FindInput(stringParameterInputs, i => i.IsAvailable(), this.gameObject);
            else if (param is EnumParameterDto<string>) return FindInput(stringEnumParameterInputs, i => i.IsAvailable(), this.gameObject);
            else return null;
        }

        #region Find Manipulation

        /// <summary>
        /// Find the best dof separation for this controller.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public override DofGroupOptionDto FindBest(DofGroupOptionDto[] options)
        {
            return options[0];
        }

        /// <summary>
        /// Find the best free input for a given manipulation dof.
        /// </summary>
        /// <param name="manip">Manipulation to associate input to</param>
        /// <param name="dof">Degree of freedom to associate input to</param>
        /// <returns></returns>
        public override AbstractUMI3DInput FindInput(ManipulationDto manip, DofGroupDto dof, bool unused = true)
        {
            AbstractVRInput result = null;

            foreach (ManipulationInput input in manipulationInputs)
            {
                if (input.IsCompatibleWith(manip))
                {
                    if (input.IsAvailable() || !unused)
                    {
                        result = input;
                        break;
                    }
                }
            }

            if (result == null)
            {
                //if no input was found
                result = this.gameObject.AddComponent<MenuInput>();
                result.Init(this);
                menuInputs.Add(result as MenuInput);
            }

            PlayerMenuManager.Instance.CtrlToolMenu.AddBinding(result);

            return result;
        }

        #endregion
    }
}
