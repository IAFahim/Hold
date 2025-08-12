using Unity.GraphToolkit.Editor;

namespace Missions.Missions.Authoring.Editor.Graph
{
    internal interface ISchemaEvaluatorNode<out TSchema>
    {
        TSchema EvaluateSchemaPort(IPort port);
    }
}
