using Goals.Goals.Data.Goals;
using Unity.Entities;

namespace Goals.Goals.Data.GoalBlob
{
    public struct GoalRangeFloatBlobComponent : IComponentData
    {
        public BlobAssetReference<BlobArray<GoalRangeFloat>> BlobAssetRef;
    }
}