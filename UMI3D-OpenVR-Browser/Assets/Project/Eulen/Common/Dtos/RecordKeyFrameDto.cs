using System;
using System.Collections.Generic;
using umi3d.common;

namespace com.inetum.addonEulen.common.dtos
{
    /// <summary>
    /// Stores a keyframe record off a full body.
    /// </summary>
    [Serializable]
    public class RecordKeyFrameDto : UMI3DDto
    {
        public List<RecordKeyEntryDto> entries = new List<RecordKeyEntryDto>();

        public RecordKeyFrameDto(List<RecordKeyEntryDto> entries)
        {
            this.entries = new List<RecordKeyEntryDto>(entries);
        }
    }
}