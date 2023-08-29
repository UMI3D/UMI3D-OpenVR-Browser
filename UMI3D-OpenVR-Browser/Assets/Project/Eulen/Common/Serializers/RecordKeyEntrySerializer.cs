using com.inetum.addonEulen.common.dtos;
using umi3d.common;

namespace com.inetum.addonEulen.common.serializers
{
    public class RecordKeyEntrySerializer : UMI3DSerializerModule<RecordKeyEntryDto>
    {
        public bool IsCountable()
        {
            return true;
        }

        public bool Read(ByteContainer container, out bool readable, out RecordKeyEntryDto result)
        {
            result = new RecordKeyEntryDto(UMI3DSerializer.Read<int>(container),
                UMI3DSerializer.Read<Vector3Dto>(container),
                UMI3DSerializer.Read<Vector4Dto>(container));

            readable = true;
            return true;
        }

        public bool Write(RecordKeyEntryDto value, out Bytable bytable, params object[] parameters)
        {
            bytable = UMI3DSerializer.Write(value.source);
            bytable += UMI3DSerializer.Write(value.position);
            bytable += UMI3DSerializer.Write(value.rotation);

            return true;
        }
    }
}