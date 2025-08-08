
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

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        private void OnSceneGUI()
        {
#if ALINE
            var stationSchema = (LocationSchema)target;
            Drawing.Draw.WireSphere(stationSchema.position, stationSchema.range, Color.antiqueWhite);
            Drawing.Draw.Label2D(stationSchema.position, stationSchema.name);
#endif
        }
    }
}