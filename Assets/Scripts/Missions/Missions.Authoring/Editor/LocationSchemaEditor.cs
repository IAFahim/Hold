
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
            var root = base.CreateInspectorGUI();

            var separator = new VisualElement { style = { height = 8 } };
            root.Add(separator);

            var box = new HelpBox("", HelpBoxMessageType.None);
            box.style.marginTop = 4;
            box.style.marginBottom = 4;

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
            // IMGUI fallback to base
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