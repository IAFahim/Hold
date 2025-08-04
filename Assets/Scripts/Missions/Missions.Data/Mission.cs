using System;
using BovineLabs.Core.ObjectManagement;
using Goals.Goals.Data.Goals;
using Rewards.Rewards.Data;
using UnityEngine;

namespace Maps.Maps.Data
{
    [Serializable]
    public class Mission : ScriptableObject, IUID
    {
        public ushort id;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public Segment segment;

        public GoalInt[] goalInts;
        public GoalRangeInt[] goalRangeInts;

        public GoalFloat[] goalFloats;
        public GoalRangeFloat[] goalRangeFloats;

        public RewardInt[] rewardInts;
        public RewardFloat[] rewardFloats;

        public GoalTime time;
    }
}