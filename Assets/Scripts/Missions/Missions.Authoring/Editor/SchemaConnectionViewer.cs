using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Missions.Missions.Authoring.Scriptable;

namespace Missions.Missions.Authoring.Editor
{
    /// <summary>
    /// A professional, high-performance editor tool for visualizing connections between BaseSchema ScriptableObjects.
    /// Features hierarchical navigation, advanced filtering, multi-column drill-down views, and comprehensive analytics.
    /// </summary>
    public class SchemaConnectionViewer : EditorWindow
    {
        #region Constants & Configuration
        private const float LIST_PANEL_WIDTH_RATIO = 0.4f;
        private const float CONNECTION_COLUMN_WIDTH_RATIO = 0.48f;
        private const float ROW_HEIGHT = 22f;
        private const float PING_BUTTON_WIDTH = 45f;
        private const float INDENT_WIDTH = 16f;
        private const int HUB_THRESHOLD = 5;
        private const int MAX_DRILL_DOWN_DEPTH = 10;
        private const double DATA_REFRESH_INTERVAL = 2.0; // seconds
        
        private static readonly Color SELECTION_COLOR = new(0.24f, 0.48f, 0.90f, 0.8f);
        private static readonly Color HOVER_COLOR = new(0.5f, 0.5f, 0.5f, 0.3f);
        private static readonly Color HUB_COLOR = new(1f, 0.6f, 0f, 0.4f);
        private static readonly Color ORPHAN_COLOR = new(0.8f, 0.4f, 0.4f, 0.3f);
        private static readonly Color ENDPOINT_COLOR = new(0.4f, 0.8f, 0.4f, 0.3f);
        
        private static GUIStyle BoldFoldoutStyle
        {
            get
            {
                if (_boldFoldoutStyle == null)
                {
                    _boldFoldoutStyle = new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold };
                }
                return _boldFoldoutStyle;
            }
        }
        #endregion

        #region Data Structures
        [Serializable]
        public class SchemaMetrics
        {
            public int outgoingCount;
            public int incomingCount;
            public int totalConnections => outgoingCount + incomingCount;
            public bool isHub => totalConnections >= HUB_THRESHOLD;
            public bool isOrphan => incomingCount == 0;
            public bool isEndpoint => outgoingCount == 0;
        }

        [Serializable]
        public class ViewState
        {
            public Vector2 listScrollPos;
            public Vector2 detailsScrollPos;
            public string searchQuery = "";
            public FilterMode filterMode = FilterMode.All;
            public SortMode sortMode = SortMode.Name;
            public bool sortAscending = true;
            public Dictionary<Type, bool> categoryFoldouts = new();
            public List<BaseSchema> drillDownPath = new();
            public BaseSchema selectedSchema;
        }

        public enum FilterMode 
        { 
            All, 
            Orphans, 
            Endpoints, 
            Hubs, 
            HasConnections,
            NoConnections 
        }

        public enum SortMode 
        { 
            Name, 
            OutgoingCount, 
            IncomingCount, 
            TotalConnections,
            Type 
        }
        #endregion

        #region Private Fields
        // Core Data
        private Dictionary<Type, List<BaseSchema>> _categorizedSchemas = new();
        private Dictionary<BaseSchema, List<BaseSchema>> _outgoingConnections = new();
        private Dictionary<BaseSchema, List<BaseSchema>> _incomingConnections = new();
        private Dictionary<BaseSchema, SchemaMetrics> _schemaMetrics = new();
        
        // UI State
        private ViewState _viewState = new();
        private BaseSchema _hoveredSchema;
        private double _lastRefreshTime;
        private bool _isDirty = true;
        
        // Performance Optimization
        private Dictionary<Type, List<BaseSchema>> _filteredCache = new();
        private string _lastSearchQuery = "";
        private FilterMode _lastFilterMode = FilterMode.All;
        private SortMode _lastSortMode = SortMode.Name;
        private bool _lastSortAscending = true;

        // Analytics
        private int _totalSchemas;
        private int _totalConnections;
        private int _hubCount;
        private int _orphanCount;
        private int _endpointCount;
        private static GUIStyle _boldFoldoutStyle;

