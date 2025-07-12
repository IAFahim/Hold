using System;
using Unity.Mathematics;

namespace StatsHelpers.StatsHelpers.Data
{
    [Serializable]
    public struct KvIntrinsic
    {
        public byte key;
        public short value;
    }
    
    [Serializable]
    public struct KvStat
    {
        public byte key;
        public short added;
        public half multi;
    }
}