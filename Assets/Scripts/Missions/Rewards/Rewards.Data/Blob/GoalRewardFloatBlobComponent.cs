
using Rewards.Rewards.Data.GoalReward;
using Unity.Entities;

namespace Rewards.Rewards.Data.Blob
{
    public struct GoalRewardFloatBlobComponent : IComponentData
    {
        public BlobAssetReference<BlobArray<RewardGoalFloat>> BlobAssetRef;
    }
}
