using Missions.Missions.Authoring.BlobComponents;
using Missions.Missions.Authoring.Schemas;
using Missions.Missions.Authoring.Scriptable;
using Unity.Entities;
using UnityEngine;
using Time = Missions.Missions.Authoring.Time;

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
