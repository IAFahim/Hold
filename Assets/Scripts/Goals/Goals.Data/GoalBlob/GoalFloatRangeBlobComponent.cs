using Goals.Goals.Data.Goals;
using Unity.Entities;

namespace Goals.Goals.Data.GoalBlob
{
    public struct GoalFloatRangeBlobComponent : IComponentData
    {
        public BlobAssetReference<BlobArray<GoalRangeFloat>> BlobAssetRef;
    }
}