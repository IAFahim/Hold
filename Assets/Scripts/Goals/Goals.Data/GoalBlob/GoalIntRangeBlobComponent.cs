using Goals.Goals.Data.Goals;
using Unity.Entities;

namespace Goals.Goals.Data.GoalBlob
{
    public struct GoalIntRangeBlobComponent : IComponentData
    {
        public BlobAssetReference<BlobArray<GoalRangeInt>> BlobAssetRef;
    }
}