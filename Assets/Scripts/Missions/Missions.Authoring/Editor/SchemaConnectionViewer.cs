using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Data;
using System;

namespace Missions.Missions.Authoring.Editor
{
    /// <summary>
    /// A professional, robust editor tool for visualizing connections between BaseSchema ScriptableObjects.
    /// Provides a hierarchical list, advanced filtering, and a multi-column drill-down navigation view.
    /// This code is designed to be clean, maintainable, and self-contained, avoiding unreliable built-in icon strings.
    /// </summary>
    public class SchemaConnectionViewer : EditorWindow
    {
        #region Constants & Static
        private const float LIST_PANEL_WIDTH_RATIO = 0.45f;
        private const float CONNECTION_COLUMN_WIDTH_RATIO = 0.5f; // Relative to the details panel
        private const float ROW_HEIGHT = 20f;
        private const float PING_BUTTON_WIDTH = 50f;
        private const int HUB_THRESHOLD = 5;
        private static readonly Color SelectionColor = new Color(0.2f, 0.45f, 0.8f, 0.7f);
        #endregion

        #region Private Fields
        // Data Stores
        private Dictionary<Type, List<BaseSchema>> _categorizedSchemas = new Dictionary<Type, List<BaseSchema>>();
        private Dictionary<BaseSchema, List<BaseSchema>> _outgoingConnections = new Dictionary<BaseSchema, List<BaseSchema>>();
        private Dictionary<BaseSchema, List<BaseSchema>> _incomingConnections = new Dictionary<BaseSchema, List<BaseSchema>>();

        // UI State
        private Vector2 _listScrollPos, _detailsScrollPos;
        private BaseSchema _selectedSchema;
        private string _searchQuery = "";
        private Dictionary<Type, bool> _categoryFoldouts = new Dictionary<Type, bool>();
        private FilterMode _filterMode = FilterMode.All;
        private List<BaseSchema> _drillDownPath = new List<BaseSchema>();
        #endregion

        private enum FilterMode { All, Orphans, Endpoints, Hubs }

        [MenuItem("Window/Hold/Schema Connection Viewer Pro")]
        public static void ShowWindow()
        {
            var window = GetWindow<SchemaConnectionViewer>("Schema Connections Pro");
            window.minSize = new Vector2(800, 400);
        }

        #region Unity Methods
        private void OnEnable() => RefreshData();
        private void OnFocus() => RefreshData();

        private void OnGUI()
        {
            DrawToolbar();
            DrawPanels();
            DrawStatusBar();
        }
        #endregion

        #region Data Handling
        private void RefreshData()
        {
            // Clear existing data
            _categorizedSchemas.Clear();
            _outgoingConnections.Clear();
            _incomingConnections.Clear();

            var allSchemaAssets = GetAllSchemaAssets();
            CalculateConnections(allSchemaAssets);
            CategorizeSchemas(allSchemaAssets);

            Repaint();
        }

        private List<BaseSchema> GetAllSchemaAssets()
        {
            return AssetDatabase.FindAssets("t:BaseSchema")
                .Select(guid => AssetDatabase.LoadAssetAtPath<BaseSchema>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(s => s != null)
                .ToList();
        }

        private void CalculateConnections(List<BaseSchema> allSchemas)
        {
            foreach (var schema in allSchemas)
            {
                if (!_outgoingConnections.ContainsKey(schema)) _outgoingConnections[schema] = new List<BaseSchema>();
                if (!_incomingConnections.ContainsKey(schema)) _incomingConnections[schema] = new List<BaseSchema>();

                var so = new SerializedObject(schema);
                var iterator = so.GetIterator();
                while (iterator.NextVisible(true))
                {
                    if (iterator.propertyType == SerializedPropertyType.ObjectReference && iterator.objectReferenceValue is BaseSchema referencedSchema)
                    {
                        if (!_outgoingConnections[schema].Contains(referencedSchema)) _outgoingConnections[schema].Add(referencedSchema);
                        if (!_incomingConnections.ContainsKey(referencedSchema)) _incomingConnections[referencedSchema] = new List<BaseSchema>();
                        if (!_incomingConnections[referencedSchema].Contains(schema)) _incomingConnections[referencedSchema].Add(schema);
                    }
                }
            }
        }

        private void CategorizeSchemas(List<BaseSchema> allSchemas)
        {
            foreach (var schema in allSchemas)
            {
                var type = schema.GetType();
                if (!_categorizedSchemas.ContainsKey(type)) _categorizedSchemas[type] = new List<BaseSchema>();
                _categorizedSchemas[type].Add(schema);
            }
            _categorizedSchemas = _categorizedSchemas.OrderBy(kvp => kvp.Key.Name)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.OrderBy(s => s.name).ToList());
        }
        #endregion

