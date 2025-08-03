using UnityEngine;
using Missions.Missions.Data;

namespace Missions.Missions.Data
{
    /// <summary>
    /// Factory class for creating predefined missions from the introductory mission set
    /// </summary>
    public static class IntroductoryMissionsFactory
    {
        /// <summary>
        /// Create Mission 1: First Track Run
        /// </summary>
        public static MissionData CreateMission01FirstTrackRun()
        {
            var mission = ScriptableObject.CreateInstance<MissionData>();
            mission.name = "Mission01_FirstTrackRun";
            
            // Mission Identity
            mission.missionId = 1;
            mission.missionName = "First Track Run";
            mission.description = "Your first hustle. Just get the package to Baranagar. Watch out for junk on the tracks.";
            
            // Route Information
            mission.startStation = "Dakshineswar";
            mission.startStationCode = "DN";
            mission.endStation = "Baranagar";
            mission.endStationCode = "BRN";
            
            // Parcel Details
            mission.parcel.name = "Bootleg Music Chip";
            mission.parcel.type = ParcelType.Standard;
            
            // Mission Parameters
            mission.timeLimit = 0f; // No time limit
            
            // Rewards
            mission.baseReward = 50;
            mission.bonusReward = 0;
            
            // Objectives
            mission.primaryObjective = ObjectiveType.LearnBasicLaneSwitching;
            mission.secondaryObjectives = new ObjectiveType[0];
            
            // Difficulty & Rules
            mission.obstacleConfig.obstacleTypes = new ObstacleType[] { ObstacleType.Static };
            mission.obstacleConfig.density = 0.5f; // Few obstacles
            mission.obstacleConfig.trackBonusDensity = 1.0f; // Standard amount
            mission.obstacleConfig.hasNearMissBonus = false;
            mission.obstacleConfig.policeRadarActive = false;
            mission.obstacleConfig.zeroToleranceMode = false;
            mission.obstacleConfig.damageLimit = 10; // No damage limit
            
            // Mission Flags
            mission.isTutorial = true;
            mission.isExpressRoute = false;
            mission.isCriticalDelivery = false;
            
            #if UNITY_EDITOR
            mission.devNotes = "Tutorial mission focusing on basic left/right swipes. Few static obstacles only.";
            #endif
            
            return mission;
        }
        
        /// <summary>
        /// Create Mission 2: Coffee Kickstart
        /// </summary>
        public static MissionData CreateMission02CoffeeKickstart()
        {
            var mission = ScriptableObject.CreateInstance<MissionData>();
            mission.name = "Mission02_CoffeeKickstart";
            
            // Mission Identity
            mission.missionId = 2;
            mission.missionName = "Coffee Kickstart";
            mission.description = "Deliver these rare coffee beans fast! Use ramps to jump over low barriers.";
            
            // Route Information
            mission.startStation = "Baranagar";
            mission.startStationCode = "BRN";
            mission.endStation = "Noapara";
            mission.endStationCode = "NP";
            
            // Parcel Details
            mission.parcel.name = "Homegrown Coffee Beans";
            mission.parcel.type = ParcelType.Standard;
            
            // Mission Parameters
            mission.timeLimit = 70f; // 70 seconds
            
            // Rewards
            mission.baseReward = 60;
            mission.bonusReward = 0;
            
            // Objectives
            mission.primaryObjective = ObjectiveType.IntroduceJumpMechanic;
            mission.secondaryObjectives = new ObjectiveType[0];
            
            // Difficulty & Rules
            mission.obstacleConfig.obstacleTypes = new ObstacleType[] { ObstacleType.Static, ObstacleType.Jump };
            mission.obstacleConfig.density = 0.7f;
            mission.obstacleConfig.trackBonusDensity = 1.0f; // Standard amount
            mission.obstacleConfig.hasNearMissBonus = false;
            mission.obstacleConfig.policeRadarActive = false;
            mission.obstacleConfig.zeroToleranceMode = false;
            mission.obstacleConfig.damageLimit = 10;
            
            // Mission Flags
            mission.isTutorial = true;
            mission.isExpressRoute = false;
            mission.isCriticalDelivery = false;
            
            #if UNITY_EDITOR
            mission.devNotes = "Introduces jump mechanic with gentle time pressure. Static obstacles + low barriers/ramps.";
            #endif
            
            return mission;
        }
        
