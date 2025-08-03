using Goals.Goals.Data.Goals;
using Unity.Entities;

namespace Goals.Goals.Data.GoalBlob
{
    public struct GoalFloatBlobComponent : IComponentData
    {
        public BlobAssetReference<BlobArray<GoalFloat>> BlobAssetRef;
    }
}