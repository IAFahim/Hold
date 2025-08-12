using System;
using Missions.Missions.Authoring.Schemas;

namespace Missions.Missions.Authoring.Editor.Graph
{
    [Serializable] internal class MissionSchemaNode : SchemaNode<MissionSchema> {}
    [Serializable] internal class GoalSchemaNode : SchemaNode<GoalSchema> {}
    [Serializable] internal class LocationSchemaNode : SchemaNode<LocationSchema> {}
    [Serializable] internal class RangeFloatSchemaNode : SchemaNode<RangeFloatSchema> {}
    [Serializable] internal class RangeIntSchemaNode : SchemaNode<RangeIntSchema> {}
    [Serializable] internal class RewardSchemaNode : SchemaNode<RewardSchema> {}
    [Serializable] internal class TimeSchemaNode : SchemaNode<TimeSchema> {}
    [Serializable] internal class DataContainerSchemaNode : SchemaNode<DataContainerSchema> {}
    [Serializable] internal class NameSchemaNode : SchemaNode<NameSchema> {}
    [Serializable] internal class DescriptionSchemaNode : SchemaNode<DescriptionSchema> {}
    [Serializable] internal class ItemSchemaNode : SchemaNode<ItemSchema> {}
}
