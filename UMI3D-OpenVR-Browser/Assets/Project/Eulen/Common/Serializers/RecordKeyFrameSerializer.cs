using com.inetum.addonEulen.common.dtos;
using umi3d.common;

namespace com.inetum.addonEulen.common.serializers
{
    public class RecordKeyFrameSerializer : UMI3DSerializerModule<RecordKeyFrameDto>
    {
        public bool IsCountable()
        {
            return true;
        }

        public bool Read(ByteContainer container, out bool readable, out RecordKeyFrameDto result)
        {
            result = new RecordKeyFrameDto(new());
            result.entries = UMI3DSerializer.ReadList<RecordKeyEntryDto>(container);
            readable = true;

            return true;
        }

        public bool Write(RecordKeyFrameDto value, out Bytable bytable, params object[] parameters)
        {
            bytable = UMI3DSerializer.Write(value.entries);

            return true;
        }
    }
}