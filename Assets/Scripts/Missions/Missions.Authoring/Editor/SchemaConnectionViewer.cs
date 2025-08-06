using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Data;

namespace Missions.Missions.Authoring.Editor
{
    public class SchemaConnectionViewer : EditorWindow
    {
        // Data
        private List<BaseSchema> allSchemas = new List<BaseSchema>();
        private Dictionary<BaseSchema, List<BaseSchema>> outgoingConnections = new Dictionary<BaseSchema, List<BaseSchema>>();
        private Dictionary<BaseSchema, List<BaseSchema>> incomingConnections = new Dictionary<BaseSchema, List<BaseSchema>>();

        // UI State
        private Vector2 listScrollPos;
        private Vector2 detailsScrollPos;
        private BaseSchema selectedSchema;
        private string searchQuery = "";

        [MenuItem("Window/Hold/Schema Connection Viewer")]
        public static void ShowWindow()
        {
            var window = GetWindow<SchemaConnectionViewer>("Schema Connections");
            window.minSize = new Vector2(400, 300);
        }

        private void OnEnable()
        {
            RefreshData();
        }

        private void OnGUI()
        {
            DrawToolbar();
            DrawPanels();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            // Refresh Button
            if (GUILayout.Button(new GUIContent(" Refresh", EditorGUIUtility.IconContent("d_Refresh").image), EditorStyles.toolbarButton))
            {
                RefreshData();
            }

            GUILayout.FlexibleSpace();

            // Search Field
            searchQuery = EditorGUILayout.TextField(searchQuery, EditorStyles.toolbarSearchField, GUILayout.Width(200));
            if (GUILayout.Button("", EditorStyles.toolbarButton))
            {
                searchQuery = "";
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

        private void DrawSchemaListPanel()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(position.width * 0.4f));
            
            EditorGUILayout.LabelField("All Schemas", EditorStyles.boldLabel);
            listScrollPos = EditorGUILayout.BeginScrollView(listScrollPos);

            var filteredSchemas = string.IsNullOrEmpty(searchQuery)
                ? allSchemas
                : allSchemas.Where(s => s.name.ToLower().Contains(searchQuery.ToLower())).ToList();

            foreach (var schema in filteredSchemas)
            {
                var style = selectedSchema == schema ? new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold, normal = { textColor = Color.cyan } } : EditorStyles.label;
                if (GUILayout.Button(schema.name, style))
                {
                    selectedSchema = schema;
                    GUI.FocusControl(null); // Deselect search bar
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawDetailsPanel()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandWidth(true));
            detailsScrollPos = EditorGUILayout.BeginScrollView(detailsScrollPos);

            if (selectedSchema == null)
            {
                EditorGUILayout.HelpBox("Select a schema from the list on the left to view its connections.", MessageType.Info);
            }
            else
            {
                DrawDetailContent();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        
        private void DrawDetailContent()
        {
            // --- Header ---
            EditorGUILayout.LabelField(selectedSchema.name, EditorStyles.largeLabel);
            if (GUILayout.Button("Ping Asset", GUILayout.Width(100)))
            {
                EditorGUIUtility.PingObject(selectedSchema);
            }
            
            EditorGUILayout.Space();

            // --- Outgoing Connections ---
            EditorGUILayout.LabelField("Uses (Outgoing Connections)", EditorStyles.boldLabel);
            if (outgoingConnections.TryGetValue(selectedSchema, out var uses) && uses.Any())
            {
                foreach (var usedSchema in uses)
                {
                    EditorGUILayout.ObjectField(usedSchema.name, usedSchema, typeof(BaseSchema), false);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("This schema does not reference any other schemas.", MessageType.None);
            }

            EditorGUILayout.Space();

            // --- Incoming Connections ---
            EditorGUILayout.LabelField("Referenced By (Incoming Connections)", EditorStyles.boldLabel);
            if (incomingConnections.TryGetValue(selectedSchema, out var refs) && refs.Any())
            {
                foreach (var referencingSchema in refs)
                {
                    EditorGUILayout.ObjectField(referencingSchema.name, referencingSchema, typeof(BaseSchema), false);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("This schema is not referenced by any other schemas.", MessageType.None);
            }
        }

        private void RefreshData()
        {
            allSchemas.Clear();
            outgoingConnections.Clear();
            incomingConnections.Clear();

            var guids = AssetDatabase.FindAssets("t:BaseSchema");
            allSchemas = guids
                .Select(guid => AssetDatabase.LoadAssetAtPath<BaseSchema>(AssetDatabase.GUIDToAssetPath(guid)))
                .OrderBy(s => s.name)
                .ToList();

            foreach (var schema in allSchemas)
            {
                // Initialize dictionaries
                if (!outgoingConnections.ContainsKey(schema)) outgoingConnections[schema] = new List<BaseSchema>();
                if (!incomingConnections.ContainsKey(schema)) incomingConnections[schema] = new List<BaseSchema>();

                // Find outgoing connections
                var so = new SerializedObject(schema);
                var iterator = so.GetIterator();
                while (iterator.NextVisible(true))
                {
                    if (iterator.propertyType == SerializedPropertyType.ObjectReference && iterator.objectReferenceValue is BaseSchema referencedSchema)
                    {
                        outgoingConnections[schema].Add(referencedSchema);

                        // While we're here, add the incoming connection for the other schema
                        if (!incomingConnections.ContainsKey(referencedSchema)) incomingConnections[referencedSchema] = new List<BaseSchema>();
                        if (!incomingConnections[referencedSchema].Contains(schema))
                        {
                            incomingConnections[referencedSchema].Add(schema);
                        }
                    }
                }
            }
            Repaint();
        }
    }
}