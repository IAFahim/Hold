using Missions.Missions.Authoring.BlobComponents;
using Missions.Missions.Authoring.Schemas;
using Missions.Missions.Authoring.Scriptable;
using Unity.Entities;
using UnityEngine;

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
                var blobAssetRef = BakingSchema<TimeStruct>.ToAssetRef(authoring.timeSchemas);
                AddComponent(entity, new TimeBlob
                {
                    BlobAssetRef = blobAssetRef
                });
            }
        }
    }
}
