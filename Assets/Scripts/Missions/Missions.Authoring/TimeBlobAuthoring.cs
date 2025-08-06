using Data;
using Missions.Missions.Authoring.Schemas;
using Unity.Entities;
using UnityEngine;
using Time = Data.Time;

namespace Missions.Missions.Authoring
{
    public class TimeBlobAuthoring : MonoBehaviour
    {
        public TimeSchema[] timeSchemas;
        public class TimeBlobBaker : Baker<TimeBlobAuthoring>
        {
            public override void Bake(TimeBlobAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var blobAssetRef = BakingSchema<Time>.ToBlobAssetRef(authoring.timeSchemas);
                AddComponent(entity, new TimeBlob
                {
                    BlobAssetRef = blobAssetRef
                });
            }
        }
    }
}
