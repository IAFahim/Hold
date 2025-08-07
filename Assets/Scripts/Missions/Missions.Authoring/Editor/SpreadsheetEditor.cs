using System;
using System.Collections.Generic;
using System.Linq;
using Missions.Missions.Authoring.Scriptable;
using UnityEditor;
using UnityEngine;

namespace Missions.Missions.Authoring.Editor
{
    public class SpreadsheetEditor : EditorWindow
    {
        private Dictionary<Type, List<BaseSchema>> _schemas = new();
        private Dictionary<Type, List<(string path, SerializedPropertyType type)>> _properties = new();
        private Vector2 _scrollPosition;
        private Dictionary<Type, bool> _foldouts = new();
        private string _searchQuery = "";

        [MenuItem("Tools/Schema/Spreadsheet Editor")]
        public static void ShowWindow()
        {
            GetWindow<SpreadsheetEditor>("Spreadsheet Editor");
        }

        private void OnEnable()
        {
            LoadSchemas();
        }

        private void LoadSchemas()
        {
            _schemas.Clear();
            _properties.Clear();

            var guids = AssetDatabase.FindAssets("t:BaseSchema");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var schema = AssetDatabase.LoadAssetAtPath<BaseSchema>(path);
                var type = schema.GetType();

                if (!_schemas.ContainsKey(type))
                {
                    _schemas[type] = new List<BaseSchema>();
                    _properties[type] = new List<(string path, SerializedPropertyType type)>();
                    _foldouts[type] = true;

                    var so = new SerializedObject(schema);
                    var iterator = so.GetIterator();
                    iterator.NextVisible(true);
                    while (iterator.NextVisible(false))
                    {
                        _properties[type].Add((iterator.propertyPath, iterator.propertyType));
                    }
                }

                _schemas[type].Add(schema);
            }
        }

        private void OnGUI()
        {
            DrawToolbar();
            DrawSpreadsheet();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            _searchQuery = EditorGUILayout.TextField(_searchQuery, EditorStyles.toolbarSearchField);
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
            {
                LoadSchemas();
            }

            if (GUILayout.Button("Save Changes", EditorStyles.toolbarButton))
            {
                AssetDatabase.SaveAssets();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSpreadsheet()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            foreach (var (type, schemaList) in _schemas)
            {
                var filteredList = schemaList.Where(s =>
                    string.IsNullOrEmpty(_searchQuery) || s.name.ToLower().Contains(_searchQuery.ToLower())).ToList();
                if (filteredList.Count == 0) continue;

                _foldouts[type] = EditorGUILayout.Foldout(_foldouts[type], $"{type.Name} ({filteredList.Count})", true);
                if (!_foldouts[type]) continue;

                DrawSchemaType(type, filteredList);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawSchemaType(Type type, List<BaseSchema> schemaList)
        {
            var props = _properties[type];

            // Calculate widths for properties
            var widths = new List<float>();
            for (var j = 0; j < props.Count; j++)
            {
                var (path, propType) = props[j];
                float width = j == 0 ? 50 : 150;
                if (propType == SerializedPropertyType.ObjectReference)
                {
                    var fieldName = path;
                    var fieldInfo = type.GetField(fieldName,
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance);
                    if (fieldInfo != null && (fieldInfo.FieldType.IsClass ||
                                              fieldInfo.FieldType.IsSubclassOf(typeof(ScriptableObject))))
                    {
                        width = 250;
                    }
                }
                else if (propType == SerializedPropertyType.Generic)
                {
                    var sampleSo = new SerializedObject(schemaList[0]);
                    var sampleProp = sampleSo.FindProperty(path);
                    if (sampleProp.isArray)
                    {
                        width = 350;
                    }
                }

                widths.Add(width);
            }

            // Draw header
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Asset", EditorStyles.boldLabel, GUILayout.Width(200));
            for (var j = 0; j < props.Count; j++)
            {
                var (path, _) = props[j];
                var sampleSo = new SerializedObject(schemaList[0]);
                var sampleProp = sampleSo.FindProperty(path);
                EditorGUILayout.LabelField(sampleProp.displayName, EditorStyles.boldLabel, GUILayout.Width(widths[j]));
            }

            EditorGUILayout.LabelField("", GUILayout.Width(100)); // Space reserved for the button column
            EditorGUILayout.EndHorizontal();

            // Draw schema rows
            for (int i = 0; i < schemaList.Count; i++)
            {
                var schema = schemaList[i];
                var so = new SerializedObject(schema);

                GUI.backgroundColor = i % 2 == 0 ? Color.white : new Color(0.9f, 0.9f, 0.9f);
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(schema, typeof(BaseSchema), false, GUILayout.Width(200));
                EditorGUI.EndDisabledGroup();

                for (var j = 0; j < props.Count; j++)
                {
                    var (path, _) = props[j];
                    var serializedProperty = so.FindProperty(path);
                    EditorGUILayout.PropertyField(serializedProperty, GUIContent.none, GUILayout.Width(widths[j]));
                }

                // Add flexible space to push the button to the right
                GUILayout.FlexibleSpace();

                // Right-aligned "Create New" button
                if (GUILayout.Button("Create New", GUILayout.Width(100)))
                {
                    CreateNewSchema(schema);
                }

                EditorGUILayout.EndHorizontal();
                so.ApplyModifiedProperties();
            }

            GUI.backgroundColor = Color.white;
        }

        private void CreateNewSchema(BaseSchema originalSchema)
        {
            var type = originalSchema.GetType();
            var schemasOfType = _schemas[type];

            // Find max ID
            int maxId = 0;
            var idPropName = _properties[type][0].path; // Assuming first property is ID
            foreach (var schema in schemasOfType)
            {
                var so = new SerializedObject(schema);
                var idProp = so.FindProperty(idPropName);
                if (idProp.propertyType == SerializedPropertyType.Integer)
                {
                    maxId = Mathf.Max(maxId, idProp.intValue);
                }
            }

            int newId = maxId + 1;

            // Get the folder of the original schema
            string originalPath = AssetDatabase.GetAssetPath(originalSchema);
            string folder = System.IO.Path.GetDirectoryName(originalPath);

            // Generate a unique asset path
            string newPath = AssetDatabase.GenerateUniqueAssetPath(folder + "/New Schema.asset");

            // Copy the asset
            AssetDatabase.CopyAsset(originalPath, newPath);

            // Load the new asset
            var newSchema = AssetDatabase.LoadAssetAtPath<BaseSchema>(newPath);

            // Set the new ID
            var newSo = new SerializedObject(newSchema);
            var newIdProp = newSo.FindProperty(idPropName);
            if (newIdProp.propertyType == SerializedPropertyType.Integer)
            {
                newIdProp.intValue = newId;
                newSo.ApplyModifiedProperties();
            }

            // Save assets
            AssetDatabase.SaveAssets();

            // Refresh the editor
            LoadSchemas();
        }
    }
}