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
        private Dictionary<Type, List<SerializedProperty>> _properties = new();
        private Vector2 _scrollPosition;
        private Dictionary<Type, bool> _foldouts = new();
        private string _searchQuery = "";

        [MenuItem("Tools/Base Schema/Spreadsheet Editor")]
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
                    _properties[type] = new List<SerializedProperty>();
                    _foldouts[type] = true;

                    var so = new SerializedObject(schema);
                    var iterator = so.GetIterator();
                    iterator.NextVisible(true);
                    while (iterator.NextVisible(false))
                    {
                        _properties[type].Add(so.FindProperty(iterator.name));
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
                var filteredList = schemaList.Where(s => string.IsNullOrEmpty(_searchQuery) || s.name.ToLower().Contains(_searchQuery.ToLower())).ToList();
                if(filteredList.Count == 0) continue;

                _foldouts[type] = EditorGUILayout.Foldout(_foldouts[type], $"{type.Name} ({filteredList.Count})", true);
                if (!_foldouts[type]) continue;

                DrawSchemaType(type, filteredList);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawSchemaType(Type type, List<BaseSchema> schemaList)
        {
            var props = _properties[type];

            // Header
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Object", EditorStyles.boldLabel, GUILayout.Width(200));
            foreach (var prop in props)
            {
                EditorGUILayout.LabelField(prop.displayName, EditorStyles.boldLabel, GUILayout.Width(150));
            }
            EditorGUILayout.EndHorizontal();

            // Rows
            for (int i = 0; i < schemaList.Count; i++)
            {
                var schema = schemaList[i];
                var so = new SerializedObject(schema);
                
                GUI.backgroundColor = i % 2 == 0 ? Color.white : new Color(0.9f, 0.9f, 0.9f);
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.ObjectField(schema, typeof(BaseSchema), false, GUILayout.Width(200));

                foreach (var prop in props)
                {
                    var serializedProperty = so.FindProperty(prop.name);
                    EditorGUILayout.PropertyField(serializedProperty, GUIContent.none, GUILayout.Width(150));
                }
                EditorGUILayout.EndHorizontal();
                so.ApplyModifiedProperties();
            }
            GUI.backgroundColor = Color.white;
        }
    }
}