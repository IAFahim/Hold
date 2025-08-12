using System;

namespace Missions.Missions.Authoring
{
    [Serializable]
    public struct Goal : IHasID, IEquatable<ushort>
    {
        public ushort id;
        public ETargetType targetType;
        public ENumType rangeType;
        public ushort rangeId;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public bool Equals(Goal other) => id == other.id;
        public override int GetHashCode() => id.GetHashCode();
        public bool Equals(ushort other) => id == other;
    }
}