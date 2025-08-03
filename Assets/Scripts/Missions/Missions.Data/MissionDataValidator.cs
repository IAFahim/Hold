using UnityEngine;
using Missions.Missions.Data;

namespace Missions.Missions.Data.Tests
{
    /// <summary>
    /// Simple validation script to test mission creation and data integrity
    /// </summary>
    public class MissionDataValidator : MonoBehaviour
    {
        [Header("Test Results")]
        [SerializeField] private bool allTestsPassed = false;
        [SerializeField] private string testResults = "";
        
        [ContextMenu("Run Mission Data Tests")]
        public void RunTests()
        {
            testResults = "";
            bool success = true;
            
            try
            {
                // Test Mission 1
                var mission1 = IntroductoryMissionsFactory.CreateMission01FirstTrackRun();
                success &= ValidateMission(mission1, "Mission 1");
                
                // Test Mission 2
                var mission2 = IntroductoryMissionsFactory.CreateMission02CoffeeKickstart();
                success &= ValidateMission(mission2, "Mission 2");
                
                // Test Mission 6 (Fragile)
                var mission6 = IntroductoryMissionsFactory.CreateMission06HandleWithCare();
                success &= ValidateMission(mission6, "Mission 6");
                
                // Test Mission 16 (Heavy)
                var mission16 = IntroductoryMissionsFactory.CreateMission16JatinDasDeadweight();
                success &= ValidateMission(mission16, "Mission 16");
                
                // Test parcel type validation
                success &= TestParcelTypeValidation();
                
                allTestsPassed = success;
                testResults += success ? "\n✅ All tests passed!" : "\n❌ Some tests failed!";
                
                Debug.Log($"Mission Data Validation: {(success ? "PASSED" : "FAILED")}\n{testResults}");
            }
            catch (System.Exception e)
            {
                allTestsPassed = false;
                testResults += $"\n❌ Exception during testing: {e.Message}";
                Debug.LogError($"Mission Data Validation failed with exception: {e}");
            }
        }
        
        private bool ValidateMission(MissionData mission, string missionName)
        {
            bool isValid = true;
            
            // Basic validation
            if (mission == null)
            {
                testResults += $"\n❌ {missionName}: Mission is null";
                return false;
            }
            
            if (mission.missionId == 0)
            {
                testResults += $"\n❌ {missionName}: Invalid mission ID";
                isValid = false;
            }
            
            if (string.IsNullOrEmpty(mission.missionName))
            {
                testResults += $"\n❌ {missionName}: Missing mission name";
                isValid = false;
            }
            
            if (string.IsNullOrEmpty(mission.description))
            {
                testResults += $"\n❌ {missionName}: Missing description";
                isValid = false;
            }
            
            if (string.IsNullOrEmpty(mission.startStation) || string.IsNullOrEmpty(mission.endStation))
            {
                testResults += $"\n❌ {missionName}: Missing station information";
                isValid = false;
            }
            
            if (mission.baseReward <= 0)
            {
                testResults += $"\n❌ {missionName}: Invalid base reward";
                isValid = false;
            }
            
            if (string.IsNullOrEmpty(mission.parcel.name))
            {
                testResults += $"\n❌ {missionName}: Missing parcel name";
                isValid = false;
            }
            
            // Test helper methods
            string formattedTitle = mission.GetFormattedTitle();
            if (string.IsNullOrEmpty(formattedTitle))
            {
                testResults += $"\n❌ {missionName}: GetFormattedTitle() failed";
                isValid = false;
            }
            
            string routeDesc = mission.GetRouteDescription();
            if (string.IsNullOrEmpty(routeDesc))
            {
                testResults += $"\n❌ {missionName}: GetRouteDescription() failed";
                isValid = false;
            }
            
            int totalReward = mission.GetTotalPossibleReward();
            if (totalReward < mission.baseReward)
            {
                testResults += $"\n❌ {missionName}: GetTotalPossibleReward() logic error";
                isValid = false;
            }
            
            if (isValid)
            {
                testResults += $"\n✅ {missionName}: All validations passed";
                testResults += $"\n   - ID: {mission.missionId}, Name: {mission.missionName}";
                testResults += $"\n   - Route: {mission.GetRouteDescription()}";
                testResults += $"\n   - Parcel: {mission.parcel.name} ({mission.parcel.type})";
                testResults += $"\n   - Reward: ₹{mission.GetTotalPossibleReward()}";
                testResults += $"\n   - Time Limit: {(mission.HasTimeLimit() ? $"{mission.timeLimit}s" : "None")}";
            }
            
            return isValid;
        }
        
        private bool TestParcelTypeValidation()
        {
            bool success = true;
            
            // Test fragile parcel auto-configuration
            var fragileMission = IntroductoryMissionsFactory.CreateMission06HandleWithCare();
            if (!fragileMission.parcel.failsOnCollision)
            {
                testResults += "\n❌ Fragile parcel validation: failsOnCollision not set";
                success = false;
            }
            
            if (!fragileMission.obstacleConfig.zeroToleranceMode)
            {
                testResults += "\n❌ Fragile parcel validation: zeroToleranceMode not set";
                success = false;
            }
            
            // Test heavy parcel auto-configuration
            var heavyMission = IntroductoryMissionsFactory.CreateMission16JatinDasDeadweight();
            if (heavyMission.parcel.speedMultiplier >= 1.0f)
            {
                testResults += "\n❌ Heavy parcel validation: speedMultiplier should be < 1.0";
                success = false;
            }
            
            if (success)
            {
                testResults += "\n✅ Parcel type validation: All auto-configurations working";
            }
            
            return success;
        }
        
        void Start()
        {
            // Auto-run tests in development builds
            #if DEVELOPMENT_BUILD || UNITY_EDITOR
            RunTests();
            #endif
        }
    }
}