using Data;
using Unity.Entities;
using UnityEngine;

namespace Missions.Missions.Authoring
{
    public class StationBlobAuthoring : MonoBehaviour
    {
        public StationSchema[] stationSchemas;
        public class StationBlobBaker : Baker<StationBlobAuthoring>
        {
            public override void Bake(StationBlobAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var blobAssetRef = StationSchema.ToBlobAssetRef(authoring.stationSchemas);
                AddComponent(entity, new StationBlob
                {
                    BlobAssetRef = blobAssetRef
                });
            }
        }
    }
}