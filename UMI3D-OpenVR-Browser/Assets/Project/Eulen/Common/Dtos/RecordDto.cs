using System;
using System.Collections.Generic;
using umi3d.common;

namespace com.inetum.addonEulen.common.dtos
{
    /// <summary>
    /// Stores a record off a full movement.
    /// </summary>
    [Serializable]
    public class RecordDto : UMI3DDto
    {
        /// <summary>
        /// Data about the recorded user.
        /// </summary>
        public UserSettingsDto userSettings = new UserSettingsDto();

        /// <summary>
        /// Number of frame recorded per seconds.
        /// </summary>
        public float recordFps = 0f;

        /// <summary>
        /// All keyframes of the movement.
        /// </summary>
        public List<RecordKeyFrameDto> frames = new List<RecordKeyFrameDto>();

        public override string ToString()
        {
            return $"[RecordDto] Movement by a user with a camera height of {userSettings.cameraHeight} m. \n"
                + $"Record fps : { recordFps } \n" +
                 $"Keyframes count { frames.Count }. Movement of { frames.Count / recordFps} s.";
        }
    }
}