using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Missions.Missions.Authoring.Scriptable;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Missions.Missions.Authoring.Editor
{
    public class SpreadsheetEditor : EditorWindow
    {
        private Dictionary<Type, List<BaseSchema>> _schemas = new();
        private Dictionary<Type, List<(string path, SerializedPropertyType type)>> _properties = new();
        private Dictionary<Type, bool> _foldouts = new();
        private string _searchQuery = "";

        // UI Toolkit
        private ToolbarSearchField _searchField;
        private ScrollView _mainScroll;

        [MenuItem("Tools/Schema/Spreadsheet Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<SpreadsheetEditor>("Spreadsheet Editor");
            window.minSize = new Vector2(800, 400);
            window.titleContent = new GUIContent("Spreadsheet Editor",
                EditorGUIUtility.IconContent("d_UnityEditor.HierarchyWindow").image);
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
                if (schema == null) continue;
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

            RebuildUISections();
        }

        public VisualTreeAsset uxml;
        public StyleSheet uss;

        public void CreateGUI()
        {
            // Load UXML/USS
            // uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Missions/Missions.Authoring/Editor/UI/SpreadsheetEditor.uxml");
            // uss = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Missions/Missions.Authoring/Editor/UI/MissionsEditor.uss");

            rootVisualElement.Clear();
            if (uss != null) rootVisualElement.styleSheets.Add(uss);
            if (uxml != null) uxml.CloneTree(rootVisualElement);

            // Theme class
            ApplyThemeClass(rootVisualElement);

            // Wire toolbar
            _searchField = rootVisualElement.Q<ToolbarSearchField>("searchField");
            _mainScroll = rootVisualElement.Q<ScrollView>("mainScroll");
            var refreshBtn = rootVisualElement.Q<ToolbarButton>("refreshButton");
            var saveBtn = rootVisualElement.Q<ToolbarButton>("saveButton");

            if (_searchField != null)
            {
                _searchField.value = _searchQuery;
                _searchField.RegisterValueChangedCallback(evt =>
                {
                    _searchQuery = evt.newValue;
                    RebuildUISections();
                });
            }

            if (refreshBtn != null) refreshBtn.clicked += () => LoadSchemas();
            if (saveBtn != null) saveBtn.clicked += () => AssetDatabase.SaveAssets();

            RebuildUISections();
        }

        private void ApplyThemeClass(VisualElement root)
        {
            bool dark = EditorGUIUtility.isProSkin;
            root.RemoveFromClassList("theme--dark");
            root.RemoveFromClassList("theme--light");
            root.AddToClassList(dark ? "theme--dark" : "theme--light");
        }

        private void RebuildUISections()
        {
            if (_mainScroll == null) return;
            _mainScroll.Clear();

            var schemasList = _schemas.ToList();
            for (int i = schemasList.Count - 1; i >= 0; i--)
            {
                var (type, schemaList) = schemasList[i];
                var filteredList = schemaList.Where(s =>
                    string.IsNullOrEmpty(_searchQuery) || s.name.ToLower().Contains(_searchQuery.ToLower())).ToList();
                if (filteredList.Count == 0) continue;

                var foldout = new Foldout
                    { text = $"{type.Name} ({filteredList.Count})", value = _foldouts.GetValueOrDefault(type, true) };
                foldout.RegisterValueChangedCallback(evt => _foldouts[type] = evt.newValue);

                var section = BuildTypeSection(type, filteredList);
                foldout.Add(section);
                _mainScroll.Add(foldout);
            }
        }

        private VisualElement BuildTypeSection(Type type, List<BaseSchema> schemaList)
        {
            var container = new VisualElement { style = { marginBottom = 6 } };
            var props = _properties[type];

            // Calculate widths similar to IMGUI version
            var widths = new List<float>();
            for (var j = 0; j < props.Count; j++)
            {
                var (_, propType) = props[j];
                float width = j == 0 ? 50 : 150;
                if (propType == SerializedPropertyType.ObjectReference) width = 250;
                else if (propType == SerializedPropertyType.Generic)
                {
                    var sampleSo = new SerializedObject(schemaList[0]);
                    var sampleProp = sampleSo.FindProperty(props[j].path);
                    if (sampleProp is { isArray: true }) width = 350;
                }

                widths.Add(width);
            }

            // Header row
            var header = new VisualElement
                { style = { flexDirection = FlexDirection.Row, marginBottom = 2, paddingLeft = 10 } };
            var holdFirstHeader = false;
            for (var j = 0; j < props.Count; j++)
            {
                var sampleSo = new SerializedObject(schemaList[0]);
                var sampleProp = sampleSo.FindProperty(props[j].path);
                var label = new Label(sampleProp != null ? sampleProp.displayName : props[j].path)
                {
                    style =
                    {
                        width = widths[j], unityFontStyleAndWeight = FontStyle.Bold
                    }
                };
                header.Add(label);
                if (!holdFirstHeader)
                {
                    var assetHeader = new Label("Asset")
                    {
                        style =
                        {
                            width = 200, unityFontStyleAndWeight = FontStyle.Bold
                        }
                    };
                    header.Add(assetHeader);
                    holdFirstHeader = true;
                }
            }

            header.Add(new Label("") { style = { width = 100 } });
            container.Add(header);

            // Rows
            for (int i = 0; i < schemaList.Count; i++) CreateRow(i, schemaList, props, widths, container);
            return container;
        }

        private void CreateRow(
            int rowIndex,
            List<BaseSchema> schemaList,
            List<(string path, SerializedPropertyType type)> props,
            List<float> widths, VisualElement container
        )
        {
            var schema = schemaList[rowIndex];
            var so = new SerializedObject(schema);

            var row = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row, paddingRight = 4
                }
            };
            row.EnableInClassList("schema-row--odd", rowIndex % 2 == 1);
            row.EnableInClassList("schema-row--even", rowIndex % 2 == 0);

            AddProperty(props, widths, 0, so, row);
            AddMainScriptableObject(schema, row);
            for (var i = 1; i < props.Count; i++) AddProperty(props, widths, i, so, row);
            var newBtn = new Button(() => { CreateNewSchema(schema); })
            {
                text = "New",
                style =
                {
                    width = 100,
                    marginLeft = 6
                }
            };
            row.Add(newBtn);

            // Bind once per-row to ensure all PropertyFields connect
            row.Bind(so);
            container.Add(row);
        }

        private static void AddMainScriptableObject(BaseSchema schema, VisualElement row)
        {
            var objField = new ObjectField
                { objectType = typeof(BaseSchema), value = schema, allowSceneObjects = false };
            objField.SetEnabled(false);
            objField.style.width = 200;
            row.Add(objField);
        }

        private static void AddProperty(List<(string path, SerializedPropertyType type)> props, List<float> widths,
            int j, SerializedObject so, VisualElement row)
        {
            var propPath = props[j].path;
            var serializedProperty = so.FindProperty(propPath);
            var field = serializedProperty != null
                ? new PropertyField(serializedProperty, "")
                : new PropertyField();
            field.style.width = widths[j];
            row.Add(field);
        }

        private void CreateNewSchema(BaseSchema originalSchema)
        {
            string originalPath = AssetDatabase.GetAssetPath(originalSchema);
            string folder = System.IO.Path.GetDirectoryName(originalPath);

            var assetName = $"/{IncrementLastNumber(originalSchema.name)}.asset";
            string newPath = AssetDatabase.GenerateUniqueAssetPath(folder + assetName);

            AssetDatabase.CopyAsset(originalPath, newPath);
            AssetDatabase.SaveAssets();
            LoadSchemas();
        }

        private void OnGUI()
        {
            // Fallback to IMGUI if UI Toolkit not initialized
            if (rootVisualElement is { childCount: > 0 }) return;

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
            var _scrollPosition = Vector2.zero;
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            var schemasList = _schemas.ToList();
            for (int i = schemasList.Count - 1; i >= 0; i--)
            {
                var (type, schemaList) = schemasList[i];
                var filteredList = schemaList.Where(s =>
                    string.IsNullOrEmpty(_searchQuery) || s.name.ToLower().Contains(_searchQuery.ToLower())).ToList();
                if (filteredList.Count == 0) continue;

                _foldouts[type] = EditorGUILayout.Foldout(_foldouts[type], $"{type.Name} ({filteredList.Count})", true);
                if (!_foldouts[type]) continue;

                // Fallback minimal: just list assets
                foreach (var s in filteredList)
                {
                    EditorGUILayout.ObjectField(s, typeof(BaseSchema), false);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        static string IncrementLastNumber(string input)
        {
            Match match = Regex.Match(input, @"\d+", RegexOptions.RightToLeft);
            if (match.Success)
            {
                int number = int.Parse(match.Value);
                int index = match.Index;
                int incrementedNumber = number + 1;
                string result = input.Remove(index, match.Length).Insert(index, incrementedNumber.ToString());
                return result;
            }

            return input;
        }
    }
}