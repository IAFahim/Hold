using Goals.Goals.Data.Goals;
using Unity.Entities;

namespace Goals.Goals.Data
{
    public struct GoalIntBlobComponent : IComponentData
    {
        public BlobAssetReference<BlobArray<GoalInt>> BlobAssetRef;
    }
}