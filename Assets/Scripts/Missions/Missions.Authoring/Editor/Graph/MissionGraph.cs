using System;
using Unity.GraphToolkit.Editor;

namespace Missions.Missions.Authoring.Editor.Graph
{
    [Graph(AssetExtension)]
    [Serializable]
    internal class MissionGraph : Unity.GraphToolkit.Editor.Graph
    {
        internal const string AssetExtension = "missions"; // *.missions

        public override void OnGraphChanged(GraphLogger logger)
        {
            // Optional: add validations here (e.g., missing sinks)
        }

        public static T ResolvePortValue<T>(IPort port)
        {
            var source = port.firstConnectedPort;
            switch (source?.GetNode())
            {
                case IConstantNode cn when cn.TryGetValue(out T c):
                    return c;
                case IVariableNode vn when vn.variable.TryGetDefaultValue(out T v):
                    return v;
                case ISchemaEvaluatorNode<T> eval:
                    return eval.EvaluateSchemaPort(source);
                case null:
                    port.TryGetValue(out T embedded);
                    return embedded;
            }
            return default;
        }
    }
}
