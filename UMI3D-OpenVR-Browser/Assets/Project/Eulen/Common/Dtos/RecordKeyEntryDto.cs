using System;
using umi3d.common;
using UnityEngine;

namespace com.inetum.addonEulen.common.dtos
{
    /// <summary>
    /// Stores a keyframe record for a single bone.
    /// </summary>
    [Serializable]
    public class RecordKeyEntryDto : UMI3DDto
    {
        public int source;

        /// <summary>
        /// World position
        /// </summary>
        public Vector3Dto position;

        /// <summary>
        /// Local rotation
        /// </summary>
        public Vector4Dto rotation;

        public RecordKeyEntryDto(int source, Vector3Dto position, Vector4Dto rotation)
        {
            this.source = source;
            this.position = position;
            this.rotation = rotation;
        }
    }
}