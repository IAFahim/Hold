using Missions.Missions.Authoring.BlobComponents;
using Missions.Missions.Authoring.Schemas;
using Unity.Entities;
using UnityEngine;

namespace Missions.Missions.Authoring
{
    public class StationBlobAuthoring : MonoBehaviour
    {
        public LocationSchema[] stationSchemas;
        public class StationBlobBaker : Baker<StationBlobAuthoring>
        {
            public override void Bake(StationBlobAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var blobAssetRef = LocationSchema.ToAssetRef(authoring.stationSchemas);
                AddComponent(entity, new StationBlob
                {
                    BlobAssetRef = blobAssetRef
                });
            }
        }
    }
}