﻿using System;
using umi3d.common.interaction;
using umi3d.common.userCapture;

namespace umi3d.common.collaboration
{
    public class UMI3DCollaborationNetworkingModule : Umi3dNetworkingHelperModule
    {

        public override bool Read<T>(ByteContainer container, out bool readable, out T result)
        {
            switch (true)
            {
                case true when typeof(T) == typeof(UserCameraPropertiesDto):
                    readable = container.length >= 17 * sizeof(float) + sizeof(uint);
                    if (readable)
                    {
                        var usercam = new UserCameraPropertiesDto();
                        usercam.scale = UMI3DNetworkingHelper.Read<float>(container);
                        usercam.projectionMatrix = UMI3DNetworkingHelper.Read<SerializableMatrix4x4>(container);
                        usercam.boneType = UMI3DNetworkingHelper.Read<uint>(container);
                        result = (T)Convert.ChangeType(usercam, typeof(T));
                    }
                    else
                        result = default(T);
                    return true;
                case true when typeof(T) == typeof(BoneDto):
                    uint type;
                    SerializableVector4 rot;
                    if (UMI3DNetworkingHelper.TryRead(container, out type)
                        && UMI3DNetworkingHelper.TryRead(container, out rot))
                    {
                        var bone = new BoneDto() { boneType = type, rotation = rot };
                        result = (T)Convert.ChangeType(bone, typeof(T));
                        readable = true;
                    }
                    else
                    {
                        result = default(T);
                        readable = false;
                    }
                    return true;
                case true when typeof(T) == typeof(BoneBindingDto):
                    string bindingId;
                    string rigName;
                    bool active;
                    uint boneType;
                    ulong objectId;
                    bool syncPosition;
                    SerializableVector3 offsetPosition;
                    SerializableVector4 offsetRotation;
                    if (UMI3DNetworkingHelper.TryRead(container, out bindingId)
                        && UMI3DNetworkingHelper.TryRead(container, out rigName)
                        && UMI3DNetworkingHelper.TryRead(container, out active)
                        && UMI3DNetworkingHelper.TryRead(container, out boneType)
                        && UMI3DNetworkingHelper.TryRead(container, out objectId)
                        && UMI3DNetworkingHelper.TryRead(container, out offsetPosition)
                        && UMI3DNetworkingHelper.TryRead(container, out offsetRotation)
                        && UMI3DNetworkingHelper.TryRead(container, out syncPosition))
                    {
                        var bone = new BoneBindingDto()
                        {
                            bindingId = bindingId,
                            rigName = rigName,
                            active = active,
                            boneType = boneType,
                            objectId = objectId,
                            offsetPosition = offsetPosition,
                            offsetRotation = offsetRotation,
                            syncPosition = syncPosition
                        };
                        result = (T)Convert.ChangeType(bone, typeof(T));
                        readable = true;
                    }
                    else
                    {
                        result = default(T);
                        readable = false;
                    }
                    return true;
                case true when typeof(T) == typeof(UMI3DRenderedNodeDto.MaterialOverrideDto):
                    var mat = new UMI3DRenderedNodeDto.MaterialOverrideDto();
                    readable = UMI3DNetworkingHelper.TryRead<ulong>(container, out mat.newMaterialId);
                    if (readable)
                    {
                        readable = UMI3DNetworkingHelper.TryRead<bool>(container, out mat.addMaterialIfNotExists);
                        if (readable)
                        {
                            mat.overridedMaterialsId = UMI3DNetworkingHelper.ReadList<string>(container);
                            result = (T)Convert.ChangeType(mat, typeof(T));
                        }
                        else
                            result = default(T);
                    }
                    else
                        result = default(T);
                    return true;
                case true when typeof(T) == typeof(ResourceDto):
                    var resource = new ResourceDto();
                    resource.variants = UMI3DNetworkingHelper.ReadList<FileDto>(container);
                    readable = true;
                    result = (T)Convert.ChangeType(resource, typeof(T));
                    return true;
                case true when typeof(T) == typeof(FileDto):
                    var file = new FileDto();
                    file.metrics = new AssetMetricDto();
                    readable = UMI3DNetworkingHelper.TryRead<string>(container, out file.url)
                               && UMI3DNetworkingHelper.TryRead<string>(container, out file.format)
                               && UMI3DNetworkingHelper.TryRead<string>(container, out file.extension)
                               && UMI3DNetworkingHelper.TryRead<int>(container, out file.metrics.resolution)
                               && UMI3DNetworkingHelper.TryRead<float>(container, out file.metrics.size)
                               && UMI3DNetworkingHelper.TryRead<string>(container, out file.pathIfInBundle)
                               && UMI3DNetworkingHelper.TryRead<string>(container, out file.libraryKey);
                    if (readable)
                    {
                        result = (T)Convert.ChangeType(file, typeof(T));
                    }
                    else
                        result = default(T);
                    return true;
                default:
                    result = default(T);
                    readable = false;
                    return false;
            }
        }

