using System;
using Goals.Goals.Data;
using Unity.Mathematics;
using UnityEngine;

namespace Maps.Maps.Data
{
    [Serializable]
    public struct Mission
    {
        public ushort id;
        public Segment segment;
        [Range(0, 1)] public half traveled;
        public Goal goal;
    }
}