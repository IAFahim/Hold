using Missions.Missions.Authoring.Data;

namespace Missions.Missions.Authoring
{
    internal static class MissionExt{
        public static ushort ToData(this ref Mission mission) => mission.id;
    }
}