using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Data;
using System;

namespace Missions.Missions.Authoring.Editor
{
    public class SchemaConnectionViewer : EditorWindow
    {
        // Data
        private Dictionary<Type, List<BaseSchema>> categorizedSchemas = new Dictionary<Type, List<BaseSchema>>();
        private Dictionary<BaseSchema, List<BaseSchema>> outgoingConnections = new Dictionary<BaseSchema, List<BaseSchema>>();
        private Dictionary<BaseSchema, List<BaseSchema>> incomingConnections = new Dictionary<BaseSchema, List<BaseSchema>>();

        // UI State
        private Vector2 listScrollPos, detailsScrollPos;
        private BaseSchema selectedSchema;
        private string searchQuery = "";
        private Dictionary<Type, bool> categoryFoldouts = new Dictionary<Type, bool>();
        private FilterMode filterMode = FilterMode.All;
        private const int HubThreshold = 5;
        private List<BaseSchema> drillDownPath = new List<BaseSchema>();

        private static readonly Color SelectionColor = new Color(0.2f, 0.45f, 0.8f, 0.7f);

        private enum FilterMode { All, Orphans, Endpoints, Hubs }

        [MenuItem("Window/Hold/Schema Connection Viewer Pro")]
        public static void ShowWindow()
        {
            var window = GetWindow<SchemaConnectionViewer>("Schema Connections Pro");
            window.minSize = new Vector2(800, 400);
        }

        private void OnEnable() => RefreshData();
        private void OnFocus() => RefreshData();

        private void OnGUI()
        {
            DrawToolbar();
            DrawPanels();
            DrawStatusBar();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button(new GUIContent(" Refresh", EditorGUIUtility.IconContent("d_Refresh").image), EditorStyles.toolbarButton)) RefreshData();
            filterMode = (FilterMode)EditorGUILayout.EnumPopup(filterMode, EditorStyles.toolbarPopup, GUILayout.Width(100));
            GUILayout.FlexibleSpace();
            searchQuery = EditorGUILayout.TextField(searchQuery, EditorStyles.toolbarSearchField, GUILayout.Width(250));
            if (GUILayout.Button("", EditorStyles.toolbarButton)) { searchQuery = ""; GUI.FocusControl(null); }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPanels()
        {
            EditorGUILayout.BeginHorizontal();
            DrawSchemaListPanel();
            DrawDetailsPanel();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSchemaListPanel()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(position.width * 0.45f));
            EditorGUILayout.LabelField("Schema Hierarchy", EditorStyles.boldLabel);
            listScrollPos = EditorGUILayout.BeginScrollView(listScrollPos);

            var filteredCategorizedSchemas = GetFilteredSchemas();
            if (!filteredCategorizedSchemas.Any()) EditorGUILayout.HelpBox("No schemas match the current filter.", MessageType.Info);

            foreach (var category in filteredCategorizedSchemas)
            {
                if (!categoryFoldouts.ContainsKey(category.Key)) categoryFoldouts[category.Key] = true;
                categoryFoldouts[category.Key] = EditorGUILayout.Foldout(categoryFoldouts[category.Key], $"{category.Key.Name} ({category.Value.Count})", true, EditorStyles.foldoutHeader);
                if (categoryFoldouts[category.Key])
                {
                    EditorGUI.indentLevel++;
                    foreach (var schema in category.Value) DrawSchemaEntry(schema);
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawSchemaEntry(BaseSchema schema)
        {
            Rect rowRect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label, GUILayout.Height(20));

            if (selectedSchema == schema)
            {
                EditorGUI.DrawRect(new Rect(0, rowRect.y, position.width, rowRect.height), SelectionColor);
            }

            Rect indentedRect = EditorGUI.IndentedRect(rowRect);

            int outgoingCount = outgoingConnections.TryGetValue(schema, out var uses) ? uses.Count : 0;
            int incomingCount = incomingConnections.TryGetValue(schema, out var refs) ? refs.Count : 0;

            var outgoingContent = new GUIContent($" {outgoingCount}", EditorGUIUtility.IconContent("d_forward").image, "Uses");
            var incomingContent = new GUIContent($" {incomingCount}", EditorGUIUtility.IconContent("d_back").image, "Used By");

            Rect buttonRect = new Rect(indentedRect.x, indentedRect.y, indentedRect.width - 80, indentedRect.height);
            Rect outgoingRect = new Rect(indentedRect.x + indentedRect.width - 80, indentedRect.y, 40, indentedRect.height);
            Rect incomingRect = new Rect(indentedRect.x + indentedRect.width - 40, indentedRect.y, 40, indentedRect.height);

            if (GUI.Button(buttonRect, new GUIContent(schema.name, EditorGUIUtility.ObjectContent(schema, typeof(BaseSchema)).image), EditorStyles.label))
            {
                selectedSchema = schema;
                drillDownPath.Clear();
                drillDownPath.Add(schema);
                GUI.FocusControl(null);
            }

            GUI.Label(outgoingRect, outgoingContent);
            GUI.Label(incomingRect, incomingContent);
        }

        private void DrawDetailsPanel()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandWidth(true));
            detailsScrollPos = EditorGUILayout.BeginScrollView(detailsScrollPos, GUILayout.ExpandHeight(true));

