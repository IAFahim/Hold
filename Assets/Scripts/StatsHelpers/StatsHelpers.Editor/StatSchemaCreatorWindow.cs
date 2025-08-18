#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BovineLabs.Essence.Authoring;

public class EssenceSchemaForge : EditorWindow
{
    #region Configuration & State
    private const string StatSchemaPath = "Assets/Settings/Schemas/Stats";
    private const string IntrinsicSchemaPath = "Assets/Settings/Schemas/Intrinsics";

    [System.Serializable]
    private class SchemaProposal
    {
        public bool IsEnabled = true;
        public SchemaType Type;
        public int Key;
        public string OriginalVariableName;
        public string DisplayName;
        public float Factor = 1.0f;
        public ProposalStatus Status;
        public string StatusMessage;
        public string Group;
        public StatSchemaObject MaxStatLink; // The killer feature
    }

    private enum SchemaType { Stat, Intrinsic }
    private enum ProposalStatus { New, Exists, KeyConflict }

    [SerializeField] private List<SchemaProposal> proposals = new List<SchemaProposal>();
    [SerializeField] private Vector2 scrollPosition;
    [SerializeField] private string inputCode = "Drag a C# script here, assign it below, or paste code...";
    [SerializeField] private MonoScript scriptObject;
    private Dictionary<string, bool> groupFoldouts = new Dictionary<string, bool>();

    private Dictionary<int, StatSchemaObject> existingStats = new Dictionary<int, StatSchemaObject>();
    private Dictionary<int, IntrinsicSchemaObject> existingIntrinsics = new Dictionary<int, IntrinsicSchemaObject>();
    private HashSet<string> existingStatAssetNames = new HashSet<string>();
    private HashSet<string> existingIntrinsicAssetNames = new HashSet<string>();
    #endregion

    #region Window Management
    [MenuItem("Tools/BovineLabs Essence/Essence Schema Forge")]
    public static void ShowWindow()
    {
        GetWindow<EssenceSchemaForge>("Essence Schema Forge");
    }

    private void OnEnable()
    {
        // Load session state for foldouts
        var data = SessionState.GetString(nameof(groupFoldouts), "");
        if (!string.IsNullOrEmpty(data))
            groupFoldouts = data.Split(';').Select(p => p.Split(':')).ToDictionary(p => p[0], p => bool.Parse(p[1]));

        RefreshExistingSchemaCache();
    }

    private void OnDisable()
    {
        // Save session state for foldouts
        var data = string.Join(";", groupFoldouts.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
        SessionState.SetString(nameof(groupFoldouts), data);
    }
    #endregion

    #region GUI Rendering
    private void OnGUI()
    {
        DrawHeader();
        DrawInputArea();
        HandleDragAndDrop();

        EditorGUILayout.Separator();

        if (proposals.Count > 0)
        {
            DrawPreviewArea();
            DrawActionButtons();
        }
    }

    private void DrawHeader()
    {
        EditorGUILayout.LabelField("Essence Schema Forge", new GUIStyle(EditorStyles.largeLabel) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter });
        EditorGUILayout.HelpBox("Intelligently parse C# components to batch-create Stat and Intrinsic schemas.", MessageType.Info);
    }

