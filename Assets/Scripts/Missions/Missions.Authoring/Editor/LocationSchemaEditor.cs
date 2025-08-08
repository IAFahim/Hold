
using Missions.Missions.Authoring.Schemas;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Unity.Mathematics;

namespace Missions.Missions.Authoring.Editor
{
    [CustomEditor(typeof(LocationSchema))]
    public class LocationSchemaEditor : BaseSchemaEditor
    {
        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUIHandler;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUIHandler;
        }

        public override VisualElement CreateInspectorGUI()
        {
            // Build base inspector with fields and connections
            var root = base.CreateInspectorGUI();

            // Append Location Tools
            var box = new HelpBox("", HelpBoxMessageType.None);
            box.style.marginTop = 6;

            var header = new Label("Location Tools")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold }
            };
            box.Add(header);

            var toolbar = new Toolbar();
            var focusBtn = new ToolbarButton(() =>
            {
                var stationSchema = (LocationSchema)target;
                if (SceneView.lastActiveSceneView != null)
                {
                    SceneView.lastActiveSceneView.LookAt((Vector3)stationSchema.position);
                }
                else
                {
                    Debug.Log("No scene view open to focus.");
                }
            }) { text = "Focus Camera" };
            toolbar.Add(focusBtn);
            box.Add(toolbar);

            root.Add(box);
            return root;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        private void OnSceneGUIHandler(SceneView view)
        {
            if (Selection.activeObject != target) return;
            var stationSchema = (LocationSchema)target;
            if (stationSchema == null) return;

            // Position handle
            EditorGUI.BeginChangeCheck();
            Vector3 pos = (Vector3)stationSchema.position;
            Handles.color = new Color(0f, 0.7f, 1f, 0.9f);
            pos = Handles.PositionHandle(pos, Quaternion.identity);

            // Radius handle for range
            float radius = stationSchema.range;
            Handles.color = new Color(0f, 0.6f, 0.9f, 0.3f);
            radius = Handles.RadiusHandle(Quaternion.identity, pos, radius);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(stationSchema, "Edit Location");
                stationSchema.position = (float3)pos;
                stationSchema.range = Mathf.Max(0f, radius);
                EditorUtility.SetDirty(stationSchema);
            }

#if ALINE
            Drawing.Draw.WireSphere(stationSchema.position, stationSchema.range, Color.cyan);
            Drawing.Draw.Label2D(stationSchema.position, stationSchema.name);
#else
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
            Handles.color = new Color(0f, 0.7f, 1f, 0.8f);
            Handles.SphereHandleCap(0, (Vector3)stationSchema.position, Quaternion.identity, stationSchema.range * 2f, EventType.Repaint);
            Handles.color = Color.white;
            Handles.Label((Vector3)stationSchema.position + Vector3.up * (stationSchema.range + 0.2f), stationSchema.name);
#endif
        }
    }
}