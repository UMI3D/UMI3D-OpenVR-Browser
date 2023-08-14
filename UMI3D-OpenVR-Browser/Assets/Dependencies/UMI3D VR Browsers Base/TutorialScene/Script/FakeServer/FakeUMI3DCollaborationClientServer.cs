///*
//Copyright 2019 - 2022 Inetum

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//    http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.
//*/

//using umi3d.cdk.collaboration;
//using umi3d.common;
//using umi3d.common.interaction;

//namespace umi3dVRBrowsersBase.tutorial.fakeServer
//{
//    /// <summary>
//    /// Imitates an UMI3DCollaborationClientServer.
//    /// </summary>
//    public class FakeUMI3DCollaborationClientServer : UMI3DCollaborationClientServer
//    {
//        /// <summary>
//        /// Associated environment loader.
//        /// </summary>
//        private FakeEnvironmentLoader loader;

//        /// <summary>
//        /// <inheritdoc/>
//        /// </summary>
//        /// <param name="dto"></param>
//        /// <param name="reliable"></param>
//        protected override void _Send(AbstractBrowserRequestDto dto, bool reliable)
//        {
//            if (loader == null)
//                loader = (FakeEnvironmentLoader.Instance as FakeEnvironmentLoader);

//            switch (dto)
//            {
//                case EventTriggeredDto eventTriggeredDto:
//                    (loader.GetInteraction(eventTriggeredDto.id) as FakeEvent)?.onTrigger?.Invoke();
//                    break;
//                case EventStateChangedDto eventStateChangedDto:
//                    if (eventStateChangedDto.active)
//                        (loader.GetInteraction(eventStateChangedDto.id) as FakeEvent)?.onHold?.Invoke();
//                    else
//                        (loader.GetInteraction(eventStateChangedDto.id) as FakeEvent)?.onRelease?.Invoke();
//                    break;
//                case ParameterSettingRequestDto parameterSettingRequestDto:
//                    (loader.GetInteraction(parameterSettingRequestDto.id) as AbstractFakeParameter)?.SetValue(parameterSettingRequestDto.parameter);
//                    break;
//                default:
//                    break;
//            }
//        }

//        /// <summary>
//        /// <inheritdoc/>
//        /// </summary>
//        /// <param name="dto"></param>
//        protected override void _SendTracking(AbstractBrowserRequestDto dto)
//        {
//        }
//    }
//}