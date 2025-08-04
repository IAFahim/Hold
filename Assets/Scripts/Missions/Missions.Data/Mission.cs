using System;
using BovineLabs.Core.ObjectManagement;
using Goals.Goals.Data.Goals;
using Maps.Maps.Data;
using Rewards.Rewards.Data;
using Rewards.Rewards.Data.GoalReward;
using Unity.Entities;

namespace Missions.Missions.Data
{
    [Serializable]
    public struct Mission
    {
        public ushort id;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public Segment segment;
        public EParcelType parcel;

        public BlobArray<ushort> goalRangeIntIndexes;
        public BlobArray<ushort> goalRangeFloatIndexes;
        
        public BlobArray<ushort> rewardIntIndexes;
        public BlobArray<ushort> rewardFloatIndexes;
    }
}