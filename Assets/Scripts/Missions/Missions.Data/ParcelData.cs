using System;
using UnityEngine;

namespace Missions.Missions.Data
{
    [Serializable]
    public struct ParcelData
    {
        [Header("Parcel Information")]
        public string name;
        public ParcelType type;
        
        [Header("Effects")]
        [Tooltip("Speed multiplier for lightweight/heavy parcels")]
        [Range(0.1f, 2.0f)]
        public float speedMultiplier;
        
        [Tooltip("Fails mission on collision (for fragile parcels)")]
        public bool failsOnCollision;
    }
}