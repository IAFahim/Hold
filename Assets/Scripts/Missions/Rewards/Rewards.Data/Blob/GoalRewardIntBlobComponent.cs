
using Rewards.Rewards.Data.GoalReward;
using Unity.Entities;

namespace Rewards.Rewards.Data.Blob
{
    public struct GoalRewardIntBlobComponent : IComponentData
    {
        public BlobAssetReference<BlobArray<RewardGoalInt>> BlobAssetRef;
    }
}
