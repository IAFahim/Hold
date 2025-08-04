using System;

namespace Rewards.Rewards.Data
{
    [Serializable]
    public struct GoalRewardInt
    {
        public ushort goalId;
        public ushort key;
        public int reward;
    }
}