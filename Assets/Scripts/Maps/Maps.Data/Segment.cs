using System;

namespace Maps.Maps.Data
{
    [Serializable]
    public struct Segment
    {
        public ushort id;
        public ushort startLocationId;
        public ushort endLocationId;
    }
}