using Goals.Goals.Data.Goals;
using Unity.Entities;

namespace Goals.Goals.Data.GoalBlob
{
    public struct GoalRangeIntBlobComponent : IComponentData
    {
        public BlobAssetReference<BlobArray<GoalRangeInt>> BlobAssetRef;
    }
}