        #endregion

        #region Unity Lifecycle
        [MenuItem("Tools/Schema/Connections Viewer")]
        public static void ShowWindow()
        {
            var window = GetWindow<SchemaConnectionViewer>("Schema Connections");
            window.minSize = new Vector2(900, 500);
            window.titleContent = new GUIContent("Schema Connections", EditorGUIUtility.IconContent("d_ScriptableObject Icon").image);
        }

        [MenuItem("Tools/Schema/Connections Viewer (No Focus)")]
        public static void ShowWindowNoFocus()
        {
            var window = GetWindow<SchemaConnectionViewer>("Schema Connections", false);
            window.minSize = new Vector2(900, 500);
            window.titleContent = new GUIContent("Schema Connections", EditorGUIUtility.IconContent("d_ScriptableObject Icon").image);
        }

        [MenuItem("Tools/Schema/Connections Viewer (Utility)")]
        public static void ShowWindowUtility()
        {
            var window = CreateInstance<SchemaConnectionViewer>();
            window.titleContent = new GUIContent("Schema Connections", EditorGUIUtility.IconContent("d_ScriptableObject Icon").image);
            window.minSize = new Vector2(900, 500);
            window.ShowUtility();
        }

        private void OnEnable()
        {
            EditorApplication.projectChanged += OnProjectChanged;
            RefreshData(force: true);
        }

        private void OnDisable()
        {
            EditorApplication.projectChanged -= OnProjectChanged;
        }

        private void OnProjectChanged()
        {
            _isDirty = true;
        }

        private void OnFocus()
        {
            if (_isDirty || EditorApplication.timeSinceStartup - _lastRefreshTime > DATA_REFRESH_INTERVAL)
            {
                RefreshData();
            }
        }

        private void OnGUI()
        {
            HandleKeyboardInput();
            DrawToolbar();
            DrawMainContent();
            DrawStatusBar();
            
            if (_isDirty)
            {
                _isDirty = false;
                Repaint();
            }
        }
        #endregion

        #region Data Management
        private void RefreshData(bool force = false)
        {
            if (!force && EditorApplication.timeSinceStartup - _lastRefreshTime < DATA_REFRESH_INTERVAL)
                return;

            ClearData();
            var allSchemas = LoadAllSchemas();
            if (!allSchemas.Any()) return;
            
            BuildConnectionMaps(allSchemas);
            CategorizeSchemas(allSchemas);
            CalculateMetrics();
            InvalidateFilterCache();
            
            _lastRefreshTime = EditorApplication.timeSinceStartup;
            _isDirty = true;
        }

        private void ClearData()
        {
            _categorizedSchemas.Clear();
            _outgoingConnections.Clear();
            _incomingConnections.Clear();
            _schemaMetrics.Clear();
            _filteredCache.Clear();
        }

        private List<BaseSchema> LoadAllSchemas()
        {
            var guids = AssetDatabase.FindAssets("t:BaseSchema");
            var schemas = new List<BaseSchema>();
            
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var schema = AssetDatabase.LoadAssetAtPath<BaseSchema>(path);
                if (schema != null) schemas.Add(schema);
            }
            
