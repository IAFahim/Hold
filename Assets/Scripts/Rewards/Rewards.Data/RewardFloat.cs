using System;

namespace Rewards.Rewards.Data
{
    [Serializable]
    public struct RewardFloat
    {
        public ushort forGoalId;
        public ushort key;
        public int reward;
    }
}