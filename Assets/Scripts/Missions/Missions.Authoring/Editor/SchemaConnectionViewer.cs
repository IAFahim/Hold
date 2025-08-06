using System.Collections.Generic;
using System.Linq;
using BovineLabs.Core.Settings;
using Data;
using UnityEditor;
using UnityEngine;

namespace Missions.Missions.Authoring.Editor
{
    public class SchemaConnectionViewer : EditorWindow
    {
        private List<BaseSchema> allSchemas = new List<BaseSchema>();
        private List<ISettings> allSettings = new List<ISettings>();
        private Dictionary<BaseSchema, List<BaseSchema>> connections = new Dictionary<BaseSchema, List<BaseSchema>>();
        private Vector2 scrollPosition;

        [MenuItem("Window/Schema Connection Viewer")]
        public static void ShowWindow()
        {
            GetWindow<SchemaConnectionViewer>("Schema Connection Viewer");
        }

        private void OnEnable()
        {
            RefreshData();
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Refresh"))
            {
                RefreshData();
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (var schema in allSchemas)
            {
                EditorGUILayout.ObjectField(schema, typeof(BaseSchema), false);

                if (connections.TryGetValue(schema, out var connectedSchemas))
                {
                    foreach (var connectedSchema in connectedSchemas)
                    {
                        EditorGUILayout.LabelField("    -> " + connectedSchema.name);
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void RefreshData()
        {
            allSchemas.Clear();
            allSettings.Clear();
            connections.Clear();

            var schemaGuids = AssetDatabase.FindAssets("t:BaseSchema");
            foreach (var guid in schemaGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                allSchemas.Add(AssetDatabase.LoadAssetAtPath<BaseSchema>(path));
            }

            var settingsGuids = AssetDatabase.FindAssets("t:ScriptableObject");
            foreach (var guid in settingsGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (asset is ISettings settings)
                {
                    allSettings.Add(settings);
                }
            }

            foreach (var schema in allSchemas)
            {
                connections[schema] = new List<BaseSchema>();
                var serializedObject = new SerializedObject(schema);
                var iterator = serializedObject.GetIterator();
                while (iterator.NextVisible(true))
                {
                    if (iterator.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        if (iterator.objectReferenceValue is BaseSchema referencedSchema)
                        {
                            connections[schema].Add(referencedSchema);
                        }
                    }
                }
            }

            foreach (var setting in allSettings)
            {
                var serializedObject = new SerializedObject(setting as ScriptableObject);
                var iterator = serializedObject.GetIterator();
                while (iterator.NextVisible(true))
                {
                    if (iterator.isArray && iterator.propertyType != SerializedPropertyType.String)
                    {
                        for (int i = 0; i < iterator.arraySize; i++)
                        {
                            var element = iterator.GetArrayElementAtIndex(i);
                            if (element.propertyType == SerializedPropertyType.ObjectReference)
                            {
                                if (element.objectReferenceValue is BaseSchema referencedSchema)
                                {
                                    if (!connections.ContainsKey(referencedSchema))
                                    {
                                        connections[referencedSchema] = new List<BaseSchema>();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
