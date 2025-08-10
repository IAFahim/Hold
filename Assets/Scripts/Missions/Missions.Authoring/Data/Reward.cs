using System;

namespace Missions.Missions.Authoring
{
    [Serializable]
    public struct Reward : IHasID, IEquatable<ushort>
    {
        public ushort id;
        public ECrossLinkType crossLinkType;
        public ushort crossLinkId;
        public ushort dataContainerId;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public bool Equals(Reward other) => id == other.id;
        public override int GetHashCode() => id.GetHashCode();
        public bool Equals(ushort other) => id == other;
    }
}