            return schemas;
        }

        private void BuildConnectionMaps(List<BaseSchema> allSchemas)
        {
            // Initialize all schemas in the dictionaries
            foreach (var schema in allSchemas)
            {
                _outgoingConnections[schema] = new List<BaseSchema>();
                _incomingConnections[schema] = new List<BaseSchema>();
            }

            // Build connections using reflection for better performance
            foreach (var schema in allSchemas)
            {
                var serializedObject = new SerializedObject(schema);
                var iterator = serializedObject.GetIterator();
                
                while (iterator.NextVisible(true))
                {
                    ProcessSerializedProperty(iterator, schema);
                }
            }
        }

        private void ProcessSerializedProperty(SerializedProperty property, BaseSchema sourceSchema)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    if (property.objectReferenceValue is BaseSchema targetSchema)
                    {
                        AddConnection(sourceSchema, targetSchema);
                    }
                    break;
                    
                case SerializedPropertyType.Generic when property.isArray:
                    ProcessArrayProperty(property, sourceSchema);
                    break;
            }
        }

        private void ProcessArrayProperty(SerializedProperty arrayProperty, BaseSchema sourceSchema)
        {
            for (int i = 0; i < arrayProperty.arraySize; i++)
            {
                var element = arrayProperty.GetArrayElementAtIndex(i);
                if (element.propertyType == SerializedPropertyType.ObjectReference && 
                    element.objectReferenceValue is BaseSchema targetSchema)
                {
                    AddConnection(sourceSchema, targetSchema);
                }
            }
        }

        private void AddConnection(BaseSchema source, BaseSchema target)
        {
            if (source == target) return; // Avoid self-references
            
            if (!_outgoingConnections[source].Contains(target))
            {
                _outgoingConnections[source].Add(target);
            }
            
            if (!_incomingConnections[target].Contains(source))
            {
                _incomingConnections[target].Add(source);
            }
        }

        private void CategorizeSchemas(List<BaseSchema> allSchemas)
        {
            var grouped = allSchemas.GroupBy(s => s.GetType())
                                   .OrderBy(g => g.Key.Name)
                                   .ToDictionary(
                                       g => g.Key, 
                                       g => g.OrderBy(s => s.name).ToList()
                                   );
            
            _categorizedSchemas = grouped;
        }

        private void CalculateMetrics()
        {
            _schemaMetrics.Clear();
            _totalSchemas = 0;
            _totalConnections = 0;
            _hubCount = 0;
            _orphanCount = 0;
            _endpointCount = 0;

            foreach (var schemaList in _categorizedSchemas.Values)
            {
                foreach (var schema in schemaList)
                {
                    var metrics = new SchemaMetrics
                    {
                        outgoingCount = _outgoingConnections.GetValueOrDefault(schema)?.Count ?? 0,
                        incomingCount = _incomingConnections.GetValueOrDefault(schema)?.Count ?? 0
                    };
                    
                    _schemaMetrics[schema] = metrics;
                    _totalSchemas++;
                    _totalConnections += metrics.outgoingCount;
                    
                    if (metrics.isHub) _hubCount++;
                    if (metrics.isOrphan) _orphanCount++;
                    if (metrics.isEndpoint) _endpointCount++;
                }
            }
        }
        #endregion

        #region Input Handling
        private void HandleKeyboardInput()
        {
            var e = Event.current;
            if (e.type != EventType.KeyDown) return;

            switch (e.keyCode)
            {
                case KeyCode.F5:
                    RefreshData(force: true);
                    e.Use();
                    break;
                    
                case KeyCode.Escape:
                    if (_viewState.drillDownPath.Count > 1)
                    {
                        _viewState.drillDownPath.RemoveAt(_viewState.drillDownPath.Count - 1);
                        e.Use();
                    }
                    else if (!string.IsNullOrEmpty(_viewState.searchQuery))
                    {
                        _viewState.searchQuery = "";
                        GUI.FocusControl(null);
                        e.Use();
                    }
                    break;
                    
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    if (_viewState.selectedSchema != null)
                    {
                        EditorGUIUtility.PingObject(_viewState.selectedSchema);
                        e.Use();
                    }
                    break;
            }
        }
        #endregion

        #region UI Drawing
        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                // Refresh button with tooltip
                var refreshContent = new GUIContent("Refresh", "Refresh all data (F5)");
                if (GUILayout.Button(refreshContent, EditorStyles.toolbarButton, GUILayout.Width(60)))
                {
                    RefreshData(force: true);
                }

                GUILayout.Space(10);

                // Filter dropdown
                EditorGUI.BeginChangeCheck();
                var newFilterMode = (FilterMode)EditorGUILayout.EnumPopup(_viewState.filterMode, EditorStyles.toolbarPopup, GUILayout.Width(120));
                if (EditorGUI.EndChangeCheck())
                {
                    _viewState.filterMode = newFilterMode;
                    InvalidateFilterCache();
                }

                // Sort controls
                GUILayout.Space(10);
                EditorGUI.BeginChangeCheck();
                var newSortMode = (SortMode)EditorGUILayout.EnumPopup(_viewState.sortMode, EditorStyles.toolbarPopup, GUILayout.Width(120));
                var newSortAscending = GUILayout.Toggle(_viewState.sortAscending, "↑", EditorStyles.toolbarButton, GUILayout.Width(20));
                if (EditorGUI.EndChangeCheck())
                {
                    _viewState.sortMode = newSortMode;
                    _viewState.sortAscending = newSortAscending;
                    InvalidateFilterCache();
                }

                GUILayout.FlexibleSpace();

                // Search field
                EditorGUI.BeginChangeCheck();
                var newSearchQuery = EditorGUILayout.TextField(_viewState.searchQuery, EditorStyles.toolbarSearchField, GUILayout.Width(200));
                if (EditorGUI.EndChangeCheck())
                {
                    _viewState.searchQuery = newSearchQuery;
                    InvalidateFilterCache();
                }

                if (GUILayout.Button("×", EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    _viewState.searchQuery = "";
                    GUI.FocusControl(null);
                    InvalidateFilterCache();
                }
            }
        }

        private void DrawMainContent()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawSchemaListPanel();
                DrawDetailsPanel();
            }
        }

        private void DrawStatusBar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField($"Schemas: {_totalSchemas}", GUILayout.Width(100));
                EditorGUILayout.LabelField($"Categories: {_categorizedSchemas.Count}", GUILayout.Width(100));
                EditorGUILayout.LabelField($"Connections: {_totalConnections}", GUILayout.Width(120));
                EditorGUILayout.LabelField($"Hubs: {_hubCount}", GUILayout.Width(80));
                EditorGUILayout.LabelField($"Orphans: {_orphanCount}", GUILayout.Width(90));
                EditorGUILayout.LabelField($"Endpoints: {_endpointCount}", GUILayout.Width(100));
                GUILayout.FlexibleSpace();
                
                if (_viewState.selectedSchema != null)
                {
                    var metrics = _schemaMetrics.GetValueOrDefault(_viewState.selectedSchema);
                    EditorGUILayout.LabelField($"Selected: {metrics?.outgoingCount ?? 0}→ ←{metrics?.incomingCount ?? 0}");
                }
            }
        }

        private void DrawSchemaListPanel()
        {
            float panelWidth = position.width * LIST_PANEL_WIDTH_RATIO;
            
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Width(panelWidth)))
            {
                using (var scrollView = new EditorGUILayout.ScrollViewScope(_viewState.listScrollPos))
                {
                    _viewState.listScrollPos = scrollView.scrollPosition;
                    
                    var filteredSchemas = GetFilteredAndSortedSchemas();
                    if (!filteredSchemas.Any())
                    {
                        EditorGUILayout.HelpBox("No schemas match the current filter.", MessageType.Info);
                        return;
                    }

                    foreach (var category in filteredSchemas)
                    {
                        DrawSchemaCategory(category.Key, category.Value);
                    }
                }
            }
        }

        private void DrawSchemaCategory(Type categoryType, List<BaseSchema> schemas)
        {
            if (!_viewState.categoryFoldouts.ContainsKey(categoryType))
                _viewState.categoryFoldouts[categoryType] = true;

            var categoryName = $"{categoryType.Name} ({schemas.Count})";
            var isExpanded = EditorGUILayout.Foldout(_viewState.categoryFoldouts[categoryType], categoryName, true, BoldFoldoutStyle);
            _viewState.categoryFoldouts[categoryType] = isExpanded;

            if (!isExpanded) return;

            EditorGUI.indentLevel++;
            foreach (var schema in schemas)
            {
                DrawSchemaEntry(schema);
            }
            EditorGUI.indentLevel--;
        }

        private void DrawSchemaEntry(BaseSchema schema)
        {
            var metrics = _schemaMetrics.GetValueOrDefault(schema);
            var rect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label, GUILayout.Height(ROW_HEIGHT));
            var isSelected = _viewState.selectedSchema == schema;
            var isHovered = rect.Contains(Event.current.mousePosition);

            // Handle mouse events
            if (Event.current.type == EventType.MouseMove && isHovered)
            {
                _hoveredSchema = schema;
                Repaint();
            }

            // Draw background
            DrawSchemaEntryBackground(rect, schema, isSelected, isHovered, metrics);

            // Draw content
            var indentedRect = EditorGUI.IndentedRect(rect);
            DrawSchemaEntryContent(indentedRect, schema, metrics, isSelected);
        }

        private void DrawSchemaEntryBackground(Rect rect, BaseSchema schema, bool isSelected, bool isHovered, SchemaMetrics metrics)
        {
            var fullWidthRect = new Rect(0, rect.y, position.width, rect.height);

            if (isSelected)
            {
                EditorGUI.DrawRect(fullWidthRect, SELECTION_COLOR);
            }
            else if (isHovered)
            {
                EditorGUI.DrawRect(fullWidthRect, HOVER_COLOR);
            }

            // Draw category-specific coloring
            if (metrics != null)
            {
                var categoryRect = new Rect(2, rect.y, 3, rect.height);
                if (metrics.isHub)
                    EditorGUI.DrawRect(categoryRect, HUB_COLOR);
                else if (metrics.isOrphan)
                    EditorGUI.DrawRect(categoryRect, ORPHAN_COLOR);
                else if (metrics.isEndpoint)
                    EditorGUI.DrawRect(categoryRect, ENDPOINT_COLOR);
            }
        }

        private void DrawSchemaEntryContent(Rect indentedRect, BaseSchema schema, SchemaMetrics metrics, bool isSelected)
        {
            var icon = EditorGUIUtility.ObjectContent(schema, typeof(BaseSchema)).image;
            var content = new GUIContent(schema.name, icon, GetSchemaTooltip(schema, metrics));

            // Layout calculations
            const float connectionInfoWidth = 80f;
            var nameRect = new Rect(indentedRect.x, indentedRect.y, 
                                   indentedRect.width - connectionInfoWidth, indentedRect.height);
            var connectionRect = new Rect(nameRect.xMax, indentedRect.y, 
                                        connectionInfoWidth, indentedRect.height);

            // Draw schema name button
            if (GUI.Button(nameRect, content, EditorStyles.label))
            {
                SelectSchema(schema);
            }

            // Draw connection info
            if (metrics != null)
            {
                var connectionText = $"{metrics.outgoingCount}→ ←{metrics.incomingCount}";
                var connectionStyle = new GUIStyle(EditorStyles.miniLabel) 
                { 
                    alignment = TextAnchor.MiddleRight,
                    normal = { textColor = isSelected ? Color.white : Color.gray }
                };
                GUI.Label(connectionRect, connectionText, connectionStyle);
            }
        }

        private void SelectSchema(BaseSchema schema)
        {
            _viewState.selectedSchema = schema;
            _viewState.drillDownPath.Clear();
            _viewState.drillDownPath.Add(schema);
            GUI.FocusControl(null);
            Repaint();
        }

        private string GetSchemaTooltip(BaseSchema schema, SchemaMetrics metrics)
        {
            if (metrics == null) return schema.name;
            
            var tooltip = $"{schema.name}\n";
            tooltip += $"Type: {schema.GetType().Name}\n";
            tooltip += $"Uses: {metrics.outgoingCount} schemas\n";
            tooltip += $"Used by: {metrics.incomingCount} schemas\n";
            
            if (metrics.isHub) tooltip += "Status: Hub (high connectivity)\n";
            if (metrics.isOrphan) tooltip += "Status: Orphan (no incoming connections)\n";
            if (metrics.isEndpoint) tooltip += "Status: Endpoint (no outgoing connections)\n";
            
            return tooltip.TrimEnd('\n');
        }

        private void DrawDetailsPanel()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.ExpandWidth(true)))
            {
                using (var scrollView = new EditorGUILayout.ScrollViewScope(_viewState.detailsScrollPos, GUILayout.ExpandHeight(true)))
                {
                    _viewState.detailsScrollPos = scrollView.scrollPosition;

                    if (_viewState.selectedSchema == null || !_viewState.drillDownPath.Any())
                    {
                        DrawEmptyDetailsState();
                    }
                    else
                    {
                        DrawBreadcrumbs();
                        EditorGUILayout.Space(5);
                        DrawConnectionColumns();
                    }
                }
            }
        }

        private void DrawEmptyDetailsState()
        {
            EditorGUILayout.Space(50);
            var style = new GUIStyle(EditorStyles.centeredGreyMiniLabel) { fontSize = 14 };
            EditorGUILayout.LabelField("Select a schema from the list to explore its connections", style);
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Keyboard shortcuts:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("• F5 - Refresh data");
            EditorGUILayout.LabelField("• Enter - Ping selected asset");
            EditorGUILayout.LabelField("• Escape - Go back or clear search");
        }

        private void DrawBreadcrumbs()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Navigation:", GUILayout.Width(70));
                
                for (int i = 0; i < _viewState.drillDownPath.Count; i++)
                {
                    var schema = _viewState.drillDownPath[i];
                    var isLast = i == _viewState.drillDownPath.Count - 1;
                    
                    GUI.enabled = !isLast;
                    var buttonStyle = isLast ? EditorStyles.miniButtonMid : EditorStyles.miniButtonLeft;
                    
                    if (GUILayout.Button(schema.name, buttonStyle, GUILayout.MaxWidth(150)))
                    {
                        _viewState.drillDownPath.RemoveRange(i + 1, _viewState.drillDownPath.Count - (i + 1));
                        break;
                    }
                    
                    if (!isLast)
                    {
                        GUILayout.Label("→", GUILayout.Width(15));
                    }
                }
                GUI.enabled = true;

                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("Ping Asset", EditorStyles.miniButtonRight, GUILayout.Width(80)))
                {
                    EditorGUIUtility.PingObject(_viewState.drillDownPath.Last());
                }
            }
        }

        private void DrawConnectionColumns()
        {
            var currentSchema = _viewState.drillDownPath.Last();
            var detailsPanelWidth = position.width * (1 - LIST_PANEL_WIDTH_RATIO) - 20;

            using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandHeight(true)))
            {
                var outgoingConnections = _outgoingConnections.GetValueOrDefault(currentSchema) ?? new List<BaseSchema>();
                var incomingConnections = _incomingConnections.GetValueOrDefault(currentSchema) ?? new List<BaseSchema>();

                DrawConnectionColumn("Uses (Outgoing)", outgoingConnections, detailsPanelWidth, Color.cyan);
                GUILayout.Space(5);
                DrawConnectionColumn("Used By (Incoming)", incomingConnections, detailsPanelWidth, Color.yellow);
            }
        }

        private void DrawConnectionColumn(string title, List<BaseSchema> connections, float panelWidth, Color accentColor)
        {
            var columnWidth = panelWidth * CONNECTION_COLUMN_WIDTH_RATIO;
            
            using (new EditorGUILayout.VerticalScope(EditorStyles.inspectorDefaultMargins, GUILayout.Width(columnWidth)))
            {
                // Header
                var headerRect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.boldLabel, GUILayout.Height(22));
                EditorGUI.DrawRect(new Rect(headerRect.x, headerRect.y, 3, headerRect.height), accentColor);
                EditorGUI.LabelField(new Rect(headerRect.x + 5, headerRect.y, headerRect.width - 5, headerRect.height), 
                                   $"{title} ({connections.Count})", EditorStyles.boldLabel);

                if (!connections.Any())
                {
                    EditorGUILayout.HelpBox("No connections", MessageType.None);
                    return;
                }

                // Connection entries
                foreach (var connection in connections)
                {
                    DrawConnectionEntry(connection);
                }
            }
        }

        private void DrawConnectionEntry(BaseSchema schema)
        {
            var rect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.miniButton, GUILayout.Height(ROW_HEIGHT));
            var mainButtonRect = new Rect(rect.x, rect.y, rect.width - PING_BUTTON_WIDTH - 2, rect.height);
            var pingButtonRect = new Rect(rect.xMax - PING_BUTTON_WIDTH, rect.y, PING_BUTTON_WIDTH, rect.height);

            var icon = EditorGUIUtility.ObjectContent(schema, typeof(BaseSchema)).image;
            var content = new GUIContent(schema.name, icon);

            if (GUI.Button(mainButtonRect, content, EditorStyles.miniButtonLeft))
            {
                if (_viewState.drillDownPath.Count >= MAX_DRILL_DOWN_DEPTH)
                {
                    EditorUtility.DisplayDialog("Maximum Depth Reached", 
                                              "You've reached the maximum drill-down depth to prevent infinite loops.", "OK");
                }
                else if (!_viewState.drillDownPath.Contains(schema))
                {
                    _viewState.drillDownPath.Add(schema);
                }
            }

            if (GUI.Button(pingButtonRect, "Ping", EditorStyles.miniButtonRight))
            {
                EditorGUIUtility.PingObject(schema);
            }
        }

        #endregion

        #region Filtering & Sorting
        private Dictionary<Type, List<BaseSchema>> GetFilteredAndSortedSchemas()
        {
            // Check cache validity
            if (_filteredCache.Count > 0 && 
                _lastSearchQuery == _viewState.searchQuery &&
                _lastFilterMode == _viewState.filterMode &&
                _lastSortMode == _viewState.sortMode &&
                _lastSortAscending == _viewState.sortAscending)
            {
                return _filteredCache;
            }

            // Rebuild cache
            var result = new Dictionary<Type, List<BaseSchema>>();
            var lowerQuery = _viewState.searchQuery.ToLower();

            foreach (var (type, schemas) in _categorizedSchemas)
            {
                var filteredSchemas = schemas.Where(schema => PassesFilter(schema, lowerQuery)).ToList();
                
                if (filteredSchemas.Any())
                {
                    result[type] = SortSchemas(filteredSchemas);
                }
            }

            // Update cache
            _filteredCache = result;
            _lastSearchQuery = _viewState.searchQuery;
            _lastFilterMode = _viewState.filterMode;
            _lastSortMode = _viewState.sortMode;
            _lastSortAscending = _viewState.sortAscending;

            return result;
        }

        private bool PassesFilter(BaseSchema schema, string lowerQuery)
        {
            // Search filter
            if (!string.IsNullOrEmpty(lowerQuery) && !schema.name.ToLower().Contains(lowerQuery))
                return false;

            // Category filter
            var metrics = _schemaMetrics.GetValueOrDefault(schema);
            return _viewState.filterMode switch
            {
                FilterMode.Orphans => metrics?.isOrphan == true,
                FilterMode.Endpoints => metrics?.isEndpoint == true,
                FilterMode.Hubs => metrics?.isHub == true,
                FilterMode.HasConnections => metrics?.totalConnections > 0,
                FilterMode.NoConnections => metrics?.totalConnections == 0,
                _ => true
            };
        }

        private List<BaseSchema> SortSchemas(List<BaseSchema> schemas)
        {
            IOrderedEnumerable<BaseSchema> ordered = _viewState.sortMode switch
            {
                SortMode.Name => schemas.OrderBy(s => s.name),
                SortMode.OutgoingCount => schemas.OrderBy(s => _schemaMetrics.GetValueOrDefault(s)?.outgoingCount ?? 0),
                SortMode.IncomingCount => schemas.OrderBy(s => _schemaMetrics.GetValueOrDefault(s)?.incomingCount ?? 0),
                SortMode.TotalConnections => schemas.OrderBy(s => _schemaMetrics.GetValueOrDefault(s)?.totalConnections ?? 0),
                SortMode.Type => schemas.OrderBy(s => s.GetType().Name),
                _ => schemas.OrderBy(s => s.name)
            };

            return _viewState.sortAscending ? ordered.ToList() : ordered.Reverse().ToList();
        }

        private void InvalidateFilterCache()
        {
            _filteredCache.Clear();
        }
        #endregion
    }
}