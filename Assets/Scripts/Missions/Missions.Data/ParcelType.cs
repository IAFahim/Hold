using System;

namespace Missions.Missions.Data
{
    [Serializable]
    public enum ParcelType
    {
        Standard,
        Lightweight,  // Minor speed boost
        Heavy,        // Speed reduction
        Fragile       // Fails on ANY collision
    }
}