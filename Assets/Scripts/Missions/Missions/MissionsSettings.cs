using Unity.Collections;
using UnityEngine;

namespace _src.Scripts.Missions.Missions
{
    public static class MissionsSettings
    {
        private static readonly FixedString32Bytes CurrentMissionKey = new("current_mission");
        public static int GetCurrentMissionID() => PlayerPrefs.GetInt(CurrentMissionKey.ToString(), 0);
    }
}