using System;
using UnityEngine;
using Unity.Collections;

namespace Missions.Missions.Data
{
    [CreateAssetMenu(fileName = "New Mission", menuName = "Hold/Mission Data", order = 1)]
    [Serializable]
    public class MissionData : ScriptableObject
    {
        [Header("Mission Identity")]
        [Tooltip("Unique mission identifier")]
        public ushort missionId;
        
        [Tooltip("Display name for the mission")]
        public string missionName = "Mission Name";
        
        [TextArea(3, 5)]
        [Tooltip("Mission description shown to player")]
        public string description = "Mission description";
        
        [Header("Route Information")]
        [Tooltip("Starting station name")]
        public string startStation = "Start Station";
        
        [Tooltip("Starting station code")]
        public FixedString32Bytes startStationCode = "STR";
        
        [Tooltip("End station name")]
        public string endStation = "End Station";
        
        [Tooltip("End station code")]
        public FixedString32Bytes endStationCode = "END";
        
        [Header("Parcel Details")]
        public ParcelData parcel;
        
        [Header("Mission Parameters")]
        [Tooltip("Time limit in seconds (0 = no limit)")]
        [Range(0, 300)]
        public float timeLimit = 0f;
        
        [Header("Rewards")]
        [Tooltip("Base reward amount in rupees")]
        [Range(0, 500)]
        public int baseReward = 50;
        
        [Tooltip("Bonus reward for perfect run")]
        [Range(0, 100)]
        public int bonusReward = 0;
        
        [Header("Objectives")]
        [Tooltip("Primary objective for this mission")]
        public ObjectiveType primaryObjective;
        
        [Tooltip("Additional secondary objectives")]
        public ObjectiveType[] secondaryObjectives;
        
        [Header("Difficulty & Rules")]
        public ObstacleConfiguration obstacleConfig;
        
        [Header("Mission Flags")]
        [Tooltip("Tutorial mission with hints")]
        public bool isTutorial = false;
        
        [Tooltip("Express route (longer distance)")]
        public bool isExpressRoute = false;
        
        [Tooltip("Critical delivery (higher stakes)")]
        public bool isCriticalDelivery = false;
        
        #if UNITY_EDITOR
        [Header("Development Notes")]
        [TextArea(2, 4)]
        [Tooltip("Internal notes for developers")]
        public string devNotes = "";
        #endif
        
        /// <summary>
        /// Initialize mission data with default values for specific parcel types
        /// </summary>
        void OnValidate()
        {
            // Auto-configure parcel effects based on type
            switch (parcel.type)
            {
                case ParcelType.Standard:
                    parcel.speedMultiplier = 1.0f;
                    parcel.failsOnCollision = false;
                    break;
                case ParcelType.Lightweight:
                    parcel.speedMultiplier = 1.2f;
                    parcel.failsOnCollision = false;
                    break;
                case ParcelType.Heavy:
                    parcel.speedMultiplier = 0.8f;
                    parcel.failsOnCollision = false;
                    break;
                case ParcelType.Fragile:
                    parcel.speedMultiplier = 1.0f;
                    parcel.failsOnCollision = true;
                    break;
            }
            
            // Auto-configure zero tolerance based on fragile parcels or critical delivery
            if (parcel.type == ParcelType.Fragile || isCriticalDelivery)
            {
                obstacleConfig.zeroToleranceMode = true;
                obstacleConfig.damageLimit = 0;
            }
        }
        
        /// <summary>
        /// Get formatted mission title for UI display
        /// </summary>
        public string GetFormattedTitle()
        {
            return $"Mission {missionId}: {missionName}";
        }
        
        /// <summary>
        /// Get route description for UI display
        /// </summary>
        public string GetRouteDescription()
        {
            return $"{startStation} ({startStationCode}) → {endStation} ({endStationCode})";
        }
        
        /// <summary>
        /// Get total possible reward including bonuses
        /// </summary>
        public int GetTotalPossibleReward()
        {
            return baseReward + bonusReward;
        }
        
        /// <summary>
        /// Check if mission has time pressure
        /// </summary>
        public bool HasTimeLimit()
        {
            return timeLimit > 0f;
        }
    }
}