            if (selectedSchema == null || !drillDownPath.Any())
            {
                EditorGUILayout.HelpBox("Select a schema from the list to begin.", MessageType.Info);
            }
            else
            {
                DrawBreadcrumbs();
                EditorGUILayout.Space();
                DrawDrillDownView();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawBreadcrumbs()
        {
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < drillDownPath.Count; i++)
            {
                var schema = drillDownPath[i];
                GUI.enabled = i < drillDownPath.Count - 1;
                if (GUILayout.Button(schema.name, EditorStyles.miniButtonLeft))
                {
                    drillDownPath = drillDownPath.GetRange(0, i + 1);
                    break;
                }
                GUI.enabled = true;
            }
            
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_Search"), EditorStyles.miniButtonRight, GUILayout.ExpandWidth(false)))
            {
                EditorGUIUtility.PingObject(drillDownPath.Last());
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawDrillDownView()
        {
            var currentSchema = drillDownPath.Last();

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
            DrawConnectionColumn("Uses (Outgoing)", outgoingConnections.ContainsKey(currentSchema) ? outgoingConnections[currentSchema] : new List<BaseSchema>());
            DrawConnectionColumn("Used By (Incoming)", incomingConnections.ContainsKey(currentSchema) ? incomingConnections[currentSchema] : new List<BaseSchema>(), true);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawConnectionColumn(string label, List<BaseSchema> connections, bool isIncoming = false)
        {
            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins, GUILayout.Width(position.width * 0.25f));
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            if (!connections.Any()) 
            {
                EditorGUILayout.HelpBox("No connections.", MessageType.None);
            }
            else
            {
                foreach (var conn in connections)
                {
                    Rect rowRect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.miniButton, GUILayout.Height(20));
                    float pingWidth = 25f;
                    Rect mainButtonRect = new Rect(rowRect.x, rowRect.y, rowRect.width - pingWidth, rowRect.height);
                    Rect pingButtonRect = new Rect(rowRect.x + rowRect.width - pingWidth, rowRect.y, pingWidth, rowRect.height);

                    if (GUI.Button(mainButtonRect, new GUIContent(conn.name, EditorGUIUtility.ObjectContent(conn, typeof(BaseSchema)).image), EditorStyles.miniButtonLeft))
                    {
                        drillDownPath.Add(conn);
                    }
                    if (GUI.Button(pingButtonRect, EditorGUIUtility.IconContent("d_Search"), EditorStyles.miniButtonRight))
                    {
                        EditorGUIUtility.PingObject(conn);
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawStatusBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            int totalSchemas = categorizedSchemas.Values.Sum(l => l.Count);
            int totalConnections = outgoingConnections.Values.Sum(list => list.Count);
            EditorGUILayout.LabelField($"Total Schemas: {totalSchemas}", GUILayout.Width(150));
            EditorGUILayout.LabelField($"Categories: {categorizedSchemas.Count}", GUILayout.Width(150));
            EditorGUILayout.LabelField($"Total Connections: {totalConnections}");
            EditorGUILayout.EndHorizontal();
        }

        private void RefreshData()
        {
            categorizedSchemas.Clear();
            outgoingConnections.Clear();
            incomingConnections.Clear();

            var allSchemaAssets = AssetDatabase.FindAssets("t:BaseSchema")
                .Select(guid => AssetDatabase.LoadAssetAtPath<BaseSchema>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(s => s != null)
                .ToList();

            foreach (var schema in allSchemaAssets)
            {
                if (!outgoingConnections.ContainsKey(schema)) outgoingConnections[schema] = new List<BaseSchema>();
                if (!incomingConnections.ContainsKey(schema)) incomingConnections[schema] = new List<BaseSchema>();

                var so = new SerializedObject(schema);
                var iterator = so.GetIterator();
                while (iterator.NextVisible(true))
                {
                    if (iterator.propertyType == SerializedPropertyType.ObjectReference && iterator.objectReferenceValue is BaseSchema referencedSchema)
                    {
                        if (!outgoingConnections[schema].Contains(referencedSchema)) outgoingConnections[schema].Add(referencedSchema);
                        if (!incomingConnections.ContainsKey(referencedSchema)) incomingConnections[referencedSchema] = new List<BaseSchema>();
                        if (!incomingConnections[referencedSchema].Contains(schema)) incomingConnections[referencedSchema].Add(schema);
                    }
                }
            }

            foreach (var schema in allSchemaAssets)
            {
                var type = schema.GetType();
                if (!categorizedSchemas.ContainsKey(type)) categorizedSchemas[type] = new List<BaseSchema>();
                categorizedSchemas[type].Add(schema);
            }
            categorizedSchemas = categorizedSchemas.OrderBy(kvp => kvp.Key.Name).ToDictionary(kvp => kvp.Key, kvp => kvp.Value.OrderBy(s => s.name).ToList());
            Repaint();
        }

        private Dictionary<Type, List<BaseSchema>> GetFilteredSchemas()
        {
            var result = new Dictionary<Type, List<BaseSchema>>();
            var lowerQuery = searchQuery.ToLower();

            foreach (var category in categorizedSchemas)
            {
                IEnumerable<BaseSchema> schemasToProcess = category.Value;

                schemasToProcess = filterMode switch
                {
                    FilterMode.Orphans => schemasToProcess.Where(s => !incomingConnections.ContainsKey(s) || !incomingConnections[s].Any()),
                    FilterMode.Endpoints => schemasToProcess.Where(s => !outgoingConnections.ContainsKey(s) || !outgoingConnections[s].Any()),
                    FilterMode.Hubs => schemasToProcess.Where(s => (incomingConnections.ContainsKey(s) ? incomingConnections[s].Count : 0) + (outgoingConnections.ContainsKey(s) ? outgoingConnections[s].Count : 0) >= HubThreshold),
                    _ => schemasToProcess,
                };

                if (!string.IsNullOrEmpty(searchQuery))
                {
                    schemasToProcess = schemasToProcess.Where(s => s.name.ToLower().Contains(lowerQuery));
                }

                var matchingSchemas = schemasToProcess.ToList();
                if (matchingSchemas.Any()) result[category.Key] = matchingSchemas;
            }
            return result;
        }
    }
}