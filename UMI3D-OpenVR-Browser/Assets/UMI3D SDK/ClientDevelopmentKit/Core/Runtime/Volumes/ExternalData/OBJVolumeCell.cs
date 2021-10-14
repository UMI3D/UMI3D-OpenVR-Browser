using System.Collections;
using System.Collections.Generic;
using umi3d.common.volume;
using UnityEngine;
using System.Linq;

namespace umi3d.cdk.volumes
{
    public class OBJVolumeCell : AbstractVolumeCell
    {
        public List<Mesh> meshes;

        public ulong id;

        public override void GetBase(System.Action<Mesh> onsuccess, float angleLimit)
        {
            List<Mesh> bases = meshes.ConvertAll(m => GeometryTools.GetBase(m, angleLimit, 0.01f));
            onsuccess.Invoke(GeometryTools.ForceNormalUp(GeometryTools.Merge(bases)));
        }

        public override Mesh GetMesh()
        {
            return GeometryTools.Merge(meshes);
        }

        public override ulong Id() => id;

        public override bool IsInside(Vector3 point, Space relativeTo) => meshes.Exists(mesh => GeometryTools.IsInside(mesh, point));
    }
}