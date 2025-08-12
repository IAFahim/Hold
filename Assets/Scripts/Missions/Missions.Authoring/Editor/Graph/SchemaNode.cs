using System;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace Missions.Missions.Authoring.Editor.Graph
{
    [Serializable]
    internal class SchemaNode<TSchema> : Node, ISchemaEvaluatorNode<TSchema> where TSchema : ScriptableObject
    {
        internal const string InName = "Asset";
        internal const string OutName = "Out";

        protected override void OnDefinePorts(IPortDefinitionContext ctx)
        {
            ctx.AddInputPort<TSchema>(InName).Build();
            ctx.AddOutputPort<TSchema>(OutName).Build();
        }

        public TSchema EvaluateSchemaPort(IPort port)
        {
            var inPort = GetInputPortByName(InName);
            return MissionGraph.ResolvePortValue<TSchema>(inPort);
        }
    }
}
