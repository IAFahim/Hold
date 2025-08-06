using System;
using Unity.Mathematics;

namespace Missions.Missions.Authoring
{
    [Serializable]
    public struct Station : IHasID, IEquatable<ushort>
    {
        public ushort id;
        public ushort nameId;
        public float3 position;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public bool Equals(Station other) => id == other.id;
        public override int GetHashCode() => id.GetHashCode();
        public bool Equals(ushort other) => id == other;
    }
}