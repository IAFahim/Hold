using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Missions.Missions.Authoring.Schemas;
using Missions.Missions.Authoring.Settings;

namespace Missions.Missions.Authoring.Editor
{
    public class NameSchemaCreatorWindow : EditorWindow
    {
        private string _nameInput = "";
        private Vector2 _scrollPosition;
        private NameSettings _nameSettings;
        private bool _showExistingNames = true;
        private string _outputPath = "Assets/ScriptableObjects/NameSchemas";

        [MenuItem("Tools/Schema/Name Schema Creator")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<NameSchemaCreatorWindow>("Name Schema Creator");
            wnd.minSize = new Vector2(600, 420);
            wnd.titleContent = new GUIContent("Name Schema Creator", EditorGUIUtility.IconContent("d_TextAsset Icon").image);
        }

        [MenuItem("Tools/Schema/Name Schema Creator (No Focus)")]
        public static void ShowWindowNoFocus()
        {
            var wnd = GetWindow<NameSchemaCreatorWindow>("Name Schema Creator", false);
            wnd.minSize = new Vector2(600, 420);
            wnd.titleContent = new GUIContent("Name Schema Creator", EditorGUIUtility.IconContent("d_TextAsset Icon").image);
        }

        [MenuItem("Tools/Schema/Name Schema Creator (Utility)")]
        public static void ShowWindowUtility()
        {
            var wnd = CreateInstance<NameSchemaCreatorWindow>();
            wnd.titleContent = new GUIContent("Name Schema Creator", EditorGUIUtility.IconContent("d_TextAsset Icon").image);
            wnd.minSize = new Vector2(600, 420);
            wnd.ShowUtility();
        }

        private void OnEnable()
        {
            LoadNameSettings();
            _outputPath = EditorPrefs.GetString(CreatePath, _outputPath);
        }

        private static string CreatePath => $"{nameof(NameSchema)}_Editor_Create_Path";

        private void LoadNameSettings()
        {
            // Try to find existing NameSettings
            string[] guids = AssetDatabase.FindAssets("t:NameSettings");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _nameSettings = AssetDatabase.LoadAssetAtPath<NameSettings>(path);
            }

