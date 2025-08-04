using System;

namespace Rewards.Rewards.Data
{
    [Serializable]
    public struct RewardInt
    {
        public ushort forGoalId;
        public ushort key;
        public int reward;
    }
}