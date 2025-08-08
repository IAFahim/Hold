using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Missions.Missions.Authoring.Scriptable;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Missions.Missions.Authoring.Editor
{
    [CustomEditor(typeof(BaseSchema), true)]
    public class BaseSchemaEditor : UnityEditor.Editor
    {
        private List<BaseSchema> outgoingConnections;
        private List<BaseSchema> incomingConnections;

        private bool showOutgoing = true;
        private bool showIncoming = true;

        // UI Toolkit elements
        private VisualElement root;
        private Foldout outgoingFoldout;
        private Foldout incomingFoldout;
        private ListView outgoingListView;
        private ListView incomingListView;
        private Button refreshButton;
        private Label statsLabel;

        void OnEnable()
        {
            FindConnections();
        }

        public override VisualElement CreateInspectorGUI()
        {
            // Load UXML/USS
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Missions/Missions.Authoring/Editor/UI/BaseSchemaInspector.uxml");
            var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Missions/Missions.Authoring/Editor/UI/MissionsEditor.uss");

            root = new VisualElement();
            if (uss != null) root.styleSheets.Add(uss);
            if (uxml != null)
            {
                uxml.CloneTree(root);
            }

            // Theme
            ApplyThemeClass(root);

            // Default inspector
            var defaultContainer = root.Q<VisualElement>("defaultInspector");
            if (defaultContainer != null)
            {
                var defaultInspector = BuildDefaultInspector(serializedObject);
                defaultContainer.Add(defaultInspector);
            }

            // Hook up UI elements
            refreshButton = root.Q<Button>("refreshButton");
            statsLabel = root.Q<Label>("statsLabel");
            outgoingFoldout = root.Q<Foldout>("outgoingFoldout");
            incomingFoldout = root.Q<Foldout>("incomingFoldout");
            outgoingListView = root.Q<ListView>("outgoingList");
            incomingListView = root.Q<ListView>("incomingList");

            // If UXML was not found or did not include expected elements, build UI programmatically
            if (defaultContainer == null)
            {
                var defaultInspector = BuildDefaultInspector(serializedObject);
                root.Add(defaultInspector);
            }
            if (outgoingFoldout == null || incomingFoldout == null || outgoingListView == null || incomingListView == null)
            {
                // Header
                root.Add(new Label("Schema Connections") { style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 4, marginBottom = 2 } });
                // Toolbar
                var toolbar = new Toolbar();
                refreshButton = new ToolbarButton(() => { FindConnections(); RefreshUILists(); }) { text = "Refresh Connections" };
                statsLabel = new Label();
                toolbar.Add(refreshButton);
                toolbar.Add(new ToolbarSpacer());
                toolbar.Add(statsLabel);
                root.Add(toolbar);
                // Panel
                var panel = new VisualElement();
                panel.AddToClassList("panel");
                outgoingFoldout = new Foldout { text = $"Uses ({outgoingConnections.Count})", value = showOutgoing };
                incomingFoldout = new Foldout { text = $"Referenced By ({incomingConnections.Count})", value = showIncoming };
                outgoingListView = new ListView();
                incomingListView = new ListView();
                SetupConnectionsListView(outgoingListView, outgoingConnections);
                SetupConnectionsListView(incomingListView, incomingConnections);
                outgoingFoldout.Add(outgoingListView);
                incomingFoldout.Add(incomingListView);
                panel.Add(outgoingFoldout);
                panel.Add(incomingFoldout);
                root.Add(panel);
            }

            if (refreshButton != null)
            {
                refreshButton.clicked += () =>
                {
                    FindConnections();
                    RefreshUILists();
                };
            }

            if (outgoingFoldout != null)
            {
                outgoingFoldout.value = showOutgoing;
                outgoingFoldout.RegisterValueChangedCallback(evt => showOutgoing = evt.newValue);
            }
            if (incomingFoldout != null)
            {
                incomingFoldout.value = showIncoming;
                incomingFoldout.RegisterValueChangedCallback(evt => showIncoming = evt.newValue);
            }

            // Create list views
            SetupConnectionsListView(outgoingListView, outgoingConnections);
            SetupConnectionsListView(incomingListView, incomingConnections);

            UpdateStatsLabel(statsLabel);
            RefreshUILists();

            return root;
        }

        private void ApplyThemeClass(VisualElement rootElement)
        {
            bool dark = EditorGUIUtility.isProSkin;
            rootElement.RemoveFromClassList("theme--dark");
            rootElement.RemoveFromClassList("theme--light");
            rootElement.AddToClassList(dark ? "theme--dark" : "theme--light");
        }

        private void SetupConnectionsListView(ListView listView, List<BaseSchema> source)
        {
            if (listView == null) return;
            listView.itemsSource = source;
            listView.selectionType = SelectionType.Single;
            listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;

            listView.makeItem = () =>
            {
                var row = new VisualElement { style = { flexDirection = FlexDirection.Row, height = 20 } };
                var objField = new ObjectField
                {
                    objectType = typeof(BaseSchema),
                    allowSceneObjects = false
                };
                objField.SetEnabled(false);
                objField.style.flexGrow = 1;
                var openButton = new Button { text = "Open" };
                openButton.style.width = 50;
                var pingButton = new Button { text = "Ping" };
                pingButton.style.marginLeft = 6;
                pingButton.style.width = 50;
                row.Add(objField);
                row.Add(openButton);
                row.Add(pingButton);
                return row;
            };

            listView.bindItem = (element, i) =>
            {
                var schema = source.ElementAtOrDefault(i);
                var objField = element.Q<ObjectField>();
                var buttons = element.Query<Button>().ToList();
                var openButton = buttons.ElementAtOrDefault(0);
                var pingButton = buttons.ElementAtOrDefault(1);
                objField.value = schema;

                if (openButton != null)
                {
                    openButton.clicked -= null;
                    openButton.clicked += () =>
                    {
                        if (schema != null)
                        {
                            SchemaConnectionViewer.ShowWindow();
                            // Select in viewer by pinging; advanced selection could be added via static API
                            EditorGUIUtility.PingObject(schema);
                        }
                    };
                }

                if (pingButton != null)
                {
                    pingButton.clicked -= null;
                    pingButton.clicked += () =>
                    {
                        if (schema != null)
                        {
                            EditorGUIUtility.PingObject(schema);
                            Selection.activeObject = schema;
                        }
                    };
                }

                element.RegisterCallback<MouseUpEvent>(_ =>
                {
                    if (schema != null)
                    {
                        Selection.activeObject = schema;
                    }
                });
            };
        }

        private VisualElement BuildDefaultInspector(SerializedObject so)
        {
            var container = new VisualElement();
            var iterator = so.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                if (iterator.name == "m_Script")
                {
                    var scriptField = new PropertyField(iterator.Copy()) { name = iterator.propertyPath };
                    scriptField.SetEnabled(false);
                    container.Add(scriptField);
                }
                else
                {
                    var field = new PropertyField(iterator.Copy()) { name = iterator.propertyPath };
                    container.Add(field);
                }
                enterChildren = false;
            }
            container.Bind(so);
            return container;
        }

        public override void OnInspectorGUI()
        {
            // Keep IMGUI fallback for older Unity versions if CreateInspectorGUI is ignored
            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Schema Connections", EditorStyles.boldLabel);

            if (GUILayout.Button("Refresh Connections"))
            {
                FindConnections();
            }

            DrawConnectionsBox();
        }

        private void DrawConnectionsBox()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // --- Outgoing Connections ---
            showOutgoing = EditorGUILayout.Foldout(showOutgoing, $"Uses ({outgoingConnections.Count})", true, EditorStyles.foldoutHeader);
            if (showOutgoing)
            {
                DrawConnectionList(outgoingConnections, "This schema does not reference any other schemas.");
            }

            // --- Incoming Connections ---
            showIncoming = EditorGUILayout.Foldout(showIncoming, $"Referenced By ({incomingConnections.Count})", true, EditorStyles.foldoutHeader);
            if (showIncoming)
            {
                DrawConnectionList(incomingConnections, "This schema is not referenced by any other schemas.");
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawConnectionList(List<BaseSchema> connections, string emptyMessage)
        {
            if (connections.Any())
            {
                EditorGUI.indentLevel++;
                foreach (var conn in connections)
                {
                    EditorGUILayout.ObjectField(conn, typeof(BaseSchema), false);
                }
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.HelpBox(emptyMessage, MessageType.None);
            }
        }

        private void FindConnections()
        {
            outgoingConnections = new List<BaseSchema>();
            incomingConnections = new List<BaseSchema>();
            var targetSchema = target as BaseSchema;
            if (targetSchema == null) return;

            // Find outgoing connections from this schema
            var so = new SerializedObject(targetSchema);
            var iterator = so.GetIterator();
            while (iterator.NextVisible(true))
            {
                if (iterator.propertyType == SerializedPropertyType.ObjectReference && iterator.objectReferenceValue is BaseSchema referencedSchema)
                {
                    if (!outgoingConnections.Contains(referencedSchema)) outgoingConnections.Add(referencedSchema);
                }
            }

            // Find incoming connections from all other schemas
            var allSchemaGuids = AssetDatabase.FindAssets("t:BaseSchema");
            foreach (var guid in allSchemaGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var currentSchema = AssetDatabase.LoadAssetAtPath<BaseSchema>(path);

                if (currentSchema == null || currentSchema == targetSchema) continue;

                var currentSo = new SerializedObject(currentSchema);
                var currentIterator = currentSo.GetIterator();
                while (currentIterator.NextVisible(true))
                {
                    if (currentIterator.propertyType == SerializedPropertyType.ObjectReference && currentIterator.objectReferenceValue == targetSchema)
                    {
                        if (!incomingConnections.Contains(currentSchema)) incomingConnections.Add(currentSchema);
                        break;
                    }
                }
            }

            RefreshUILists();
        }

        private void RefreshUILists()
        {
            if (outgoingFoldout != null) outgoingFoldout.text = $"Uses ({outgoingConnections.Count})";
            if (incomingFoldout != null) incomingFoldout.text = $"Referenced By ({incomingConnections.Count})";

            if (outgoingListView != null)
            {
                outgoingListView.itemsSource = outgoingConnections;
                outgoingListView.RefreshItems();
            }

            if (incomingListView != null)
            {
                incomingListView.itemsSource = incomingConnections;
                incomingListView.RefreshItems();
            }

            UpdateStatsLabel(statsLabel);
        }

        private void UpdateStatsLabel(Label stats)
        {
            if (stats == null) return;
            int outgoing = outgoingConnections?.Count ?? 0;
            int incoming = incomingConnections?.Count ?? 0;
            stats.text = $"Outgoing: {outgoing}   Incoming: {incoming}";
        }
    }
}