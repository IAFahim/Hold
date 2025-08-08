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

        void OnEnable()
        {
            FindConnections();
        }

        public override VisualElement CreateInspectorGUI()
        {
            // Root
            root = new VisualElement
            {
                style =
                {
                    marginLeft = 4,
                    marginRight = 4,
                    marginTop = 4,
                    marginBottom = 6
                }
            };

            // Default inspector (auto-generates from SerializedObject)
            var defaultInspector = BuildDefaultInspector(serializedObject);
            root.Add(defaultInspector);

            // Spacer
            root.Add(new VisualElement { style = { height = 8 } });

            // Header and toolbar
            var header = new Label("Schema Connections")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginTop = 4,
                    marginBottom = 2
                }
            };
            root.Add(header);

            var toolbar = new Toolbar();
            refreshButton = new ToolbarButton(() =>
            {
                FindConnections();
                RefreshUILists();
            }) { text = "Refresh Connections" };
            toolbar.Add(refreshButton);
            toolbar.Add(new ToolbarSpacer());
            var stats = new Label();
            stats.AddToClassList("mini-label");
            toolbar.Add(stats);
            root.Add(toolbar);

            // Container box
            var box = new HelpBox(string.Empty, HelpBoxMessageType.None);
            box.style.marginTop = 4;
            box.style.marginBottom = 4;
            box.style.paddingLeft = 6;
            box.style.paddingRight = 6;
            root.Add(box);

            // Outgoing foldout
            outgoingFoldout = new Foldout { text = $"Uses ({outgoingConnections.Count})", value = showOutgoing };
            outgoingFoldout.RegisterValueChangedCallback(evt => { showOutgoing = evt.newValue; });
            box.Add(outgoingFoldout);

            // Outgoing list
            outgoingListView = CreateConnectionsListView(outgoingConnections);
            outgoingFoldout.Add(outgoingListView);

            // Incoming foldout
            incomingFoldout = new Foldout { text = $"Referenced By ({incomingConnections.Count})", value = showIncoming };
            incomingFoldout.RegisterValueChangedCallback(evt => { showIncoming = evt.newValue; });
            box.Add(incomingFoldout);

            // Incoming list
            incomingListView = CreateConnectionsListView(incomingConnections);
            incomingFoldout.Add(incomingListView);

            // Initial stats
            UpdateStatsLabel(stats);

            return root;
        }

        private VisualElement BuildDefaultInspector(SerializedObject so)
        {
            var container = new VisualElement();

            // Iterate over all visible properties and add PropertyFields
            var iterator = so.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                if (iterator.name == "m_Script")
                {
                    // Show script field but disabled
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

            // Bind at the end so all children use the same SerializedObject
            container.Bind(so);
            return container;
        }

        private ListView CreateConnectionsListView(List<BaseSchema> source)
        {
            var listView = new ListView
            {
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                showFoldoutHeader = false,
                reorderable = false,
                selectionType = SelectionType.Single,
                itemsSource = source,
                style = { marginLeft = 12, marginTop = 2 }
            };

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

                var pingButton = new Button { text = "Ping" };
                pingButton.style.marginLeft = 6;
                pingButton.style.width = 50;

                row.Add(objField);
                row.Add(pingButton);
                return row;
            };

            listView.bindItem = (element, i) =>
            {
                var schema = source.ElementAtOrDefault(i);
                var objField = element.Q<ObjectField>();
                var pingButton = element.Q<Button>();

                objField.value = schema;
                pingButton.clicked -= null;
                pingButton.clicked += () =>
                {
                    if (schema != null)
                    {
                        EditorGUIUtility.PingObject(schema);
                        Selection.activeObject = schema;
                    }
                };

                element.RegisterCallback<MouseUpEvent>(_ =>
                {
                    if (schema != null)
                    {
                        Selection.activeObject = schema;
                    }
                });
            };

            // Empty state
            listView.itemsAdded += _ => { UpdateFoldoutEmptyState(listView); };
            listView.itemsRemoved += _ => { UpdateFoldoutEmptyState(listView); };
            UpdateFoldoutEmptyState(listView);

            return listView;
        }

        private void UpdateFoldoutEmptyState(ListView listView)
        {
            if (listView.itemsSource is List<BaseSchema> src && (src == null || src.Count == 0))
            {
                if (listView.parent is Foldout foldout)
                {
                    // Show small help box when empty
                    var empty = new HelpBox((foldout == outgoingFoldout)
                            ? "This schema does not reference any other schemas."
                            : "This schema is not referenced by any other schemas.",
                        HelpBoxMessageType.None)
                    {
                        style = { marginLeft = 12 }
                    };
                    // Ensure only once
                    if (!foldout.Contains(empty))
                    {
                        foldout.Add(empty);
                    }
                }
            }
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