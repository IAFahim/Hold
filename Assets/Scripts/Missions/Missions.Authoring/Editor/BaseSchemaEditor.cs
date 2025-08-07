using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Missions.Missions.Authoring.Scriptable;

namespace Missions.Missions.Authoring.Editor
{
    [CustomEditor(typeof(BaseSchema), true)]
    public class BaseSchemaEditor : UnityEditor.Editor
    {
        private List<BaseSchema> outgoingConnections;
        private List<BaseSchema> incomingConnections;

        private bool showOutgoing = true;
        private bool showIncoming = true;

        void OnEnable()
        {
            FindConnections();
        }

        public override void OnInspectorGUI()
        {
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
        }
    }
}