using com.inetum.addonEulen.common.dtos;
using inetum.unityUtils;
using umi3d.common;

namespace com.inetum.addonEulen.common.serializers
{
    public class RecordSerializer : UMI3DSerializerModule<RecordDto>
    {
        public bool IsCountable()
        {
            return true;
        }

        public bool Read(ByteContainer container, out bool readable, out RecordDto result)
        {
            readable = false;

            result = new RecordDto();
            result.userSettings = UMI3DSerializer.Read<UserSettingsDto>(container);
            result.recordFps = UMI3DSerializer.Read<float>(container);
            result.frames = UMI3DSerializer.ReadList<RecordKeyFrameDto>(container);

            return true;
        }

        public bool Write(RecordDto value, out Bytable bytable, params object[] parameters)
        {
            UnityEngine.Debug.Assert(value != null, "Value cannot be null");
            UnityEngine.Debug.Assert(value.userSettings != null);
            UnityEngine.Debug.Assert(value != null);
            UnityEngine.Debug.Assert(value != null);

            bytable = UMI3DSerializer.Write(value.userSettings);
            bytable += UMI3DSerializer.Write(value.recordFps);
            bytable += UMI3DSerializer.Write(value.frames);

            return true;
        }
    }
}