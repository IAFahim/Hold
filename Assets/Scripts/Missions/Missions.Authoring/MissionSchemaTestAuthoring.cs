using System;
using Missions.Missions.Data;
using UnityEngine;

namespace Missions.Missions.Authoring
{
    public class MissionSchemaTestAuthoring : MonoBehaviour
    {
        [Header("Mission Schema to Test")]
        public MissionSchema missionSchema;
        
        [Header("Test Controls")]
        [SerializeField] private bool runTestOnStart = true;
        [SerializeField] private bool logDetailedInfo = true;
        
        [Header("Test Results (Read Only)")]
        [SerializeField, ReadOnly] private bool testPassed;
        [SerializeField, ReadOnly] private string testResults;

        private void OnValidate()
        {
            if (runTestOnStart && missionSchema != null)
            {
                RunValidationTest();
            }
        }

        [ContextMenu("Run Validation Test")]
        public void RunValidationTest()
        {
            if (missionSchema == null)
            {
                testResults = "ERROR: No MissionSchema assigned!";
                testPassed = false;
                Debug.LogError(testResults);
                return;
            }

            try
            {
                // Test ToBlobAssetReference
                var blobAsset = missionSchema.ToBlobAssetReference();
                
                // Validate the blob asset
                ref var mission = ref blobAsset.Value;
                
                var results = ValidateMission(ref mission);
                testResults = results.message;
                testPassed = results.success;
                
                if (logDetailedInfo)
                {
                    LogDetailedInfo(ref mission);
                }
                
                // Clean up
                blobAsset.Dispose();
                
                if (testPassed)
                    Debug.Log($"✓ Mission Schema Test PASSED: {testResults}");
                else
                    Debug.LogError($"✗ Mission Schema Test FAILED: {testResults}");
            }
            catch (Exception ex)
            {
                testResults = $"EXCEPTION: {ex.Message}";
                testPassed = false;
                Debug.LogException(ex);
            }
        }

        private (bool success, string message) ValidateMission(ref Mission mission)
        {
            // Basic validation
            if (mission.id != missionSchema.id)
                return (false, $"ID mismatch: expected {missionSchema.id}, got {mission.id}");
            
            if (!mission.segment.Equals(missionSchema.segment))
                return (false, "Segment mismatch");
                
            if (mission.parcel != missionSchema.parcel)
                return (false, $"Parcel mismatch: expected {missionSchema.parcel}, got {mission.parcel}");

            // Array length validation
            if (mission.goalRangeIntIndexes.Length != missionSchema.goalRangeInts.Length)
                return (false, $"GoalRangeInt array length mismatch: expected {missionSchema.goalRangeInts.Length}, got {mission.goalRangeIntIndexes.Length}");
                
            if (mission.goalRangeFloatIndexes.Length != missionSchema.goalRangeFloats.Length)
                return (false, $"GoalRangeFloat array length mismatch: expected {missionSchema.goalRangeFloats.Length}, got {mission.goalRangeFloatIndexes.Length}");
                
            if (mission.rewardIntIndexes.Length != missionSchema.rewardInts.Length)
                return (false, $"RewardInt array length mismatch: expected {missionSchema.rewardInts.Length}, got {mission.rewardIntIndexes.Length}");
                
            if (mission.rewardFloatIndexes.Length != missionSchema.rewardFloats.Length)
                return (false, $"RewardFloat array length mismatch: expected {missionSchema.rewardFloats.Length}, got {mission.rewardFloatIndexes.Length}");

            // Array content validation
            for (int i = 0; i < mission.goalRangeIntIndexes.Length; i++)
            {
                if (mission.goalRangeIntIndexes[i] != missionSchema.goalRangeInts[i].id)
                    return (false, $"GoalRangeInt[{i}] ID mismatch: expected {missionSchema.goalRangeInts[i].id}, got {mission.goalRangeIntIndexes[i]}");
            }
            
            for (int i = 0; i < mission.goalRangeFloatIndexes.Length; i++)
            {
                if (mission.goalRangeFloatIndexes[i] != missionSchema.goalRangeFloats[i].id)
                    return (false, $"GoalRangeFloat[{i}] ID mismatch: expected {missionSchema.goalRangeFloats[i].id}, got {mission.goalRangeFloatIndexes[i]}");
            }
            
            for (int i = 0; i < mission.rewardIntIndexes.Length; i++)
            {
                if (mission.rewardIntIndexes[i] != missionSchema.rewardInts[i].id)
                    return (false, $"RewardInt[{i}] ID mismatch: expected {missionSchema.rewardInts[i].id}, got {mission.rewardIntIndexes[i]}");
            }
            
            for (int i = 0; i < mission.rewardFloatIndexes.Length; i++)
            {
                if (mission.rewardFloatIndexes[i] != missionSchema.rewardFloats[i].id)
                    return (false, $"RewardFloat[{i}] ID mismatch: expected {missionSchema.rewardFloats[i].id}, got {mission.rewardFloatIndexes[i]}");
            }

            return (true, "All validations passed successfully!");
        }

        private void LogDetailedInfo(ref Mission mission)
        {
            Debug.Log($"=== Mission Schema Validation Details ===");
            Debug.Log($"Mission ID: {mission.id}");
            Debug.Log($"Segment: {mission.segment}");
            Debug.Log($"Parcel: {mission.parcel}");
            Debug.Log($"GoalRangeInt Count: {mission.goalRangeIntIndexes.Length}");
            Debug.Log($"GoalRangeFloat Count: {mission.goalRangeFloatIndexes.Length}");
            Debug.Log($"RewardInt Count: {mission.rewardIntIndexes.Length}");
            Debug.Log($"RewardFloat Count: {mission.rewardFloatIndexes.Length}");
            
            if (mission.goalRangeIntIndexes.Length > 0)
            {
                var ids = new ushort[mission.goalRangeIntIndexes.Length];
                for (int i = 0; i < ids.Length; i++) ids[i] = mission.goalRangeIntIndexes[i];
                Debug.Log($"GoalRangeInt IDs: [{string.Join(", ", ids)}]");
            }
            
            if (mission.goalRangeFloatIndexes.Length > 0)
            {
                var ids = new ushort[mission.goalRangeFloatIndexes.Length];
                for (int i = 0; i < ids.Length; i++) ids[i] = mission.goalRangeFloatIndexes[i];
                Debug.Log($"GoalRangeFloat IDs: [{string.Join(", ", ids)}]");
            }
            
            if (mission.rewardIntIndexes.Length > 0)
            {
                var ids = new ushort[mission.rewardIntIndexes.Length];
                for (int i = 0; i < ids.Length; i++) ids[i] = mission.rewardIntIndexes[i];
                Debug.Log($"RewardInt IDs: [{string.Join(", ", ids)}]");
            }
            
            if (mission.rewardFloatIndexes.Length > 0)
            {
                var ids = new ushort[mission.rewardFloatIndexes.Length];
                for (int i = 0; i < ids.Length; i++) ids[i] = mission.rewardFloatIndexes[i];
                Debug.Log($"RewardFloat IDs: [{string.Join(", ", ids)}]");
            }
        }

        // Helper attribute for read-only fields in inspector
        public class ReadOnlyAttribute : PropertyAttribute { }
    }
}