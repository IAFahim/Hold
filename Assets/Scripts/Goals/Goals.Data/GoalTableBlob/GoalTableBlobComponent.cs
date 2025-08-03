using Unity.Entities;

namespace Goals.Goals.Data.GoalTableBlob
{
    public struct GoalTableBlobComponent : IComponentData
    {
        public BlobAssetReference<BlobArray<GoalTable>> BlobAssetRef;
    }
}