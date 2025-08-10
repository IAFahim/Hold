using Missions.Missions.Authoring.BlobComponents;
using Missions.Missions.Authoring.Schemas;
using Missions.Missions.Authoring.Scriptable;
using Unity.Entities;
using UnityEngine;

namespace Missions.Missions.Authoring
{
    public class DataContainerBlobAuthoring : MonoBehaviour
    {
        public DataContainerSchema[] dataContainerSchemas;
        public class DataContainerBlobBaker : Baker<DataContainerBlobAuthoring>
        {
            public override void Bake(DataContainerBlobAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var blobAssetRef = BakingSchema<DataContainer>.ToAssetRef(authoring.dataContainerSchemas);
                AddComponent(entity, new DataContainerBlob
                {
                    BlobAssetRef = blobAssetRef
                });
            }
        }
    }
}
