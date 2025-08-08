using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Missions.Missions.Authoring.Schemas;
using Missions.Missions.Authoring.Settings;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Missions.Missions.Authoring.Editor
{
    public class NameSchemaCreatorWindow : EditorWindow
    {
        private string _nameInput = "";
        private NameSettings _nameSettings;
        private bool _showExistingNames = true;
        private string _outputPath = "Assets/ScriptableObjects/NameSchemas";

        // UI Toolkit elements
        private ObjectField _settingsField;
        private TextField _pathField;
        private TextField _namesField;
        private Foldout _existingFoldout;
        private ListView _existingListView;
        private Label _statsLabel;

        [MenuItem("Tools/Schema/Name Schema Creator")]
        public static void ShowWindow()
        {
            GetWindow<NameSchemaCreatorWindow>("Name Schema Creator");
        }

        private void OnEnable()
        {
            LoadNameSettings();
            _outputPath = EditorPrefs.GetString(CreatePath, _outputPath);
        }

        private static string CreatePath => $"{nameof(NameSchema)}_Editor_Create_Path";

        private void LoadNameSettings()
        {
            string[] guids = AssetDatabase.FindAssets("t:NameSettings");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _nameSettings = AssetDatabase.LoadAssetAtPath<NameSettings>(path);
            }

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

            if (_settingsField != null)
            {
                _settingsField.value = _nameSettings;
                RebuildExistingList();
                UpdateStats();
            }
        }

        public void CreateGUI()
        {
            // Load UXML/USS
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Missions/Missions.Authoring/Editor/UI/NameSchemaCreatorWindow.uxml");
            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Missions/Missions.Authoring/Editor/UI/MissionsEditor.uss");

            rootVisualElement.Clear();
            if (uss != null) rootVisualElement.styleSheets.Add(uss);
            if (uxml != null) uxml.CloneTree(rootVisualElement);

            // Query controls
            var refreshBtn = rootVisualElement.Q<ToolbarButton>("refreshButton");
            _statsLabel = rootVisualElement.Q<Label>("statsLabel");
            _settingsField = rootVisualElement.Q<ObjectField>("settingsField");
            _pathField = rootVisualElement.Q<TextField>("pathField");
            var browseBtn = rootVisualElement.Q<Button>("browseButton");
            _namesField = rootVisualElement.Q<TextField>("namesField");
            var createBtn = rootVisualElement.Q<Button>("createButton");
            var clearBtn = rootVisualElement.Q<Button>("clearButton");
            _existingFoldout = rootVisualElement.Q<Foldout>("existingFoldout");
            _existingListView = rootVisualElement.Q<ListView>("existingList");

            // Wire
            if (refreshBtn != null) refreshBtn.clicked += LoadNameSettings;
            if (_settingsField != null)
            {
                _settingsField.objectType = typeof(NameSettings);
                _settingsField.allowSceneObjects = false;
                _settingsField.value = _nameSettings;
                _settingsField.RegisterValueChangedCallback(evt =>
                {
                    _nameSettings = evt.newValue as NameSettings;
                    RebuildExistingList();
                    UpdateStats();
                });
            }
            if (_pathField != null)
            {
                _pathField.value = _outputPath;
                _pathField.RegisterValueChangedCallback(evt => { _outputPath = evt.newValue; });
            }
            if (browseBtn != null)
            {
                browseBtn.clicked += () =>
                {
                    string selectedPath = EditorUtility.OpenFolderPanel("Select Output Folder", Application.dataPath, "");
                    if (!string.IsNullOrEmpty(selectedPath) && selectedPath.StartsWith(Application.dataPath))
                    {
                        _outputPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                        if (_pathField != null) _pathField.value = _outputPath;
                        EditorPrefs.SetString(CreatePath, _outputPath);
                    }
                };
            }
            if (_namesField != null)
            {
                _namesField.multiline = true;
                _namesField.value = _nameInput;
                _namesField.RegisterValueChangedCallback(evt => { _nameInput = evt.newValue; });
            }
            if (createBtn != null)
            {
                createBtn.clicked += () => { CreateNameSchemas(); RebuildExistingList(); UpdateStats(); };
            }
            if (clearBtn != null)
            {
                clearBtn.clicked += () => { _nameInput = ""; if (_namesField != null) _namesField.value = ""; };
            }
            if (_existingFoldout != null)
            {
                _existingFoldout.value = _showExistingNames;
                _existingFoldout.RegisterValueChangedCallback(evt => _showExistingNames = evt.newValue);
            }
            if (_existingListView != null)
            {
                _existingListView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
                _existingListView.selectionType = SelectionType.None;
                _existingListView.makeItem = () =>
                {
                    var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
                    var label = new Label { style = { flexGrow = 1 } };
                    var selectBtn = new Button { text = "Select" };
                    selectBtn.style.width = 60;
                    row.Add(label);
                    row.Add(selectBtn);
                    return row;
                };
                _existingListView.bindItem = (el, i) =>
                {
                    if (_nameSettings == null || _nameSettings.schemas == null) return;
                    var ordered = _nameSettings.schemas.OrderBy(s => s.fixed32).ToList();
                    if (i < 0 || i >= ordered.Count) return;
                    var data = ordered[i];
                    el.Q<Label>().text = $"{data.ID}: {data.fixed32}";
                    var btn = el.Q<Button>();
                    btn.clicked -= null;
                    btn.clicked += () => { Selection.activeObject = data; EditorGUIUtility.PingObject(data); };
                };
            }

            RebuildExistingList();
            UpdateStats();
        }

        private string BuildExistingTitle()
        {
            int count = _nameSettings != null && _nameSettings.schemas != null ? _nameSettings.schemas.Length : 0;
            return $"Existing Names ({count})";
        }

        private void RebuildExistingList()
        {
            if (_existingListView == null) return;
            if (_nameSettings == null || _nameSettings.schemas == null)
            {
                _existingListView.itemsSource = new List<NameSchema>();
            }
            else
            {
                var src = _nameSettings.schemas.OrderBy(s => s.fixed32).ToList();
                _existingListView.itemsSource = src;
                if (_existingFoldout != null) _existingFoldout.text = BuildExistingTitle();
            }
            _existingListView.Rebuild();
        }

        private void UpdateStats()
        {
            if (_statsLabel == null) return;
            int count = _nameSettings != null && _nameSettings.schemas != null ? _nameSettings.schemas.Length : 0;
            _statsLabel.text = $"Total Schemas: {count}";
        }

        private void OnGUI()
        {
            // Fallback for older Unity versions without CreateGUI support
            if (rootVisualElement != null && rootVisualElement.childCount > 0) return;

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
            _nameInput = EditorGUILayout.TextArea(_nameInput, GUILayout.ExpandHeight(true));

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
            _showExistingNames = EditorGUILayout.Foldout(_showExistingNames, BuildExistingTitle());
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
                resultMessage += $"\n\nSkipped {existingNames.Count} existing name(s):\n{string.Join(", ", existingNames)}";
            }

            EditorUtility.DisplayDialog("Success", resultMessage, "OK");

            // Clear input
            _nameInput = "";
            if (_namesField != null) _namesField.value = "";
        }

        private int GetNextAvailableId()
        {
            if (_nameSettings.schemas.Length == 0) return 1;

            HashSet<int> usedIds = new HashSet<int>();
            foreach (var schema in _nameSettings.schemas)
            {
                usedIds.Add(schema.ID);
            }

            int nextId = 1;
            while (usedIds.Contains(nextId))
            {
                nextId++;
            }

            return nextId;
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