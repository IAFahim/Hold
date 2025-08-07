using Missions.Missions.Authoring.Schemas;
using UnityEditor;
using UnityEngine;

namespace Missions.Missions.Authoring.Editor
{
    [CustomEditor(typeof(StationSchema))]
    public class StationSchemaEditor : BaseSchemaEditor
    {
        public override void OnInspectorGUI()
        {
            // Call the base implementation to get all the BaseSchema functionality
            base.OnInspectorGUI();

            // Add StationSchema-specific functionality
            var stationSchema = (StationSchema)target;

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Station Tools", EditorStyles.boldLabel);

            if (GUILayout.Button("Focus Camera"))
            {
                if (SceneView.lastActiveSceneView != null)
                {
                    SceneView.lastActiveSceneView.LookAt(stationSchema.position);
                }
                else
                {
                    Debug.Log("No scene view open to focus.");
                }
            }
        }
    }
}