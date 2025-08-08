using System;
using Unity.Collections;

namespace Missions.Missions.Authoring
{
    [Serializable]
    public struct Description : IHasID, IEquatable<ushort>
    {
        public ushort id;
        public FixedString128Bytes description;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public bool Equals(Name other) => id == other.id;
        public override int GetHashCode() => id.GetHashCode();
        public bool Equals(ushort other) => id == other;
    }
}