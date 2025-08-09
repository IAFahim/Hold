using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Missions.Missions.Authoring.Editor.Graph
{
    internal static class MissionGraphCreateAllMenu
    {
        [MenuItem("Tools/Missions/Create/Run Create Nodes in Selected Mission Graph", priority = 2001)]
        private static void RunCreatorsOnSelected()
        {
            var selected = Selection.objects
                .Select(AssetDatabase.GetAssetPath)
                .Where(p => !string.IsNullOrEmpty(p) && p.EndsWith($".{MissionGraph.AssetExtension}"))
                .ToArray();

            if (selected.Length == 0)
            {
                Debug.LogWarning($"Select one or more *.{MissionGraph.AssetExtension} assets in Project window.");
                return;
            }

            int created = 0;
            foreach (var path in selected)
            {
                var graph = Unity.GraphToolkit.Editor.GraphDatabase.LoadGraph<MissionGraph>(path);
                if (graph == null) continue;
                foreach (var node in graph.GetNodes())
                {
                    if (node is ISchemaCreatorNode c)
                    {
                        c.CreateOrUpdateAsset();
                        created++;
                    }
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Schema creation finished. Updated/created via {created} creator node(s).");
        }
    }
}