        /// <summary>
        /// Create Mission 6: Handle With Care (Fragile Example)
        /// </summary>
        public static MissionData CreateMission06HandleWithCare()
        {
            var mission = ScriptableObject.CreateInstance<MissionData>();
            mission.name = "Mission06_HandleWithCare";
            
            // Mission Identity
            mission.missionId = 6;
            mission.missionName = "Handle With Care";
            mission.description = "Fragile antique data crystal. One bump, it's dust. Perfect run required, NO collisions.";
            
            // Route Information
            mission.startStation = "Shyambazar";
            mission.startStationCode = "SBZ";
            mission.endStation = "Shobhabazar Sutanuti";
            mission.endStationCode = "SBS";
            
            // Parcel Details
            mission.parcel.name = "Antique Data Crystal";
            mission.parcel.type = ParcelType.Fragile; // Fails on ANY collision
            
            // Mission Parameters
            mission.timeLimit = 0f; // No time limit
            
            // Rewards
            mission.baseReward = 150; // High reward for perfection
            mission.bonusReward = 0;
            
            // Objectives
            mission.primaryObjective = ObjectiveType.IntroduceFragileParcelType;
            mission.secondaryObjectives = new ObjectiveType[0];
            
            // Difficulty & Rules
            mission.obstacleConfig.obstacleTypes = new ObstacleType[] { ObstacleType.Static, ObstacleType.Jump, ObstacleType.Slide };
            mission.obstacleConfig.density = 1.0f; // Carefully placed
            mission.obstacleConfig.trackBonusDensity = 0.3f; // Low amount (focus on survival)
            mission.obstacleConfig.hasNearMissBonus = false;
            mission.obstacleConfig.policeRadarActive = false;
            mission.obstacleConfig.zeroToleranceMode = true; // Auto-set by fragile parcel
            mission.obstacleConfig.damageLimit = 0; // Auto-set by fragile parcel
            
            // Mission Flags
            mission.isTutorial = false;
            mission.isExpressRoute = false;
            mission.isCriticalDelivery = true;
            
            #if UNITY_EDITOR
            mission.devNotes = "Introduces fragile parcel type with zero tolerance. Requires precise movement through mixed obstacles.";
            #endif
            
            return mission;
        }
        
        /// <summary>
        /// Create Mission 16: Jatin Das Deadweight (Heavy Parcel Example)
        /// </summary>
        public static MissionData CreateMission16JatinDasDeadweight()
        {
            var mission = ScriptableObject.CreateInstance<MissionData>();
            mission.name = "Mission16_JatinDasDeadweight";
            
            // Mission Identity
            mission.missionId = 16;
            mission.missionName = "Jatin Das Deadweight";
            mission.description = "Hauling heavy server parts. You'll be slower, making dodges trickier. Plan your moves carefully.";
            
            // Route Information
            mission.startStation = "Netaji Bhavan";
            mission.startStationCode = "NB";
            mission.endStation = "Jatin Das Park";
            mission.endStationCode = "JDP";
            
            // Parcel Details
            mission.parcel.name = "Bulk Server Components";
            mission.parcel.type = ParcelType.Heavy; // Imposes speed limit
            
            // Mission Parameters
            mission.timeLimit = 95f; // 95 seconds
            
            // Rewards
            mission.baseReward = 150;
            mission.bonusReward = 0;
            
            // Objectives
            mission.primaryObjective = ObjectiveType.IntroduceHeavyHaul;
            mission.secondaryObjectives = new ObjectiveType[0];
            
            // Difficulty & Rules
            mission.obstacleConfig.obstacleTypes = new ObstacleType[] { 
                ObstacleType.Static, ObstacleType.Jump, ObstacleType.Slide 
            };
            mission.obstacleConfig.density = 1.0f; // Standard density, but feels harder due to speed reduction
            mission.obstacleConfig.trackBonusDensity = 1.0f; // Standard amount
            mission.obstacleConfig.hasNearMissBonus = false;
            mission.obstacleConfig.policeRadarActive = false;
            mission.obstacleConfig.zeroToleranceMode = false;
            mission.obstacleConfig.damageLimit = 10;
            
            // Mission Flags
            mission.isTutorial = false;
            mission.isExpressRoute = false;
            mission.isCriticalDelivery = false;
            
            #if UNITY_EDITOR
            mission.devNotes = "Heavy parcel reduces player speed. Standard obstacles feel harder due to reduced maneuverability.";
            #endif
            
            return mission;
        }
    }
}