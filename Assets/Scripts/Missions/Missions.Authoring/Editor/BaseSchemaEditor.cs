using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Data;

namespace Missions.Missions.Authoring.Editor
{
    [CustomEditor(typeof(BaseSchema), true)]
    public class BaseSchemaEditor : UnityEditor.Editor
    {
        private List<BaseSchema> referencers;
        private bool showReferencers = true;

        void OnEnable()
        {
            FindReferencers();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Connections", EditorStyles.boldLabel);

            showReferencers = EditorGUILayout.Foldout(showReferencers, $"Referenced By ({referencers.Count})", true);
            if (showReferencers)
            {
                if (referencers.Any())
                {
                    EditorGUI.indentLevel++;
                    foreach (var referencer in referencers)
                    {
                        EditorGUILayout.ObjectField(referencer.name, referencer, typeof(BaseSchema), false);
                    }
                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("Not referenced by any other schema.");
                    EditorGUI.indentLevel--;
                }
            }
            
            if (GUILayout.Button("Refresh Connections"))
            {
                FindReferencers();
            }
        }

        private void FindReferencers()
        {
            referencers = new List<BaseSchema>();
            var targetSchema = target as BaseSchema;
            if (targetSchema == null) return;

            var allSchemaGuids = AssetDatabase.FindAssets("t:BaseSchema");
            foreach (var guid in allSchemaGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var currentSchema = AssetDatabase.LoadAssetAtPath<BaseSchema>(path);

                if (currentSchema == null || currentSchema == targetSchema) continue;

                var serializedObject = new SerializedObject(currentSchema);
                var iterator = serializedObject.GetIterator();
                while (iterator.NextVisible(true))
                {
                    if (iterator.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        if (iterator.objectReferenceValue == targetSchema)
                        {
                            referencers.Add(currentSchema);
                            break;
                        }
                    }
                }
            }
        }
    }
}
