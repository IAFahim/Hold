using System;

namespace Missions.Missions.Authoring
{
    [Serializable]
    public struct Time : IHasID, IEquatable<ushort>
    {
        public ushort id;
        public ECrossLinkType crossLinkType;
        public ushort rangeFloatId;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public bool Equals(Time other) => id == other.id;
        public override int GetHashCode() => id.GetHashCode();
        public bool Equals(ushort other) => id == other;
    }
}