        #region UI Drawing

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton)) RefreshData();
            _filterMode = (FilterMode)EditorGUILayout.EnumPopup(_filterMode, EditorStyles.toolbarPopup, GUILayout.Width(100));
            GUILayout.FlexibleSpace();

            // Search field with a reliable clear button
            _searchQuery = EditorGUILayout.TextField(_searchQuery, EditorStyles.toolbarSearchField, GUILayout.Width(250));
            if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(22)))
            {
                _searchQuery = "";
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPanels()
        {
            EditorGUILayout.BeginHorizontal();
            DrawSchemaListPanel();
            DrawDetailsPanel();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawStatusBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            int totalSchemas = _categorizedSchemas.Values.Sum(l => l.Count);
            int totalConnections = _outgoingConnections.Values.Sum(list => list.Count);
            EditorGUILayout.LabelField($"Total Schemas: {totalSchemas}", GUILayout.Width(150));
            EditorGUILayout.LabelField($"Categories: {_categorizedSchemas.Count}", GUILayout.Width(150));
            EditorGUILayout.LabelField($"Total Connections: {totalConnections}");
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSchemaListPanel()
        {
            float panelWidth = position.width * LIST_PANEL_WIDTH_RATIO;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(panelWidth));
            _listScrollPos = EditorGUILayout.BeginScrollView(_listScrollPos);

            var filteredSchemas = GetFilteredSchemas();
            if (!filteredSchemas.Any()) EditorGUILayout.HelpBox("No schemas match the current filter.", MessageType.Info);

            foreach (var category in filteredSchemas)
            {
                DrawSchemaCategory(category.Key, category.Value);
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawSchemaCategory(Type categoryType, List<BaseSchema> schemas)
        {
            if (!_categoryFoldouts.ContainsKey(categoryType)) _categoryFoldouts[categoryType] = true;
            _categoryFoldouts[categoryType] = EditorGUILayout.Foldout(_categoryFoldouts[categoryType], $"{categoryType.Name} ({schemas.Count})", true, EditorStyles.foldoutHeader);
            if (_categoryFoldouts[categoryType])
            {
                EditorGUI.indentLevel++;
                foreach (var schema in schemas) DrawSchemaEntry(schema);
                EditorGUI.indentLevel--;
            }
        }

        private void DrawSchemaEntry(BaseSchema schema)
        {
            Rect rowRect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label, GUILayout.Height(ROW_HEIGHT));

            // Draw selection highlight across the full, non-indented width
            if (_selectedSchema == schema)
            {
                EditorGUI.DrawRect(new Rect(0, rowRect.y, position.width, rowRect.height), SelectionColor);
            }

            Rect indentedRect = EditorGUI.IndentedRect(rowRect);

            // Define content and layout
            var schemaContent = new GUIContent(schema.name, EditorGUIUtility.ObjectContent(schema, typeof(BaseSchema)).image);
            int outgoingCount = _outgoingConnections.TryGetValue(schema, out var uses) ? uses.Count : 0;
            int incomingCount = _incomingConnections.TryGetValue(schema, out var refs) ? refs.Count : 0;
            var outgoingContent = new GUIContent($"> {outgoingCount}", "Uses");
            var incomingContent = new GUIContent($"< {incomingCount}", "Used By");

            float outgoingWidth = 40f;
            float incomingWidth = 40f;
            float buttonWidth = indentedRect.width - outgoingWidth - incomingWidth;

            Rect buttonRect = new Rect(indentedRect.x, indentedRect.y, buttonWidth, indentedRect.height);
            Rect outgoingRect = new Rect(buttonRect.xMax, indentedRect.y, outgoingWidth, indentedRect.height);
            Rect incomingRect = new Rect(outgoingRect.xMax, indentedRect.y, incomingWidth, indentedRect.height);

            // Draw controls
            if (GUI.Button(buttonRect, schemaContent, EditorStyles.label))
            {
                _selectedSchema = schema;
                _drillDownPath.Clear();
                _drillDownPath.Add(schema);
                GUI.FocusControl(null);
            }
            GUI.Label(outgoingRect, outgoingContent);
            GUI.Label(incomingRect, incomingContent);
        }

        private void DrawDetailsPanel()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandWidth(true));
            _detailsScrollPos = EditorGUILayout.BeginScrollView(_detailsScrollPos, GUILayout.ExpandHeight(true));

            if (_selectedSchema == null || !_drillDownPath.Any())
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
            for (int i = 0; i < _drillDownPath.Count; i++)
            {
                var schema = _drillDownPath[i];
                GUI.enabled = i < _drillDownPath.Count - 1;
                if (GUILayout.Button(schema.name, EditorStyles.miniButtonLeft)) { _drillDownPath.RemoveRange(i + 1, _drillDownPath.Count - (i + 1)); break; }
                GUI.enabled = true;
            }

            if (GUILayout.Button("Ping", EditorStyles.miniButtonRight, GUILayout.ExpandWidth(false))) { EditorGUIUtility.PingObject(_drillDownPath.Last()); }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawDrillDownView()
        {
            var currentSchema = _drillDownPath.Last();
            float panelWidth = (position.width * (1 - LIST_PANEL_WIDTH_RATIO)) - 20; // Details panel width

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
            DrawConnectionColumn("Uses (Outgoing)", _outgoingConnections.ContainsKey(currentSchema) ? _outgoingConnections[currentSchema] : new List<BaseSchema>(), panelWidth);
            DrawConnectionColumn("Used By (Incoming)", _incomingConnections.ContainsKey(currentSchema) ? _incomingConnections[currentSchema] : new List<BaseSchema>(), panelWidth);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawConnectionColumn(string label, List<BaseSchema> connections, float panelWidth)
        {
            EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins, GUILayout.Width(panelWidth * CONNECTION_COLUMN_WIDTH_RATIO));
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            if (!connections.Any()) { EditorGUILayout.HelpBox("No connections.", MessageType.None); }
            else
            {
                foreach (var conn in connections) DrawConnectionColumnEntry(conn);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawConnectionColumnEntry(BaseSchema schema)
        {
            Rect rowRect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.miniButton, GUILayout.Height(ROW_HEIGHT));
            Rect mainButtonRect = new Rect(rowRect.x, rowRect.y, rowRect.width - PING_BUTTON_WIDTH, rowRect.height);
            Rect pingButtonRect = new Rect(rowRect.x + rowRect.width - PING_BUTTON_WIDTH, rowRect.y, PING_BUTTON_WIDTH, rowRect.height);

            if (GUI.Button(mainButtonRect, new GUIContent(schema.name, EditorGUIUtility.ObjectContent(schema, typeof(BaseSchema)).image), EditorStyles.miniButtonLeft)) { _drillDownPath.Add(schema); }
            if (GUI.Button(pingButtonRect, "Ping", EditorStyles.miniButtonRight)) { EditorGUIUtility.PingObject(schema); }
        }

        private Dictionary<Type, List<BaseSchema>> GetFilteredSchemas()
        {
            var result = new Dictionary<Type, List<BaseSchema>>();
            var lowerQuery = _searchQuery.ToLower();

            foreach (var category in _categorizedSchemas)
            {
                IEnumerable<BaseSchema> schemasToProcess = category.Value;

                schemasToProcess = _filterMode switch
                {
                    FilterMode.Orphans => schemasToProcess.Where(s => !_incomingConnections.ContainsKey(s) || !_incomingConnections[s].Any()),
                    FilterMode.Endpoints => schemasToProcess.Where(s => !_outgoingConnections.ContainsKey(s) || !_outgoingConnections[s].Any()),
                    FilterMode.Hubs => schemasToProcess.Where(s => (_incomingConnections.get(s)?.Count ?? 0) + (_outgoingConnections.get(s)?.Count ?? 0) >= HUB_THRESHOLD),
                    _ => schemasToProcess,
                };

                if (!string.IsNullOrEmpty(lowerQuery)) { schemasToProcess = schemasToProcess.Where(s => s.name.ToLower().Contains(lowerQuery)); }

                var matchingSchemas = schemasToProcess.ToList();
                if (matchingSchemas.Any()) result[category.Key] = matchingSchemas;
            }
            return result;
        }
        #endregion
    }

    public static class DictionaryExtensions
    {
        public static TValue get<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        {
            dict.TryGetValue(key, out TValue val);
            return val;
        }
    }
}
