using Unity.Entities;

namespace Goals.Goals.Data
{
    public struct Goal
    {
        public ushort ID;
        public byte GoalType;
        public byte GoalIndex;
    }

    public struct GoalTable
    {
        public BlobArray<Goal> Goals;
    }
}