            // Create NameSettings if it doesn't exist
            if (_nameSettings == null)
            {
                _nameSettings = CreateInstance<NameSettings>();
                string settingsPath = "Assets/Settings";
                if (!AssetDatabase.IsValidFolder(settingsPath))
                {
                    AssetDatabase.CreateFolder("Assets", "Settings");
                }

                AssetDatabase.CreateAsset(_nameSettings, $"{settingsPath}/NameSettings.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("Name Schema Creator", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // Settings reference
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name Settings:", GUILayout.Width(100));
            _nameSettings = (NameSettings)EditorGUILayout.ObjectField(_nameSettings, typeof(NameSettings), false);
            if (GUILayout.Button("Refresh", GUILayout.Width(60)))
            {
                LoadNameSettings();
            }

            EditorGUILayout.EndHorizontal();

            if (_nameSettings == null)
            {
                EditorGUILayout.HelpBox("NameSettings not found. Please assign or create one.", MessageType.Warning);
                return;
            }

            GUILayout.Space(10);

            // Output path
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Output Path:", GUILayout.Width(100));
            _outputPath = EditorGUILayout.TextField(_outputPath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("Select Output Folder", "Assets", "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    _outputPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Text area for names input
            EditorGUILayout.LabelField("Enter Names (one per line):");
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(150));
            _nameInput = EditorGUILayout.TextArea(_nameInput, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            GUILayout.Space(10);

            // Buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create Name Schemas"))
            {
                CreateNameSchemas();
            }

            if (GUILayout.Button("Clear Input"))
            {
                _nameInput = "";
                GUI.FocusControl(null);
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);

            // Show existing names
            _showExistingNames =
                EditorGUILayout.Foldout(_showExistingNames, $"Existing Names ({_nameSettings.schemas.Length})");
            if (_showExistingNames)
            {
                EditorGUI.indentLevel++;
                if (_nameSettings.schemas.Length == 0)
                {
                    EditorGUILayout.LabelField("No existing name schemas found.");
                }
                else
                {
                    EditorGUILayout.BeginVertical("box");
                    foreach (var schema in _nameSettings.schemas.OrderBy(s => s.fixed32))
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"{schema.ID}: {schema.fixed32}", GUILayout.ExpandWidth(true));
                        if (GUILayout.Button("Select", GUILayout.Width(60)))
                        {
                            Selection.activeObject = schema;
                            EditorGUIUtility.PingObject(schema);
                        }

                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.EndVertical();
                }

                EditorGUI.indentLevel--;
            }
        }

        private void CreateNameSchemas()
        {
            if (string.IsNullOrWhiteSpace(_nameInput))
            {
                EditorUtility.DisplayDialog("Error", "Please enter at least one name.", "OK");
                return;
            }

            // Ensure output directory exists
            if (!AssetDatabase.IsValidFolder(_outputPath))
            {
                string[] pathParts = _outputPath.Split('/');
                string currentPath = pathParts[0]; // "Assets"

                for (int i = 1; i < pathParts.Length; i++)
                {
                    string newPath = currentPath + "/" + pathParts[i];
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, pathParts[i]);
                    }

                    currentPath = newPath;
                }
            }

            // Parse names from input
            string[] inputNames = _nameInput.Split('\n');
            List<string> namesToCreate = new List<string>();
            List<string> existingNames = new List<string>();
            HashSet<string> existingNameSet = new HashSet<string>();

            // Build set of existing names for quick lookup
            foreach (var schema in _nameSettings.schemas)
            {
                if (!string.IsNullOrEmpty(schema.fixed32))
                {
                    existingNameSet.Add(schema.fixed32.Trim());
                }
            }

            // Process input names
            foreach (string name in inputNames)
            {
                string trimmedName = name.Trim();
                if (string.IsNullOrEmpty(trimmedName)) continue;

                // Check length for FixedString32Bytes (max 32 bytes)
                if (System.Text.Encoding.UTF8.GetByteCount(trimmedName) > 32)
                {
                    Debug.LogWarning($"Name '{trimmedName}' exceeds 32 bytes limit and will be skipped.");
                    continue;
                }

                if (existingNameSet.Contains(trimmedName))
                {
                    existingNames.Add(trimmedName);
                }
                else
                {
                    namesToCreate.Add(trimmedName);
                    existingNameSet.Add(trimmedName); // Prevent duplicates within this batch
                }
            }

            if (namesToCreate.Count == 0)
            {
                string message = existingNames.Count > 0
                    ? $"All names already exist:\n{string.Join("\n", existingNames)}"
                    : "No valid names to create.";
                EditorUtility.DisplayDialog("Info", message, "OK");
                return;
            }

            // Get next available ID
            int nextId = GetNextAvailableId();

            // Create new schemas
            List<NameSchema> newSchemas = new List<NameSchema>();
            int createdCount = 0;

            foreach (string name in namesToCreate)
            {
                NameSchema newSchema = CreateInstance<NameSchema>();
                newSchema.fixed32 = name;
                newSchema.ID = nextId++;

                string fileName = SanitizeFileName(name);
                string assetPath = $"{_outputPath}/{newSchema.ID}_{fileName}_NameSchema.asset";

                // Handle duplicate file names
                int counter = 1;
                while (AssetDatabase.LoadAssetAtPath<NameSchema>(assetPath) != null)
                {
                    assetPath = $"{_outputPath}/{fileName}_{counter}.asset";
                    counter++;
                }

                AssetDatabase.CreateAsset(newSchema, assetPath);
                newSchemas.Add(newSchema);
                createdCount++;
            }

            // Update NameSettings
            var schemasList = _nameSettings.schemas.ToList();
            schemasList.AddRange(newSchemas);
            _nameSettings.schemas = schemasList.ToArray();

            // Save assets
            EditorUtility.SetDirty(_nameSettings);
            AssetDatabase.SaveAssets();
            EditorPrefs.SetString(CreatePath, _outputPath);
            AssetDatabase.Refresh();

            // Show results
            string resultMessage = $"Successfully created {createdCount} name schema(s).";
            if (existingNames.Count > 0)
            {
                resultMessage +=
                    $"\n\nSkipped {existingNames.Count} existing name(s):\n{string.Join(", ", existingNames)}";
            }

            EditorUtility.DisplayDialog("Success", resultMessage, "OK");

            // Clear input
            _nameInput = "";
            GUI.FocusControl(null);
        }

        private int GetNextAvailableId()
        {
            int maxId = _nameSettings.schemas.Length > 0 ? _nameSettings.schemas.Max(s => s.ID) : 0;
            return maxId + 1;
        }

        private string SanitizeFileName(string fileName)
        {
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char c in invalid)
            {
                fileName = fileName.Replace(c.ToString(), "");
            }

            return fileName.Replace(" ", "_");
        }
    }
}