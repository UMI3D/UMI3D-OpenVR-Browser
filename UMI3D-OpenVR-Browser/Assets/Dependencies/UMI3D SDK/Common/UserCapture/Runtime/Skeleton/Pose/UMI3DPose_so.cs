/*
Copyright 2019 - 2023 Inetum

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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using umi3d.common.userCapture.description;
using UnityEngine;

namespace umi3d.common.userCapture.pose
{
    /// <summary>
    /// Scriptable object to contains data for PoseDto
    /// </summary>
    [Serializable]
    public class UMI3DPose_so : ScriptableObject, IJsonSerializer, IUMI3DPoseData
    {
        #region Fields

        /// <summary>
        /// All the bones that describe the pose
        /// </summary>
        [SerializeField]
        private List<BoneField> bones = new();

        /// <summary>
        /// The bone that anchor the pose
        /// </summary>
        [SerializeField]
        private BonePoseField boneAnchor;

        #endregion Fields

        /// <inheritdoc/>
        public IList<BoneField> BoneDtos => bones;

        /// <inheritdoc/>
        public BonePoseField boneAnchorDto => boneAnchor;

        /// <inheritdoc/>
        public int Index { get; set; }

        /// <inheritdoc/>
        public void Init(List<BoneDto> bones, BonePoseDto bonePoseDto)
        {
            bones.ForEach(bp =>
            {
                this.bones.Add(new BoneField()
                {
                    boneType = bp.boneType,
                    rotation = bp.rotation.Quaternion()
                });
            });

            this.boneAnchor = new BonePoseField()
            {
                boneType = bonePoseDto.bone,
                position = bonePoseDto.position.Struct(),
                rotation = bonePoseDto.rotation.Quaternion()
            };
        }

        /// <inheritdoc/>
        public PoseDto ToDto()
        {
            return new PoseDto() { bones = GetBonesCopy(), boneAnchor = GetBonePoseCopy() };
        }

        /// <inheritdoc/>
        public List<BoneDto> GetBonesCopy()
        {
            List<BoneDto> copy = new List<BoneDto>();
            bones.ForEach(b =>
            {
                copy.Add(new BoneDto()
                {
                    boneType = b.boneType,
                    rotation = new Vector4Dto() { X = b.rotation.x, Y = b.rotation.y, Z = b.rotation.z, W = b.rotation.w }
                });
            });
            return copy;
        }

        /// <inheritdoc/>
        public BonePoseDto GetBonePoseCopy()
        {
            BonePoseDto copy = new BonePoseDto()
            {
                bone = boneAnchor.boneType,
                position = boneAnchor.position.Dto(),
                rotation = boneAnchor.rotation.Dto()
            };

            return copy;
        }

        [Serializable]
        public struct BoneField
        {
            public uint boneType;
            public Quaternion rotation;
        }

        [Serializable]
        public struct BonePoseField
        {
            public uint boneType;
            public Vector3 position;
            public Quaternion rotation;
        }

        /// <inheritdoc/>
        public string JsonSerialize()
        {
            PoseDto poseDto = ToDto();
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, Formatting = Formatting.Indented };
            string json = JsonConvert.SerializeObject(poseDto, settings);
            return json;
        }

        /// <inheritdoc/>
        public ScriptableObject JsonDeserializeScriptableObject(string data)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, Formatting = Formatting.Indented };
            PoseDto poseDto = JsonConvert.DeserializeObject(data, settings) as PoseDto;
            UMI3DPose_so poseSo = CreateInstance<UMI3DPose_so>();
            poseSo.Init(poseDto.bones, poseDto.boneAnchor);
            return poseSo;
        }
    }
}