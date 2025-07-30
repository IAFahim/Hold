using System;
using Unity.Collections;
using Unity.Entities;

namespace Goals.Goals.Data
{
    [Serializable]
    public struct GoalTable
    {
        public ushort key;
        public byte conditionType;
        public ECheckType checkType;
        public ushort value;

        // public BlobArray<GoalTable> CreateBlobArray(GoalTable[] goalTables)
        // {
        //     var builder = new BlobBuilder(Allocator.Temp);
        //
        // }
    }
}