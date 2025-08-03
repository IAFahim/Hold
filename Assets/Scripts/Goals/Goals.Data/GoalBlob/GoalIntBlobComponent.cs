using Goals.Goals.Data.Goals;
using Unity.Entities;

namespace Goals.Data.GoalBlob
{
    public struct GoalIntBlobComponent : IComponentData
    {
        public BlobAssetReference<BlobArray<GoalInt>> BlobAssetRef;
    }
}