using System;
using BovineLabs.Core.ObjectManagement;
using Goals.Goals.Data.Goals;
using Rewards.Rewards.Data;

namespace Maps.Maps.Data
{
    [Serializable]
    public struct Mission : IUID
    {
        public ushort id;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public Segment segment;

        public GoalRangeInt[] goalRangeInts;
        public GoalRangeFloat[] goalRangeFloats;

        public GoalRewardInt[] rewardInts;
        public GoalRewardFloat[] rewardFloats;
    }
}