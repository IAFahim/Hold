using System;

namespace Rewards.Rewards.Data
{
    [Serializable]
    public struct GoalRewardFloat
    {
        public ushort goalId;
        public ushort key;
        public int reward;
    }
}