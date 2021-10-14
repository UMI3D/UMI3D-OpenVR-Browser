/*
Copyright 2019 - 2021 Inetum

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

using umi3d.common.volume;
using umi3d.common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.cdk.volumes
{
    /// <summary>
    /// Loader for volume parts.
    /// </summary>
	static public class UMI3DVolumeLoader 
	{
        static public void ReadUMI3DExtension(AbstractVolumeDescriptorDto dto, Action finished, Action<string> failed)
        {
            switch (dto)
            {
                case AbstractPrimitiveDto prim:
                    VolumePrimitiveManager.CreatePrimitive(prim, p =>
                    {
                        UMI3DEnvironmentLoader.RegisterEntityInstance(dto.id, dto, p, () => VolumePrimitiveManager.DeletePrimitive(dto.id));
                        finished.Invoke();
                    });
                    break;
                case OBJVolumeDto obj:
                    ExternalVolumeDataManager.Instance.CreateOBJVolume(obj, objVolume =>
                    {
                        UMI3DEnvironmentLoader.RegisterEntityInstance(dto.id, dto, objVolume, () => ExternalVolumeDataManager.Instance.DeleteOBJVolume(dto.id));
                        finished.Invoke();
                    });

                    break;
                default:
                    failed("Unknown dto type");
                    break;
            }
        }

        static public bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            switch (property.property)
            {               

                //TODO : Primitives

                default:
                    return false;
            }
        }
    }
}