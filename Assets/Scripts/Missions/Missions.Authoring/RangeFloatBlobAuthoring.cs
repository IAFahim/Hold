using Missions.Missions.Authoring.BlobComponents;
using Missions.Missions.Authoring.Schemas;
using Unity.Entities;
using UnityEngine;

namespace Missions.Missions.Authoring
{
    public class RangeFloatBlobAuthoring : MonoBehaviour
    {
        public RangeFloatSchema[] rangeFloatSchemas;
        public class RangeFloatBlobBaker : Baker<RangeFloatBlobAuthoring>
        {
            public override void Bake(RangeFloatBlobAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var blobAssetRef = BakingSchema<RangeFloat>.ToBlobAssetRef(authoring.rangeFloatSchemas);
                AddComponent(entity, new RangeFloatBlob
                {
                    BlobAssetRef = blobAssetRef
                });
            }
        }
    }
}
