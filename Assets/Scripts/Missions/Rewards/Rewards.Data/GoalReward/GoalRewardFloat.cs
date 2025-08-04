using System;
using BovineLabs.Essence.Data;

namespace Rewards.Rewards.Data.GoalReward
{
    [Serializable]
    public struct GoalRewardFloat
    {
        public ushort id;

        public ushort ID
        {
            readonly get => id;
            set => id = value;
        }

        public ERewardGoalType goalType;
        public ushort goalId;

        public StatKey statKey;
        public float reward;
    }
}