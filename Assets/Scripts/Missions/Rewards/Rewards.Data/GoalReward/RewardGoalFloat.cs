using System;
using BovineLabs.Essence.Data;

namespace Rewards.Rewards.Data.GoalReward
{
    [Serializable]
    public struct RewardGoalFloat
    {
        public ushort id;

        public ushort ID
        {
            readonly get => id;
            set => id = value;
        }

        public ERewardGoalType rewardGoalType;
        public ushort goalId;

        public StatKey statKey;
        public float reward;
    }
}