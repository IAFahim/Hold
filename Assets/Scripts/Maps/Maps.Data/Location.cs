using System;
using Unity.Collections;
using Unity.Mathematics;

namespace Maps.Maps.Data
{
    [Serializable]
    public struct Location
    {
        public FixedString32Bytes name;
        public ushort id;
        public float3 position;
    }
}