using com.inetum.addonEulen.common.dtos;
using inetum.unityUtils;
using umi3d.common;
using UnityEngine;

namespace com.inetum.addonEulen.common.serializers
{
    public class UserSettingsSerializer : UMI3DSerializerModule<UserSettingsDto>
    {
        public bool IsCountable()
        {
            return true;
        }

        public bool Read(ByteContainer container, out bool readable, out UserSettingsDto result)
        {
            result = new();

            result.trackerToBoneRotations = UMI3DSerializer.ReadDictionary<int, Vector4Dto>(container);
            result.boneOffsets = UMI3DSerializer.ReadDictionary<int, float>(container);
            result.boneLenghts = UMI3DSerializer.ReadDictionary<int, float>(container);
            result.cameraHeight = UMI3DSerializer.Read<float>(container);
            result.offsetFromGround = UMI3DSerializer.Read<float>(container);

            readable = true;

            return true;
        }

        public bool Write(UserSettingsDto value, out Bytable bytable, params object[] parameters)
        {
            bytable = UMI3DSerializer.WriteCollection(value.trackerToBoneRotations);
            bytable += UMI3DSerializer.WriteCollection(value.boneOffsets);
            bytable += UMI3DSerializer.WriteCollection(value.boneLenghts);
            bytable += UMI3DSerializer.Write(value.cameraHeight);
            bytable += UMI3DSerializer.Write(value.offsetFromGround);

            return true;
        }
    }
}
