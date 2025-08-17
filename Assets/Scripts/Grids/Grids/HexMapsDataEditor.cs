using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace HexMaps.Editor
{
    [CustomEditor(typeof(HexMapsData))]
    public class HexMapsDataEditor : UnityEditor.Editor
    {
        private int selectedLayer = 0;
        private Vector2 scrollPosition;
        private bool showGridEditor = false;

        public override void OnInspectorGUI()
        {
            var data = (HexMapsData)target;

            EditorGUI.BeginChangeCheck();

            // Default inspector for basic properties
            DrawPropertiesExcluding(serializedObject, "layers");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Map Layers", EditorStyles.boldLabel);

            // Layer management
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Layer"))
            {
                data.AddLayer($"Layer {data.layers.Count}");
                EditorUtility.SetDirty(data);
            }

            if (GUILayout.Button("Remove Selected") && data.layers.Count > 0)
            {
                data.RemoveLayer(selectedLayer);
                selectedLayer = Mathf.Min(selectedLayer, data.layers.Count - 1);
                EditorUtility.SetDirty(data);
            }

            EditorGUILayout.EndHorizontal();

            // Layer selection
            if (data.layers.Count > 0)
            {
                string[] layerNames = new string[data.layers.Count];
                for (int i = 0; i < data.layers.Count; i++)
                {
                    layerNames[i] = $"{i}: {data.layers[i].layerName}";
                }

                selectedLayer = EditorGUILayout.Popup("Selected Layer", selectedLayer, layerNames);
                selectedLayer = Mathf.Clamp(selectedLayer, 0, data.layers.Count - 1);

                // Layer properties
                var layer = data.layers[selectedLayer];
                EditorGUILayout.BeginVertical("box");
                layer.layerName = EditorGUILayout.TextField("Layer Name", layer.layerName);
                layer.description = EditorGUILayout.TextArea(layer.description);
                layer.layerColor = EditorGUILayout.ColorField("Layer Color", layer.layerColor);
                EditorGUILayout.EndVertical();

                // Import/Export
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Import/Export", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Import CSV to Layer"))
                {
                    if (data.csvImport != null)
                    {
                        HexMapsUtilities.ImportFromCSV(data, data.csvImport.text, selectedLayer);
                        EditorUtility.SetDirty(data);
                    }
                }

                if (GUILayout.Button("Export Layer to CSV"))
                {
                    string path = EditorUtility.SaveFilePanel("Export CSV", "",
                        $"{data.name}_layer_{selectedLayer}.csv", "csv");
                    if (!string.IsNullOrEmpty(path))
                    {
                        System.IO.File.WriteAllText(path, HexMapsUtilities.ExportToCSV(data, selectedLayer));
                    }
                }

                EditorGUILayout.EndHorizontal();

                // Grid Editor
                EditorGUILayout.Space();
                showGridEditor = EditorGUILayout.Foldout(showGridEditor, "Grid Editor");
                if (showGridEditor)
                {
                    DrawGridEditor(data, layer);
                }
            }

            // Utility buttons
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Utilities", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            

            if (GUILayout.Button("Find Authoring Components"))
            {
                var authorings = FindObjectsOfType<HexMapsAuthoring>();
                foreach (var authoring in authorings)
                {
                    if (authoring.Data == data)
                    {
                        Selection.activeObject = authoring.gameObject;
                        break;
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(data);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGridEditor(HexMapsData data, HexMapLayer layer)
        {
            const int cellSize = 20;
            const int maxDisplayRows = 20;
            const int maxDisplayCols = 20;

            int displayRows = Mathf.Min(data.rows, maxDisplayRows);
            int displayCols = Mathf.Min(data.columns, maxDisplayCols);

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Grid Editor ({displayRows}x{displayCols} of {data.rows}x{data.columns})",
                EditorStyles.boldLabel);

            if (data.rows > maxDisplayRows || data.columns > maxDisplayCols)
            {
                EditorGUILayout.HelpBox(
                    $"Only showing first {maxDisplayRows}x{maxDisplayCols} cells. Use CSV import/export for larger grids.",
                    MessageType.Info);
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(400));

            for (int row = 0; row < displayRows; row++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{row:D2}", GUILayout.Width(30));

                for (int col = 0; col < displayCols; col++)
                {
                    sbyte currentValue = layer.GetHeight(row, col, data.columns);

                    GUI.backgroundColor = Color.Lerp(Color.darkGray, layer.layerColor, currentValue / 255f);
                    string newValueStr = EditorGUILayout.TextField(currentValue.ToString(), GUILayout.Width(30));
                    GUI.backgroundColor = Color.white;

                    if (sbyte.TryParse(newValueStr, out sbyte newValue) && newValue != currentValue)
                    {
                        layer.SetHeight(row, col, data.columns, newValue);
                        EditorUtility.SetDirty(data);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
    }
}
#endif