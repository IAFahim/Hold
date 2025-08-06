using System;
using Unity.Entities;

namespace Missions.Missions.Authoring.Data
{
    [Serializable]
    public struct Mission : IHasID, IEquatable<ushort>
    {
        public ushort id;
        public ushort station;
        public ushort name;
        public BlobArray<ushort> Goals;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }

        public override int GetHashCode() => id.GetHashCode();
        public bool Equals(ushort other) => id == other;
        
    }
}