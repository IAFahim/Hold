using Unity.Entities;

namespace Missions.Missions.Authoring.BlobComponents
{
    public struct RewardBlob : IComponentData
    {
        public BlobAssetReference<BlobArray<Reward>> BlobAssetRef;
    }
}