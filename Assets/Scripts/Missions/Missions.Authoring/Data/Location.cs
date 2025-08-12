using System;
using Unity.Mathematics;

namespace Missions.Missions.Authoring
{
    [Serializable]
    public struct Location : IHasID, IEquatable<ushort>
    {
        public ushort id;
        public ushort nameId;
        public float3 position;
        public float range;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public bool Equals(Location other) => id == other.id;
        public override int GetHashCode() => id.GetHashCode();
        public bool Equals(ushort other) => id == other;
    }
}