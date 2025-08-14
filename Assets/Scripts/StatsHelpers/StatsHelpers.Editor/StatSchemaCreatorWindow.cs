#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BovineLabs.Essence.Authoring; // Ensure this namespace is correct

public class AdvancedStatSchemaParser : EditorWindow
{
    // --- Configuration ---
    private const string StatSchemaPath = "Assets/Settings/Schemas/Stats";
    private const string IntrinsicSchemaPath = "Assets/Settings/Schemas/Intrinsics";

    // --- UI State ---
    private string inputCode = "Paste your C# component code here...";
    private Vector2 scrollPosition;
    private string feedbackMessage;
    private MessageType feedbackType = MessageType.Info;

    // --- Data Models ---
    private enum SchemaType { Stat, Intrinsic }
    private enum ProposalStatus { New, Exists, KeyConflict }

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
    }

    private List<SchemaProposal> proposals = new List<SchemaProposal>();
    private Dictionary<int, string> existingStatKeys = new Dictionary<int, string>();
    private Dictionary<int, string> existingIntrinsicKeys = new Dictionary<int, string>();
    private HashSet<string> existingStatNames = new HashSet<string>();
    private HashSet<string> existingIntrinsicNames = new HashSet<string>();

    [MenuItem("Tools/BovineLabs Essence/Advanced Schema Parser")]
    public static void ShowWindow()
    {
        GetWindow<AdvancedStatSchemaParser>("Advanced Schema Parser");
    }

    private void OnEnable()
    {
        RefreshExistingSchemaCache();
    }

    private void OnGUI()
    {
        DrawHeader();
        DrawInputArea();

        if (proposals.Count > 0)
        {
            DrawPreviewArea();
            DrawActionButtons();
        }
    }

    #region Drawing Methods

    private void DrawHeader()
    {
        EditorGUILayout.LabelField("Advanced Stat & Intrinsic Parser", new GUIStyle(EditorStyles.largeLabel) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter });
        EditorGUILayout.HelpBox("1. Paste a C# component struct/class below.\n2. Click 'Parse Code' to generate a list of potential schemas.\n3. Review, edit, and disable items in the preview list.\n4. Click 'Create Selected' to generate the asset files.", MessageType.Info);
        
        if (!string.IsNullOrEmpty(feedbackMessage))
        {
            EditorGUILayout.HelpBox(feedbackMessage, feedbackType);
        }
    }

    private void DrawInputArea()
    {
        EditorGUILayout.LabelField("C# Code Input", EditorStyles.boldLabel);
        inputCode = EditorGUILayout.TextArea(inputCode, GUILayout.Height(150));

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Parse Code", GUILayout.Height(30)))
            {
                ParseInputCode();
                GUI.FocusControl(null);
            }
            if (GUILayout.Button("Clear", GUILayout.Height(30), GUILayout.Width(60)))
            {
                ClearAll();
                GUI.FocusControl(null);
            }
        }
    }

    private void DrawPreviewArea()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Schema Creation Preview", EditorStyles.boldLabel);

        // Header Row
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            GUILayout.Label("Create", GUILayout.Width(50));
            GUILayout.Label("Type", GUILayout.Width(80));
            GUILayout.Label("Key", GUILayout.Width(40));
            GUILayout.Label("Display Name", GUILayout.ExpandWidth(true));
            GUILayout.Label("Factor", GUILayout.Width(50));
            GUILayout.Label("Status", GUILayout.Width(100));
        }

        // Content Rows
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, EditorStyles.helpBox);
        foreach (var proposal in proposals)
        {
            DrawProposalRow(proposal);
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawProposalRow(SchemaProposal proposal)
    {
        bool isExisting = proposal.Status == ProposalStatus.Exists;
        Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = GetColorForStatus(proposal.Status);

        using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
        {
            GUI.backgroundColor = originalColor;

            EditorGUI.BeginChangeCheck();

            using (new EditorGUI.DisabledScope(isExisting))
            {
                proposal.IsEnabled = EditorGUILayout.Toggle(proposal.IsEnabled, GUILayout.Width(50));
                proposal.Type = (SchemaType)EditorGUILayout.EnumPopup(proposal.Type, GUILayout.Width(80));
                proposal.Key = EditorGUILayout.IntField(proposal.Key, GUILayout.Width(40));
                proposal.DisplayName = EditorGUILayout.TextField(proposal.DisplayName, GUILayout.ExpandWidth(true));

                using (new EditorGUI.DisabledScope(proposal.Type == SchemaType.Stat))
                {
                    proposal.Factor = EditorGUILayout.FloatField(proposal.Factor, GUILayout.Width(50));
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                ValidateProposals(); // Re-validate if user makes a change
            }

            EditorGUILayout.LabelField(new GUIContent(proposal.Status.ToString(), proposal.StatusMessage), GUILayout.Width(100));
        }
    }

    private void DrawActionButtons()
    {
        EditorGUILayout.Space(5);
        if (GUILayout.Button("Create Selected Schemas", GUILayout.Height(40)))
        {
            CreateSelectedSchemas();
        }
    }

    #endregion

    #region Core Logic

    private void ClearAll()
    {
        inputCode = "";
        proposals.Clear();
        feedbackMessage = "";
    }

    private void ParseInputCode()
    {
        proposals.Clear();
        // Regex to find public fields of numeric types (float, byte, int, short)
        var regex = new Regex(@"public\s+(float|byte|int|short)\s+([a-zA-Z0-9_]+);");
        var matches = regex.Matches(inputCode);

        if (matches.Count == 0)
        {
            feedbackMessage = "No valid public fields (float, byte, int, short) found in the provided code.";
            feedbackType = MessageType.Warning;
            return;
        }

        foreach (Match match in matches)
        {
            var proposal = new SchemaProposal
            {
                OriginalVariableName = match.Groups[2].Value,
                DisplayName = FormatDisplayName(match.Groups[2].Value)
            };

            // Heuristic: "Current" often implies an Intrinsic that tracks a value.
            // Also, `MaxUngroundedJumps` is a Stat, but `CurrentUngroundedJumps` is an Intrinsic.
            if (proposal.OriginalVariableName.StartsWith("Current") || proposal.OriginalVariableName.Contains("Counter"))
            {
                proposal.Type = SchemaType.Intrinsic;
            }
            else
            {
                proposal.Type = SchemaType.Stat;
            }
            
            proposals.Add(proposal);
        }

        ValidateProposals();
        feedbackMessage = $"Successfully parsed {proposals.Count} potential schemas. Please review the list below.";
        feedbackType = MessageType.Info;
    }

    private void ValidateProposals()
    {
        int nextStatKey = existingStatKeys.Count > 0 ? existingStatKeys.Keys.Max() + 1 : 1;
        int nextIntrinsicKey = existingIntrinsicKeys.Count > 0 ? existingIntrinsicKeys.Keys.Max() + 1 : 1;

        var assignedStatKeys = new HashSet<int>(existingStatKeys.Keys);
        var assignedIntrinsicKeys = new HashSet<int>(existingIntrinsicKeys.Keys);

        foreach (var proposal in proposals)
        {
            // Assign a key if it's 0 (unassigned)
            if (proposal.Key == 0)
            {
                if (proposal.Type == SchemaType.Stat)
                {
                    while (assignedStatKeys.Contains(nextStatKey)) nextStatKey++;
                    proposal.Key = nextStatKey;
                    assignedStatKeys.Add(nextStatKey);
                }
                else
                {
                    while (assignedIntrinsicKeys.Contains(nextIntrinsicKey)) nextIntrinsicKey++;
                    proposal.Key = nextIntrinsicKey;
                    assignedIntrinsicKeys.Add(nextIntrinsicKey);
                }
            }

            // Check status
            if (proposal.Type == SchemaType.Stat)
            {
                string filename = $"{proposal.Key:D3} {proposal.DisplayName}";
                if (existingStatNames.Contains(filename))
                {
                    proposal.Status = ProposalStatus.Exists;
                    proposal.StatusMessage = $"Asset already exists at {StatSchemaPath}";
                    proposal.IsEnabled = false;
                }
                else if (existingStatKeys.ContainsKey(proposal.Key))
                {
                    proposal.Status = ProposalStatus.KeyConflict;
                    proposal.StatusMessage = $"Key {proposal.Key} is already used by '{existingStatKeys[proposal.Key]}'";
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
                if (existingIntrinsicNames.Contains(filename))
                {
                    proposal.Status = ProposalStatus.Exists;
                    proposal.StatusMessage = $"Asset already exists at {IntrinsicSchemaPath}";
                    proposal.IsEnabled = false;
                }
                else if (existingIntrinsicKeys.ContainsKey(proposal.Key))
                {
                    proposal.Status = ProposalStatus.KeyConflict;
                    proposal.StatusMessage = $"Key {proposal.Key} is already used by '{existingIntrinsicKeys[proposal.Key]}'";
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
        int createdCount = 0;
        foreach (var proposal in proposals.Where(p => p.IsEnabled && p.Status == ProposalStatus.New))
        {
            if (proposal.Type == SchemaType.Stat)
            {
                string assetName = $"{proposal.Key:D3} {proposal.DisplayName}";
                CreateAsset<StatSchemaObject>(assetName, StatSchemaPath);
            }
            else
            {
                string assetName = $"{proposal.Key} {proposal.DisplayName} {proposal.Factor}";
                CreateAsset<IntrinsicSchemaObject>(assetName, IntrinsicSchemaPath);
            }
            createdCount++;
        }

        if (createdCount > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            feedbackMessage = $"Successfully created {createdCount} schema assets.";
            feedbackType = MessageType.Info;
            RefreshExistingSchemaCache();
            ValidateProposals(); // Re-validate to update status to "Exists"
        }
        else
        {
            feedbackMessage = "No new schemas were selected for creation.";
            feedbackType = MessageType.Warning;
        }
    }

    private void CreateAsset<T>(string assetName, string path) where T : ScriptableObject
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        string fullPath = Path.Combine(path, assetName + ".asset");
        T instance = CreateInstance<T>();
        AssetDatabase.CreateAsset(instance, fullPath);
    }

    #endregion

    #region Utility Methods

    private void RefreshExistingSchemaCache()
    {
        existingStatKeys.Clear();
        existingIntrinsicKeys.Clear();
        existingStatNames.Clear();
        existingIntrinsicNames.Clear();

        FindSchemas<StatSchemaObject>(existingStatKeys, existingStatNames);
        FindSchemas<IntrinsicSchemaObject>(existingIntrinsicKeys, existingIntrinsicNames);
    }

    private void FindSchemas<T>(Dictionary<int, string> keyDict, HashSet<string> nameSet) where T : ScriptableObject
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
                    if (!keyDict.ContainsKey(key))
                    {
                        keyDict.Add(key, assetName);
                    }
                }
            }
        }
    }

    private static string FormatDisplayName(string variableName)
    {
        // Add spaces before capital letters, then trim
        return Regex.Replace(variableName, "(\\B[A-Z])", " $1").Trim();
    }

    private Color GetColorForStatus(ProposalStatus status)
    {
        switch (status)
        {
            case ProposalStatus.New: return new Color(0.7f, 1.0f, 0.7f, 1f); // Light Green
            case ProposalStatus.Exists: return new Color(0.8f, 0.8f, 0.8f, 1f); // Light Gray
            case ProposalStatus.KeyConflict: return new Color(1.0f, 0.9f, 0.6f, 1f); // Light Yellow/Orange
            default: return Color.white;
        }
    }

    #endregion
}
#endif