    private void DrawInputArea()
    {
        EditorGUILayout.LabelField("Source Component", EditorStyles.boldLabel);
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUI.BeginChangeCheck();
            scriptObject = (MonoScript)EditorGUILayout.ObjectField("Script Asset", scriptObject, typeof(MonoScript), false);
            if (EditorGUI.EndChangeCheck() && scriptObject != null)
            {
                inputCode = scriptObject.text;
                ParseInputCode();
            }

            inputCode = EditorGUILayout.TextArea(inputCode, GUILayout.Height(100));
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Parse Code", GUILayout.Height(30)))
            {
                ParseInputCode();
                GUI.FocusControl(null);
            }
            if (GUILayout.Button("Clear All", GUILayout.Height(30), GUILayout.Width(80)))
            {
                ClearAll();
                GUI.FocusControl(null);
            }
        }
    }

    private void DrawPreviewArea()
    {
        EditorGUILayout.LabelField("Schema Creation Preview", EditorStyles.boldLabel);
        DrawPreviewToolbar();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, "box");
        var groupedProposals = proposals.GroupBy(p => p.Group);
        foreach (var group in groupedProposals)
        {
            if (!groupFoldouts.ContainsKey(group.Key)) groupFoldouts[group.Key] = true;
            groupFoldouts[group.Key] = EditorGUILayout.Foldout(groupFoldouts[group.Key], group.Key, true, EditorStyles.foldoutHeader);

            if (groupFoldouts[group.Key])
            {
                foreach (var proposal in group)
                {
                    DrawProposalRow(proposal);
                }
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawPreviewToolbar()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            if (GUILayout.Button("Enable All", EditorStyles.toolbarButton)) proposals.ForEach(p => p.IsEnabled = true);
            if (GUILayout.Button("Disable All", EditorStyles.toolbarButton)) proposals.ForEach(p => p.IsEnabled = false);
            if (GUILayout.Button("Select All New", EditorStyles.toolbarButton))
            {
                proposals.ForEach(p => p.IsEnabled = p.Status == ProposalStatus.New);
            }
            GUILayout.FlexibleSpace();
        }
    }

    private void DrawProposalRow(SchemaProposal proposal)
    {
        bool isExisting = proposal.Status == ProposalStatus.Exists;
        Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = GetColorForStatus(proposal.Status);

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            GUI.backgroundColor = originalColor;
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(isExisting))
                {
                    proposal.IsEnabled = EditorGUILayout.Toggle(proposal.IsEnabled, GUILayout.Width(20));
                    proposal.Type = (SchemaType)EditorGUILayout.EnumPopup(proposal.Type, GUILayout.Width(80));
                    proposal.Key = EditorGUILayout.IntField(proposal.Key, GUILayout.Width(40));
                    proposal.DisplayName = EditorGUILayout.TextField(proposal.DisplayName, GUILayout.ExpandWidth(true));
                }
                
                var (icon, tooltip) = GetIconForStatus(proposal.Status, proposal.StatusMessage);
                GUILayout.Label(new GUIContent(icon, tooltip), GUILayout.Width(25));
            }

            if (proposal.Type == SchemaType.Intrinsic)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(150); // Indent
                    using (new EditorGUI.DisabledScope(isExisting))
                    {
                        proposal.Factor = EditorGUILayout.FloatField("Factor", proposal.Factor, GUILayout.Width(120));
                        GUILayout.Space(10);
                        proposal.MaxStatLink = (StatSchemaObject)EditorGUILayout.ObjectField("Max Stat Link", proposal.MaxStatLink, typeof(StatSchemaObject), false);
                    }
                }
            }
        }
        
        // Re-validate if user makes a change
        if (GUI.changed)
        {
            ValidateProposals();
        }
    }

    private void DrawActionButtons()
    {
        bool hasConflicts = proposals.Any(p => p.IsEnabled && p.Status == ProposalStatus.KeyConflict);
        using (new EditorGUI.DisabledScope(hasConflicts))
        {
            if (GUILayout.Button("Create Selected Schemas", GUILayout.Height(40)))
            {
                CreateSelectedSchemas();
            }
        }
        if (hasConflicts)
        {
            EditorGUILayout.HelpBox("Cannot create schemas. Please resolve all Key Conflicts (⚠️) first.", MessageType.Error);
        }
    }
    #endregion

    #region Core Logic
    private void ClearAll()
    {
        scriptObject = null;
        inputCode = "";
        proposals.Clear();
        groupFoldouts.Clear();
    }

    private void ParseInputCode()
    {
        proposals.Clear();
        groupFoldouts.Clear();
        
        string currentGroup = "General";
        var lines = inputCode.Split('\n');

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (trimmedLine.StartsWith("[Header(\""))
            {
                currentGroup = trimmedLine.Split('\"')[1];
                if (!groupFoldouts.ContainsKey(currentGroup)) groupFoldouts[currentGroup] = true;
                continue;
            }

            if (trimmedLine.StartsWith("[HideInInspector]")) continue;

            var match = Regex.Match(trimmedLine, @"public\s+(float|byte|int|short)\s+([a-zA-Z0-9_]+);");
            if (match.Success)
            {
                var proposal = new SchemaProposal
                {
                    OriginalVariableName = match.Groups[2].Value,
                    DisplayName = FormatDisplayName(match.Groups[2].Value),
                    Group = currentGroup
                };

                if (proposal.OriginalVariableName.StartsWith("Current") || proposal.OriginalVariableName.Contains("Counter"))
                    proposal.Type = SchemaType.Intrinsic;
                else
                    proposal.Type = SchemaType.Stat;
                
                proposals.Add(proposal);
            }
        }
        
        // Auto-link MaxStat for intrinsics
        foreach (var intrinsicProposal in proposals.Where(p => p.Type == SchemaType.Intrinsic))
        {
            string potentialStatName = intrinsicProposal.OriginalVariableName.Replace("Current", "Max");
            var matchingStatProposal = proposals.FirstOrDefault(p => p.Type == SchemaType.Stat && p.OriginalVariableName == potentialStatName);
            if (matchingStatProposal != null)
            {
                // This is a placeholder link for the UI. The actual asset link happens on creation.
                // We can't link to an asset that doesn't exist yet. This logic is handled in CreateSelectedSchemas.
            }
        }

        ValidateProposals();
    }

    private void ValidateProposals()
    {
        int nextStatKey = existingStats.Count > 0 ? existingStats.Keys.Max() + 1 : 1;
        int nextIntrinsicKey = existingIntrinsics.Count > 0 ? existingIntrinsics.Keys.Max() + 1 : 1;

        var assignedStatKeys = new HashSet<int>(existingStats.Keys);
        var assignedIntrinsicKeys = new HashSet<int>(existingIntrinsics.Keys);

        foreach (var proposal in proposals)
        {
            if (proposal.Key == 0)
            {
                if (proposal.Type == SchemaType.Stat)
                {
                    while (assignedStatKeys.Contains(nextStatKey)) nextStatKey++;
                    proposal.Key = nextStatKey;
                }
                else
                {
                    while (assignedIntrinsicKeys.Contains(nextIntrinsicKey)) nextIntrinsicKey++;
                    proposal.Key = nextIntrinsicKey;
                }
            }
            // Add to assigned keys to prevent re-use in this same batch
            if (proposal.Type == SchemaType.Stat) assignedStatKeys.Add(proposal.Key);
            else assignedIntrinsicKeys.Add(proposal.Key);

            // Check status
            if (proposal.Type == SchemaType.Stat)
            {
                string filename = $"{proposal.Key:D3} {proposal.DisplayName}";
                if (existingStatAssetNames.Contains(filename))
                {
                    proposal.Status = ProposalStatus.Exists;
                    proposal.StatusMessage = $"Asset '{filename}' already exists.";
                }
                else if (existingStats.ContainsKey(proposal.Key))
                {
                    proposal.Status = ProposalStatus.KeyConflict;
                    proposal.StatusMessage = $"Key {proposal.Key} is used by '{existingStats[proposal.Key].name}'.";
                }
                else
                {
                    proposal.Status = ProposalStatus.New;
                    proposal.StatusMessage = "Ready to create.";
                }
            }
            else // Intrinsic
            {
                string filename = $"{proposal.Key} {proposal.DisplayName} {proposal.Factor}";
                if (existingIntrinsicAssetNames.Contains(filename))
                {
                    proposal.Status = ProposalStatus.Exists;
                    proposal.StatusMessage = $"Asset '{filename}' already exists.";
                }
                else if (existingIntrinsics.ContainsKey(proposal.Key))
                {
                    proposal.Status = ProposalStatus.KeyConflict;
                    proposal.StatusMessage = $"Key {proposal.Key} is used by '{existingIntrinsics[proposal.Key].name}'.";
                }
                else
                {
                    proposal.Status = ProposalStatus.New;
                    proposal.StatusMessage = "Ready to create.";
                }
            }
        }
    }

    private void CreateSelectedSchemas()
    {
        var validProposals = proposals.Where(p => p.IsEnabled && p.Status == ProposalStatus.New).ToList();
        if (validProposals.Count == 0)
        {
            EditorUtility.DisplayDialog("No Schemas Created", "There were no new schemas selected for creation.", "OK");
            return;
        }

        AssetDatabase.StartAssetEditing();
        int statCount = 0;
        int intrinsicCount = 0;
        var createdStatAssets = new Dictionary<string, StatSchemaObject>();

        try
        {
            // First pass: Create all STATS
            foreach (var proposal in validProposals.Where(p => p.Type == SchemaType.Stat))
            {
                string assetName = $"{proposal.Key:D3} {proposal.DisplayName}";
                var asset = CreateAsset<StatSchemaObject>(assetName, StatSchemaPath);
                createdStatAssets.Add(proposal.OriginalVariableName, asset);
                statCount++;
            }

            // Second pass: Create all INTRINSICS and link them
            foreach (var proposal in validProposals.Where(p => p.Type == SchemaType.Intrinsic))
            {
                string assetName = $"{proposal.Key} {proposal.DisplayName} {proposal.Factor}";
                var asset = CreateAsset<IntrinsicSchemaObject>(assetName, IntrinsicSchemaPath);

                // Find the linked stat to connect
                StatSchemaObject statToLink = proposal.MaxStatLink; // From manual assignment
                if (statToLink == null) // Try to find from auto-created stats
                {
                    string potentialStatName = proposal.OriginalVariableName.Replace("Current", "Max");
                    createdStatAssets.TryGetValue(potentialStatName, out statToLink);
                }
                
                if (statToLink != null)
                {
                    // This is the magic: modify the IntrinsicSchemaObject before saving
                    var serializedObject = new SerializedObject(asset);
                    serializedObject.FindProperty("maxStat").objectReferenceValue = statToLink;
                    serializedObject.ApplyModifiedProperties();
                }
                intrinsicCount++;
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        EditorUtility.DisplayDialog("Creation Complete", $"Successfully created:\n- {statCount} Stat Schemas\n- {intrinsicCount} Intrinsic Schemas", "OK");
        RefreshExistingSchemaCache();
        ParseInputCode(); // Re-parse to update status of created items
    }
    #endregion

    #region Utility Methods
    private void HandleDragAndDrop()
    {
        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drag & Drop C# Script Here");

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition)) break;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (var path in DragAndDrop.paths)
                    {
                        if (path.EndsWith(".cs"))
                        {
                            scriptObject = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                            inputCode = scriptObject.text;
                            ParseInputCode();
                            break; // Handle first valid script
                        }
                    }
                }
                evt.Use();
                break;
        }
    }

    private void RefreshExistingSchemaCache()
    {
        existingStats.Clear();
        existingIntrinsics.Clear();
        existingStatAssetNames.Clear();
        existingIntrinsicAssetNames.Clear();

        FindSchemas<StatSchemaObject>(existingStats, existingStatAssetNames);
        FindSchemas<IntrinsicSchemaObject>(existingIntrinsics, existingIntrinsicAssetNames);
    }

    private void FindSchemas<T>(Dictionary<int, T> keyDict, HashSet<string> nameSet) where T : ScriptableObject
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                string assetName = Path.GetFileNameWithoutExtension(path);
                nameSet.Add(assetName);
                
                string[] parts = assetName.Split(' ');
                if (parts.Length > 0 && int.TryParse(parts[0], out int key))
                {
                    if (!keyDict.ContainsKey(key)) keyDict.Add(key, asset);
                }
            }
        }
    }

    private T CreateAsset<T>(string assetName, string path) where T : ScriptableObject
    {
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        string fullPath = Path.Combine(path, assetName + ".asset");
        T instance = CreateInstance<T>();
        AssetDatabase.CreateAsset(instance, fullPath);
        return instance;
    }

    private static string FormatDisplayName(string variableName) => Regex.Replace(variableName, "(\\B[A-Z])", " $1").Trim();

    private (Texture, string) GetIconForStatus(ProposalStatus status, string message)
    {
        switch (status)
        {
            case ProposalStatus.New: return (EditorGUIUtility.IconContent("d_winbtn_mac_max").image, message);
            case ProposalStatus.Exists: return (EditorGUIUtility.IconContent("d_winbtn_mac_min").image, message);
            case ProposalStatus.KeyConflict: return (EditorGUIUtility.IconContent("Warning").image, message);
            default: return (null, message);
        }
    }

    private Color GetColorForStatus(ProposalStatus status)
    {
        switch (status)
        {
            case ProposalStatus.New: return new Color(0.7f, 1.0f, 0.7f, 0.2f);
            case ProposalStatus.Exists: return new Color(0.8f, 0.8f, 0.8f, 0.2f);
            case ProposalStatus.KeyConflict: return new Color(1.0f, 0.9f, 0.6f, 0.2f);
            default: return Color.white;
        }
    }
    #endregion
}
#endif