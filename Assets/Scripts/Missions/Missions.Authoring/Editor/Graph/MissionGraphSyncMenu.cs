using System.Linq;
using UnityEditor;
using UnityEngine;
using Unity.GraphToolkit.Editor;

namespace Missions.Missions.Authoring.Editor.Graph
{
    internal static class MissionGraphSyncMenu
    {
        [MenuItem("Tools/Missions/Sync Settings From Mission Graph", priority = 2000)]
        private static void SyncOpenMissionGraph()
        {
            // Prefer selected assets in Project window; fallback to prompt
            var selectedPaths = Selection.objects
                .Select(AssetDatabase.GetAssetPath)
                .Where(p => !string.IsNullOrEmpty(p))
                .ToArray();

            if (selectedPaths.Length == 0)
            {
                var path = EditorUtility.OpenFilePanel("Select Mission Graph", Application.dataPath, MissionGraph.AssetExtension);
                if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
                {
                    var rel = "Assets" + path.Substring(Application.dataPath.Length);
                    selectedPaths = new[] { rel };
                }
            }

            if (selectedPaths.Length == 0)
            {
                Debug.LogWarning($"Select a *.{MissionGraph.AssetExtension} asset in Project window or pick one in the file dialog.");
                return;
            }

            int totalChanged = 0;
            foreach (var assetPath in selectedPaths)
            {
                if (!assetPath.EndsWith($".{MissionGraph.AssetExtension}"))
                    continue;

                var graph = GraphDatabase.LoadGraph<MissionGraph>(assetPath);
                if (graph == null)
                    continue;

                int changed = 0;
                foreach (var node in graph.GetNodes())
                {
                    switch (node)
                    {
                        case GoalSettingsSinkNode n: n.Sync(); changed++; break;
                        case MissionSettingsSinkNode n: n.Sync(); changed++; break;
                        case LocationSettingsSinkNode n: n.Sync(); changed++; break;
                        case RangeFloatSettingsSinkNode n: n.Sync(); changed++; break;
                        case RangeIntSettingsSinkNode n: n.Sync(); changed++; break;
                        case RewardSettingsSinkNode n: n.Sync(); changed++; break;
                        case TimeSettingsSinkNode n: n.Sync(); changed++; break;
                        case DataContainerSettingsSinkNode n: n.Sync(); changed++; break;
                        case NameSettingsSinkNode n: n.Sync(); changed++; break;
                        case DescriptionSettingsSinkNode n: n.Sync(); changed++; break;
                        case ItemSettingsSinkNode n: n.Sync(); changed++; break;
                    }
                }
                totalChanged += changed;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"MissionGraph sync complete. Updated {totalChanged} settings nodes across {selectedPaths.Length} asset(s).");
        }
    }
}
