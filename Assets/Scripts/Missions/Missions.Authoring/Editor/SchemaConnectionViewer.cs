using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Missions.Missions.Authoring.Scriptable;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

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

        // UI Toolkit
        private TwoPaneSplitView _splitView;
        private VisualElement _leftPanel;
        private VisualElement _rightPanel;
        private ScrollView _leftScroll;
        private ScrollView _rightScroll;
        private Label _statusLabel;
        private ToolbarToggle _sortAscToggle;
        private EnumField _filterField;
        private EnumField _sortField;
        private ToolbarSearchField _searchField;
        private VisualElement _breadcrumbs;
        private ListView _outgoingListView;
        private ListView _incomingListView;
        private Label _cardSchemas;
        private Label _cardConnections;
        private Label _cardHubs;
        private Label _cardOrphans;
        private Label _cardEndpoints;
        private Label _chipAll, _chipHubs, _chipOrphans, _chipEndpoints, _chipHas, _chipNone;
        private float _splitRatio = LIST_PANEL_WIDTH_RATIO;
        #endregion

        #region Unity Lifecycle
        [MenuItem("Tools/Schema/Connections Viewer")]
        public static void ShowWindow()
        {
            var window = GetWindow<SchemaConnectionViewer>("Schema Connections");
            window.minSize = new Vector2(900, 500);
            window.titleContent = new GUIContent("Schema Connections", EditorGUIUtility.IconContent("d_ScriptableObject Icon").image);
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

        private void OnFocus()
        {
            if (_isDirty || EditorApplication.timeSinceStartup - _lastRefreshTime > DATA_REFRESH_INTERVAL)
            {
                RefreshData();
                RebuildUI();
            }
        }

        public void CreateGUI()
        {
            // Load UXML/USS
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Missions/Missions.Authoring/Editor/UI/SchemaConnectionViewer.uxml");
            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Missions/Missions.Authoring/Editor/UI/MissionsEditor.uss");

            rootVisualElement.Clear();
            if (uss != null) rootVisualElement.styleSheets.Add(uss);
            if (uxml != null) uxml.CloneTree(rootVisualElement);

            // Theme class
            ApplyThemeClass(rootVisualElement);

            // Toolbar wiring
            var refreshBtn = rootVisualElement.Q<ToolbarButton>("refreshButton");
            _filterField = rootVisualElement.Q<EnumField>("filterField");
            _sortField = rootVisualElement.Q<EnumField>("sortField");
            _sortAscToggle = rootVisualElement.Q<ToolbarToggle>("ascToggle");
            _searchField = rootVisualElement.Q<ToolbarSearchField>("searchField");
            _statusLabel = rootVisualElement.Q<Label>("statusLabel");
            _cardSchemas = rootVisualElement.Q<Label>("cardSchemas");
            _cardConnections = rootVisualElement.Q<Label>("cardConnections");
            _cardHubs = rootVisualElement.Q<Label>("cardHubs");
            _cardOrphans = rootVisualElement.Q<Label>("cardOrphans");
            _cardEndpoints = rootVisualElement.Q<Label>("cardEndpoints");
            _chipAll = rootVisualElement.Q<Label>("chipAll");
            _chipHubs = rootVisualElement.Q<Label>("chipHubs");
            _chipOrphans = rootVisualElement.Q<Label>("chipOrphans");
            _chipEndpoints = rootVisualElement.Q<Label>("chipEndpoints");
            _chipHas = rootVisualElement.Q<Label>("chipHas");
            _chipNone = rootVisualElement.Q<Label>("chipNone");

            SetupChip(_chipAll, FilterMode.All);
            SetupChip(_chipHubs, FilterMode.Hubs);
            SetupChip(_chipOrphans, FilterMode.Orphans);
            SetupChip(_chipEndpoints, FilterMode.Endpoints);
            SetupChip(_chipHas, FilterMode.HasConnections);
            SetupChip(_chipNone, FilterMode.NoConnections);

            if (_filterField != null)
            {
                // Removed explicit filter dropdown; chips handle filtering
                _filterField.visible = false;
            }
            if (_sortField != null)
            {
                _sortField.Init(_viewState.sortMode);
                _sortField.RegisterValueChangedCallback(evt => { _viewState.sortMode = (SortMode)evt.newValue; InvalidateFilterCache(); RebuildUI(); });
            }
            if (_sortAscToggle != null)
            {
                _sortAscToggle.value = _viewState.sortAscending;
                _sortAscToggle.RegisterValueChangedCallback(evt => { _viewState.sortAscending = evt.newValue; InvalidateFilterCache(); RebuildUI(); });
            }
            if (_searchField != null)
            {
                _searchField.value = _viewState.searchQuery;
                _searchField.RegisterValueChangedCallback(evt => { _viewState.searchQuery = evt.newValue; InvalidateFilterCache(); RebuildUI(); });
            }
            if (refreshBtn != null)
            {
                refreshBtn.clicked += () => { RefreshData(force: true); RebuildUI(); };
            }
            var exportJsonBtn = rootVisualElement.Q<ToolbarButton>("exportJsonButton");
            var exportCsvBtn = rootVisualElement.Q<ToolbarButton>("exportCsvButton");
            if (exportJsonBtn != null) exportJsonBtn.clicked += ExportCurrentViewJson;
            if (exportCsvBtn != null) exportCsvBtn.clicked += ExportCurrentViewCsv;

            // Split root
            var splitRoot = rootVisualElement.Q<VisualElement>("splitRoot");
            splitRoot.Clear();
            _splitView = new TwoPaneSplitView(0, position.width * _splitRatio, TwoPaneSplitViewOrientation.Horizontal);
            _leftPanel = new VisualElement();
            _rightPanel = new VisualElement();
            _leftScroll = new ScrollView();
            _rightScroll = new ScrollView();
            _leftPanel.Add(_leftScroll);
            _rightPanel.Add(_rightScroll);
            _splitView.Add(_leftPanel);
            _splitView.Add(_rightPanel);
            splitRoot.Add(_splitView);

            // Handle window resize and split drag
            RegisterCallback<GeometryChangedEvent>(_ => UpdateSplitWidth());
            _splitView.RegisterCallback<MouseUpEvent>(_ => PersistSplitWidth());

            RebuildUI();
        }

        private void UpdateSplitWidth()
        {
            if (_splitView == null) return;
            float newWidth = position.width * _splitRatio;
            _splitView.fixedPaneInitialDimension = newWidth;
        }

        private void PersistSplitWidth()
        {
            if (_leftPanel == null) return;
            float current = _leftPanel.resolvedStyle.width;
            _splitRatio = Mathf.Clamp01(current / Mathf.Max(position.width, 1f));
        }

        private void ApplyThemeClass(VisualElement root)
        {
            // Unity has EditorGUIUtility.isProSkin for dark theme
            bool dark = EditorGUIUtility.isProSkin;
            root.RemoveFromClassList("theme--dark");
            root.RemoveFromClassList("theme--light");
            root.AddToClassList(dark ? "theme--dark" : "theme--light");
        }

        private void OnProjectChanged()
        {
            _isDirty = true;
        }

        private void OnGUI()
        {
            // IMGUI fallback only when UIToolkit not initialized
            if (rootVisualElement != null && rootVisualElement.childCount > 0) return;

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

        #region UI Toolkit UI
        private void RebuildUI()
        {
            if (_leftScroll == null || _rightScroll == null) return;
            BuildLeftPanel();
            BuildRightPanel();
            UpdateStatusBar();
        }

        private void BuildLeftPanel()
        {
            _leftScroll.Clear();
            var filtered = GetFilteredAndSortedSchemas();
            if (!filtered.Any())
            {
                _leftScroll.Add(new HelpBox("No schemas match the current filter.", HelpBoxMessageType.Info));
                return;
            }

            foreach (var kvp in filtered)
            {
                var type = kvp.Key;
                var schemas = kvp.Value;
                if (!_viewState.categoryFoldouts.ContainsKey(type)) _viewState.categoryFoldouts[type] = true;

                var fold = new Foldout { text = $"{type.Name} ({schemas.Count})", value = _viewState.categoryFoldouts[type] };
                fold.RegisterValueChangedCallback(evt => _viewState.categoryFoldouts[type] = evt.newValue);

                var list = new ListView
                {
                    itemsSource = schemas,
                    selectionType = SelectionType.Single,
                    virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
                };
                list.makeItem = () =>
                {
                    var row = new VisualElement();
                    row.AddToClassList("schema-row");
                    var stripe = new VisualElement();
                    stripe.AddToClassList("schema-row__stripe");
                    var nameLabel = new Label();
                    nameLabel.AddToClassList("schema-row__name");
                    var info = new Label();
                    info.AddToClassList("schema-row__metrics");
                    var hubBadge = new Label("H"); hubBadge.AddToClassList("badge"); hubBadge.AddToClassList("badge--hub");
                    var orphanBadge = new Label("O"); orphanBadge.AddToClassList("badge"); orphanBadge.AddToClassList("badge--orphan");
                    var endpointBadge = new Label("E"); endpointBadge.AddToClassList("badge"); endpointBadge.AddToClassList("badge--endpoint");
                    row.Add(stripe);
                    row.Add(nameLabel);
                    row.Add(info);
                    row.Add(hubBadge);
                    row.Add(orphanBadge);
                    row.Add(endpointBadge);
                    return row;
                };
                list.bindItem = (el, i) =>
                {
                    var schema = schemas[i];
                    var metrics = _schemaMetrics.GetValueOrDefault(schema);
                    var labels = el.Query<Label>().ToList();
                    // name label is first Label after stripe
                    var nameLabel = labels.FirstOrDefault();
                    var infoLabel = labels.Skip(1).FirstOrDefault();
                    var hubBadge = labels.Skip(2).FirstOrDefault();
                    var orphanBadge = labels.Skip(3).FirstOrDefault();
                    var endpointBadge = labels.Skip(4).FirstOrDefault();
                    if (nameLabel != null) nameLabel.text = schema.name;
                    if (infoLabel != null) infoLabel.text = metrics != null ? $"{metrics.outgoingCount}→ ←{metrics.incomingCount}" : "";

                    // Stripe color
                    var stripe = el.Q<VisualElement>(className: null);
                    if (stripe != null)
                    {
                        // first child is stripe
                        var stripeEl = el.hierarchy[0];
                        if (metrics != null)
                        {
                            if (metrics.isHub) stripeEl.style.backgroundColor = new Color(1f, 0.6f, 0f);
                            else if (metrics.isOrphan) stripeEl.style.backgroundColor = new Color(0.8f, 0.4f, 0.4f);
                            else if (metrics.isEndpoint) stripeEl.style.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
                            else stripeEl.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
                        }
                    }

                    // Badges visibility
                    if (hubBadge != null) hubBadge.style.display = (metrics != null && metrics.isHub) ? DisplayStyle.Flex : DisplayStyle.None;
                    if (orphanBadge != null) orphanBadge.style.display = (metrics != null && metrics.isOrphan) ? DisplayStyle.Flex : DisplayStyle.None;
                    if (endpointBadge != null) endpointBadge.style.display = (metrics != null && metrics.isEndpoint) ? DisplayStyle.Flex : DisplayStyle.None;

                    // Alternate row style
                    el.EnableInClassList("schema-row--odd", i % 2 == 1);
                    el.EnableInClassList("schema-row--even", i % 2 == 0);
                };
                list.onItemsChosen += objs =>
                {
                    var chosen = objs?.OfType<BaseSchema>().FirstOrDefault();
                    if (chosen != null) SelectSchema(chosen);
                };
                list.onSelectionChange += objs =>
                {
                    var selected = objs?.OfType<BaseSchema>().FirstOrDefault();
                    if (selected != null) SelectSchema(selected);
                };

                fold.Add(list);
                _leftScroll.Add(fold);
            }
        }

        private void BuildRightPanel()
        {
            _rightScroll.Clear();

            if (_viewState.selectedSchema == null || !_viewState.drillDownPath.Any())
            {
                var info = new Label("Select a schema from the list to explore its connections")
                {
                    style = { unityTextAlign = TextAnchor.MiddleCenter, marginTop = 40 }
                };
                _rightScroll.Add(info);
                var tips = new VisualElement();
                tips.Add(new Label("Keyboard shortcuts:") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 8 } });
                tips.Add(new Label("• F5 - Refresh data"));
                tips.Add(new Label("• Enter - Ping selected asset"));
                tips.Add(new Label("• Escape - Go back or clear search"));
                _rightScroll.Add(tips);
                return;
            }

            // Breadcrumbs
            _breadcrumbs = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 6 } };
            _breadcrumbs.Add(new Label("Navigation:") { style = { width = 80 } });
            for (int i = 0; i < _viewState.drillDownPath.Count; i++)
            {
                var schema = _viewState.drillDownPath[i];
                bool isLast = i == _viewState.drillDownPath.Count - 1;
                var btn = new Button(() =>
                {
                    int idx = _viewState.drillDownPath.IndexOf(schema);
                    if (idx >= 0 && idx < _viewState.drillDownPath.Count - 1)
                    {
                        _viewState.drillDownPath.RemoveRange(idx + 1, _viewState.drillDownPath.Count - (idx + 1));
                        RebuildUI();
                    }
                }) { text = schema.name };
                btn.SetEnabled(!isLast);
                _breadcrumbs.Add(btn);
                if (!isLast) _breadcrumbs.Add(new Label("→") { style = { width = 15, unityTextAlign = TextAnchor.MiddleCenter } });
            }
            var pingBtn = new Button(() => { EditorGUIUtility.PingObject(_viewState.drillDownPath.Last()); }) { text = "Ping Asset" };
            pingBtn.style.marginLeft = 6;
            _breadcrumbs.Add(pingBtn);
            _rightScroll.Add(_breadcrumbs);

            // Columns
            var columns = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            var currentSchema = _viewState.drillDownPath.Last();
            var outgoing = _outgoingConnections.GetValueOrDefault(currentSchema) ?? new List<BaseSchema>();
            var incoming = _incomingConnections.GetValueOrDefault(currentSchema) ?? new List<BaseSchema>();

            columns.Add(BuildConnectionColumn("Uses (Outgoing)", outgoing));
            var spacer = new VisualElement { style = { width = 8 } };
            columns.Add(spacer);
            columns.Add(BuildConnectionColumn("Used By (Incoming)", incoming));
            _rightScroll.Add(columns);
        }

        private VisualElement BuildConnectionColumn(string title, List<BaseSchema> connections)
        {
            var column = new VisualElement { style = { flexDirection = FlexDirection.Column, flexGrow = 1 } };

            var header = new VisualElement { style = { flexDirection = FlexDirection.Row, height = 22 } };
            header.Add(new Label(title + $" ({connections.Count})") { style = { unityFontStyleAndWeight = FontStyle.Bold } });
            column.Add(header);

            if (!connections.Any())
            {
                column.Add(new HelpBox("No connections", HelpBoxMessageType.None));
                return column;
            }

            var list = new ListView
            {
                itemsSource = connections,
                selectionType = SelectionType.None,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight
            };
            list.makeItem = () =>
            {
                var row = new VisualElement { style = { flexDirection = FlexDirection.Row, height = ROW_HEIGHT } };
                var label = new Label { style = { flexGrow = 1 } };
                var ping = new Button { text = "Ping" };
                ping.style.width = PING_BUTTON_WIDTH;
                row.Add(label);
                row.Add(ping);
                return row;
            };
            list.bindItem = (el, i) =>
            {
                var schema = connections[i];
                el.Q<Label>().text = schema != null ? schema.name : "<null>";
                var btn = el.Q<Button>();
                btn.clicked -= null;
                btn.clicked += () => { if (schema != null) EditorGUIUtility.PingObject(schema); };
                el.RegisterCallback<MouseUpEvent>(_ =>
                {
                    if (schema == null) return;
                    if (_viewState.drillDownPath.Count >= MAX_DRILL_DOWN_DEPTH) {
                        EditorUtility.DisplayDialog("Maximum Depth Reached", "You've reached the maximum drill-down depth to prevent infinite loops.", "OK");
                        return;
                    }
                    if (!_viewState.drillDownPath.Contains(schema))
                    {
                        _viewState.drillDownPath.Add(schema);
                        _viewState.selectedSchema = schema;
                        RebuildUI();
                    }
                });
            };
            column.Add(list);

            return column;
        }

        private void UpdateStatusBar()
        {
            if (_statusLabel == null) return;
            _statusLabel.style.whiteSpace = WhiteSpace.NoWrap;
            _statusLabel.text = $"Schemas: {_totalSchemas}    Categories: {_categorizedSchemas.Count}    Connections: {_totalConnections}    Hubs: {_hubCount}    Orphans: {_orphanCount}    Endpoints: {_endpointCount}";

            if (_cardSchemas != null) _cardSchemas.text = _totalSchemas.ToString();
            if (_cardConnections != null) _cardConnections.text = _totalConnections.ToString();
            if (_cardHubs != null) _cardHubs.text = _hubCount.ToString();
            if (_cardOrphans != null) _cardOrphans.text = _orphanCount.ToString();
            if (_cardEndpoints != null) _cardEndpoints.text = _endpointCount.ToString();

            UpdateActiveChip();
        }

        private void SetupChip(Label chip, FilterMode mode)
        {
            if (chip == null) return;
            chip.AddToClassList("chip");
            chip.AddToClassList("hoverable");
            chip.RegisterCallback<MouseUpEvent>(_ => { _viewState.filterMode = mode; InvalidateFilterCache(); RebuildUI(); });
        }

        private void UpdateActiveChip()
        {
            var chips = new[] { _chipAll, _chipHubs, _chipOrphans, _chipEndpoints, _chipHas, _chipNone };
            foreach (var c in chips)
            {
                if (c == null) continue;
                c.RemoveFromClassList("chip--active");
            }
            Label active = _viewState.filterMode switch
            {
                FilterMode.All => _chipAll,
                FilterMode.Hubs => _chipHubs,
                FilterMode.Orphans => _chipOrphans,
                FilterMode.Endpoints => _chipEndpoints,
                FilterMode.HasConnections => _chipHas,
                FilterMode.NoConnections => _chipNone,
                _ => _chipAll
            };
            active?.AddToClassList("chip--active");
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

        #region IMGUI Fallback Drawing (kept minimal)
        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                var refreshContent = new GUIContent("Refresh", "Refresh all data (F5)");
                if (GUILayout.Button(refreshContent, EditorStyles.toolbarButton, GUILayout.Width(60)))
                {
                    RefreshData(force: true);
                }

                GUILayout.Space(10);

                EditorGUI.BeginChangeCheck();
                var newFilterMode = (FilterMode)EditorGUILayout.EnumPopup(_viewState.filterMode, EditorStyles.toolbarPopup, GUILayout.Width(120));
                if (EditorGUI.EndChangeCheck())
                {
                    _viewState.filterMode = newFilterMode;
                    InvalidateFilterCache();
                }

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
            RebuildUI();
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

        private void ExportCurrentViewJson()
        {
            var data = new
            {
                totalSchemas = _totalSchemas,
                totalConnections = _totalConnections,
                hubs = _hubCount,
                orphans = _orphanCount,
                endpoints = _endpointCount,
                filter = _viewState.filterMode.ToString(),
                sort = _viewState.sortMode.ToString(),
                ascending = _viewState.sortAscending,
                search = _viewState.searchQuery,
                categories = GetFilteredAndSortedSchemas()
                    .ToDictionary(kv => kv.Key.Name, kv => kv.Value.Select(s => s.name).ToArray())
            };
            var json = JsonUtility.ToJson(new SerializableWrapper(data), true);
            var path = EditorUtility.SaveFilePanel("Export JSON", Application.dataPath, "SchemaView", "json");
            if (!string.IsNullOrEmpty(path)) System.IO.File.WriteAllText(path, json);
        }

        private void ExportCurrentViewCsv()
        {
            var path = EditorUtility.SaveFilePanel("Export CSV", Application.dataPath, "SchemaView", "csv");
            if (string.IsNullOrEmpty(path)) return;
            var lines = new List<string> { "Category,Schema,Outgoing,Incoming" };
            var filtered = GetFilteredAndSortedSchemas();
            foreach (var (type, schemas) in filtered)
            {
                foreach (var s in schemas)
                {
                    var m = _schemaMetrics.GetValueOrDefault(s);
                    lines.Add($"{type.Name},{EscapeCsv(s.name)},{m?.outgoingCount ?? 0},{m?.incomingCount ?? 0}");
                }
            }
            System.IO.File.WriteAllLines(path, lines);
        }

        [Serializable]
        private class SerializableWrapper
        {
            public object payload;
            public SerializableWrapper(object payload) { this.payload = payload; }
        }

        private string EscapeCsv(string input)
        {
            if (input.Contains(",") || input.Contains("\""))
            {
                return $"\"{input.Replace("\"", "\"\"")}\"";
            }
            return input;
        }
    }
}