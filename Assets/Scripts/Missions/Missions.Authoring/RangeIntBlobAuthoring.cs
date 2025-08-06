using Data;
using Missions.Missions.Authoring.Schemas;
using Unity.Entities;
using UnityEngine;
using RangeInt = Data.RangeInt;

namespace Missions.Missions.Authoring
{
    public class RangeIntBlobAuthoring : MonoBehaviour
    {
        public RangeIntSchema[] rangeIntSchemas;
        public class RangeIntBlobBaker : Baker<RangeIntBlobAuthoring>
        {
            public override void Bake(RangeIntBlobAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var blobAssetRef = BakingSchema<RangeInt>.ToBlobAssetRef(authoring.rangeIntSchemas);
                AddComponent(entity, new RangeIntBlob
                {
                    BlobAssetRef = blobAssetRef
                });
            }
        }
    }
}
