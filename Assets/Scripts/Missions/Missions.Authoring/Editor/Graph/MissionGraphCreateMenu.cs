using UnityEditor;
using Unity.GraphToolkit.Editor;

namespace Missions.Missions.Authoring.Editor.Graph
{
    internal static class MissionGraphCreateMenu
    {
        [MenuItem("Assets/Create/Missions/Mission Graph", priority = 302)]
        private static void CreateMissionGraph()
        {
            GraphDatabase.PromptInProjectBrowserToCreateNewAsset<MissionGraph>("MissionGraph");
        }
    }
}
