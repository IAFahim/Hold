
using Missions.Missions.Authoring.Schemas;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

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
                    SceneView.lastActiveSceneView.LookAt(stationSchema.position);
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

        private void OnSceneGUIHandler(SceneView view)
        {
            if (Selection.activeObject != target) return;
            var stationSchema = (LocationSchema)target;
            if (stationSchema == null) return;

#if ALINE
            Drawing.Draw.WireSphere(stationSchema.position, stationSchema.range, Color.cyan);
            Drawing.Draw.Label2D(stationSchema.position, stationSchema.name);
#endif
        }
    }
}