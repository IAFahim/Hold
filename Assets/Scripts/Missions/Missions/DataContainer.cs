using System;

namespace Data
{
    [Serializable]
    public struct DataContainer : IHasID, IEquatable<ushort>
    {
        public ushort id;
        public NumType numType;
        public TargetType targetType;
        public float valueFloat;
        public int valueInt;

        public int ID
        {
            get => id;
            set => id = (ushort)value;
        }
        
        public bool Equals(DataContainer other) => id == other.id;
        public override int GetHashCode() => id.GetHashCode();
        public bool Equals(ushort other) => id == other;
    }
}