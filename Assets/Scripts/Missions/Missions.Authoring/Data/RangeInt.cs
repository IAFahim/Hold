using System;

namespace Missions.Missions.Authoring
{
    [Serializable]
    public struct RangeInt : IHasID, IEquatable<ushort>
    {
        public ushort id;
        public ECheckType checkType;
        public int lower;
        public int upper;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public bool Equals(RangeInt other) => id == other.id;
        public override int GetHashCode() => id.GetHashCode();
        public bool Equals(ushort other) => id == other;
    }
}