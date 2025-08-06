using System;

namespace Missions.Missions.Authoring
{
    [Serializable]
    public struct RangeFloat : IHasID, IEquatable<ushort>
    {
        public ushort id;
        public ECheckType checkType;
        public float lower;
        public float upper;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public bool Equals(RangeFloat other) => id == other.id;
        public override int GetHashCode() => id.GetHashCode();
        public bool Equals(ushort other) => id == other;
    }
}