        public override bool Write<T>(T value, out Bytable bytable)
        {
            switch (value)
            {
                case LocalInfoRequestParameterValue localInfovalue:
                    bytable = UMI3DNetworkingHelper.Write(localInfovalue.read);
                    bytable += UMI3DNetworkingHelper.Write(localInfovalue.write);
                    break;
                case UserCameraPropertiesDto camera:
                    bytable = UMI3DNetworkingHelper.Write(camera.scale);
                    bytable += UMI3DNetworkingHelper.Write(camera.projectionMatrix);
                    bytable += UMI3DNetworkingHelper.Write(camera.boneType);
                    break;
                case EnumParameterDto<string> param:
                    bytable = UMI3DNetworkingHelper.Write(UMI3DParameterKeys.Enum);
                    bytable += UMI3DNetworkingHelper.Write(param.value);
                    break;
                case BooleanParameterDto param:
                    bytable = UMI3DNetworkingHelper.Write(UMI3DParameterKeys.Bool);
                    bytable += UMI3DNetworkingHelper.Write(param.value);
                    break;
                case FloatParameterDto param:
                    bytable = UMI3DNetworkingHelper.Write(UMI3DParameterKeys.Float);
                    bytable += UMI3DNetworkingHelper.Write(param.value);
                    break;
                case IntegerParameterDto param:
                    bytable = UMI3DNetworkingHelper.Write(UMI3DParameterKeys.Int);
                    bytable += UMI3DNetworkingHelper.Write(param.value);
                    break;
                case StringParameterDto param:
                    bytable = UMI3DNetworkingHelper.Write(UMI3DParameterKeys.String);
                    bytable += UMI3DNetworkingHelper.Write(param.value);
                    break;
                case UploadFileParameterDto param:
                    bytable = UMI3DNetworkingHelper.Write(UMI3DParameterKeys.StringUploadFile);
                    bytable += UMI3DNetworkingHelper.Write(param.value);
                    bytable += UMI3DNetworkingHelper.Write(param.authorizedExtensions);
                    break;
                case IntegerRangeParameterDto param:
                    bytable = UMI3DNetworkingHelper.Write(UMI3DParameterKeys.IntRange);
                    bytable += UMI3DNetworkingHelper.Write(param.value);
                    bytable += UMI3DNetworkingHelper.Write(param.min);
                    bytable += UMI3DNetworkingHelper.Write(param.max);
                    bytable += UMI3DNetworkingHelper.Write(param.increment);
                    break;
                case FloatRangeParameterDto param:
                    bytable = UMI3DNetworkingHelper.Write(UMI3DParameterKeys.FloatRange);
                    bytable += UMI3DNetworkingHelper.Write(param.value);
                    bytable += UMI3DNetworkingHelper.Write(param.min);
                    bytable += UMI3DNetworkingHelper.Write(param.max);
                    bytable += UMI3DNetworkingHelper.Write(param.increment);
                    break;
                case UMI3DRenderedNodeDto.MaterialOverrideDto material:
                    bytable = UMI3DNetworkingHelper.Write(material.newMaterialId);
                    bytable += UMI3DNetworkingHelper.Write(material.addMaterialIfNotExists);
                    bytable += UMI3DNetworkingHelper.WriteCollection(material.overridedMaterialsId);
                    break;
                case ResourceDto resourceDto:
                    bytable = UMI3DNetworkingHelper.WriteCollection(resourceDto.variants);
                    break;
                case FileDto fileDto:
                    bytable = UMI3DNetworkingHelper.Write(fileDto.url)
                        + UMI3DNetworkingHelper.Write(fileDto.format)
                        + UMI3DNetworkingHelper.Write(fileDto.extension)
                        + UMI3DNetworkingHelper.Write(fileDto.metrics.resolution)
                        + UMI3DNetworkingHelper.Write(fileDto.metrics.size)
                        + UMI3DNetworkingHelper.Write(fileDto.pathIfInBundle)
                        + UMI3DNetworkingHelper.Write(fileDto.libraryKey);
                    break;
                default:
                    bytable = null;
                    return false;
            }
            return true;
        }
    }
}