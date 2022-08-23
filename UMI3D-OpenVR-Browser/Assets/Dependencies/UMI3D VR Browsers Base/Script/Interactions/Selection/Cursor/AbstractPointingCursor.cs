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

using UnityEngine.Events;

namespace umi3dBrowsers.interaction.selection.cursor
{
    /// <summary>
    /// Based class for cursor associated to selection based on the virtual pointing paradigm
    /// </summary>
    public abstract class AbstractPointingCursor : AbstractCursor
    {
        public class CursorTrackingEvent : UnityEvent<PointingInfo>
        { }

        /// <summary>
        /// Event triggered when the cursor enters an UMI3D interactable
        /// </summary>
        public static CursorTrackingEvent OnCursorEnter = new CursorTrackingEvent();
        /// <summary>
        /// Event triggered when the cursor stays inside an UMI3D interactable
        /// </summary>
        public static CursorTrackingEvent OnCursorStay = new CursorTrackingEvent();
        /// <summary>
        /// Event triggered when the cursor exits an UMI3D interactable
        /// </summary>
        public static CursorTrackingEvent OnCursorExit = new CursorTrackingEvent();
    }
}