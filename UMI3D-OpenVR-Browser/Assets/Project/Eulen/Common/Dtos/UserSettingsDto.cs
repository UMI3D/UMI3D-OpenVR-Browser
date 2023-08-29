using System;
using System.Collections.Generic;
using umi3d.common;

namespace com.inetum.addonEulen.common.dtos
{
    [Serializable]
    public class UserSettingsDto : UMI3DDto
    {
        public Dictionary<int, Vector4Dto> trackerToBoneRotations = new Dictionary<int, Vector4Dto>();

        public Dictionary<int, float> boneOffsets = new Dictionary<int, float>();

        public Dictionary<int, float> boneLenghts = new Dictionary<int, float>();

        public float cameraHeight = 1.65f;

        public float offsetFromGround = 0.0f;
    }
}