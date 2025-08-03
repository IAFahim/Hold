using System;
using UnityEngine;

namespace Missions.Missions.Data
{
    [Serializable]
    public struct ObstacleConfiguration
    {
        [Header("Obstacle Setup")]
        public ObstacleType[] obstacleTypes;
        
        [Header("Density & Difficulty")]
        [Tooltip("Overall obstacle density")]
        [Range(0.1f, 2.0f)]
        public float density;
        
        [Tooltip("Track bonus cash density")]
        [Range(0.1f, 3.0f)]
        public float trackBonusDensity;
        
        [Header("Special Features")]
        [Tooltip("Has near miss bonus opportunities")]
        public bool hasNearMissBonus;
        
        [Tooltip("Police radar warning system active")]
        public bool policeRadarActive;
        
        [Tooltip("Zero tolerance mode (immediate fail on collision)")]
        public bool zeroToleranceMode;
        
        [Tooltip("Damage limit before failure")]
        [Range(0, 10)]
        public int damageLimit;
    }
}