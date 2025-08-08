// using System;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.Mathematics;
// using UnityEngine;
//
// namespace Data
// {
//     public class StationAuthoring : MonoBehaviour
//     {
//         public StationSchema[] station;
//
//         public class StationBaker : Baker<StationAuthoring>
//         {
//             public override void Bake(StationAuthoring authoring)
//             {
//                 var entity = GetEntity(TransformUsageFlags.None);
//                 AddComponent(entity, new Station
//                 {
//                     id = (ushort)authoring.station.ID,
//                     nameId = (ushort)authoring.station.nameSchema.ID,
//                     position = authoring.station.position
//                 });
//             }
//         }
//     }
//
//     public class NameAuthoring : MonoBehaviour
//     {
//         public NameSchema[] names;
//
//         public class NameBaker : Baker<NameAuthoring>
//         {
//             public override void Bake(NameAuthoring authoring)
//             {
//                 var entity = GetEntity(TransformUsageFlags.None);
//                 AddComponent(entity, new Name
//                 {
//                     id = (ushort)authoring.name.ID,
//                     name = authoring.name.name
//                 });
//             }
//         }
//     }
//
//     public class GoalAuthoring : MonoBehaviour
//     {
//         public GoalSchema goal;
//
//         public class GoalBaker : Baker<GoalAuthoring>
//         {
//             public override void Bake(GoalAuthoring authoring)
//             {
//                 var entity = GetEntity(TransformUsageFlags.None);
//                 AddComponent(entity, new Goal
//                 {
//                     id = (ushort)authoring.goal.ID,
//                     targetType = authoring.goal.targetType,
//                     rangeType = authoring.goal.rangeSchema.ToNumType(),
//                     rangeId = (ushort)authoring.goal.rangeSchema.ID
//                 });
//             }
//         }
//     }
//
//     public class RewardAuthoring : MonoBehaviour
//     {
//         public RewardSchema reward;
//
//         public class RewardBaker : Baker<RewardAuthoring>
//         {
//             public override void Bake(RewardAuthoring authoring)
//             {
//                 var entity = GetEntity(TransformUsageFlags.None);
//                 AddComponent(entity, new Reward
//                 {
//                     id = (ushort)authoring.reward.ID,
//                     crossLinkType = authoring.reward.crossLink.ToCrossLinkType(),
//                     crossLinkID = (ushort)authoring.reward.crossLink.ID,
//                     dataContainerID = (ushort)authoring.reward.dataContainer.ID
//                 });
//             }
//         }
//     }
//
//     public class DataContainerAuthoring : MonoBehaviour
//     {
//         public DataContainerSchema dataContainer;
//
//         public class DataContainerBaker : Baker<DataContainerAuthoring>
//         {
//             public override void Bake(DataContainerAuthoring authoring)
//             {
//                 var entity = GetEntity(TransformUsageFlags.None);
//                 AddComponent(entity, new DataContainer
//                 {
//                     id = (ushort)authoring.dataContainer.ID,
//                     targetType = authoring.dataContainer.targetType,
//                     numType = authoring.dataContainer.numType,
//                     valueFloat = authoring.dataContainer.valueFloat,
//                     valueInt = authoring.dataContainer.valueInt
//                 });
//             }
//         }
//     }
//
//     public class RangeFloatAuthoring : MonoBehaviour
//     {
//         public RangeFloatSchema rangeFloat;
//
//         public class RangeFloatBaker : Baker<RangeFloatAuthoring>
//         {
//             public override void Bake(RangeFloatAuthoring authoring)
//             {
//                 var entity = GetEntity(TransformUsageFlags.None);
//                 AddComponent(entity, new RangeFloat
//                 {
//                     id = (ushort)authoring.rangeFloat.ID,
//                     checkType = authoring.rangeFloat.checkType,
//                     lower = authoring.rangeFloat.lower,
//                     upper = authoring.rangeFloat.upper
//                 });
//             }
//         }
//     }
//
//     public class RangeIntAuthoring : MonoBehaviour
//     {
//         public RangeIntSchema rangeInt;
//
//         public class RangeIntBaker : Baker<RangeIntAuthoring>
//         {
//             public override void Bake(RangeIntAuthoring authoring)
//             {
//                 var entity = GetEntity(TransformUsageFlags.None);
//                 AddComponent(entity, new RangeInt
//                 {
//                     id = (ushort)authoring.rangeInt.ID,
//                     checkType = authoring.rangeInt.checkType,
//                     lower = authoring.rangeInt.lower,
//                     upper = authoring.rangeInt.upper
//                 });
//             }
//         }
//     }
//
//     public class TimeAuthoring : MonoBehaviour
//     {
//         public TimeSchema time;
//
//         public class TimeBaker : Baker<TimeAuthoring>
//         {
//             public override void Bake(TimeAuthoring authoring)
//             {
//                 var entity = GetEntity(TransformUsageFlags.None);
//                 AddComponent(entity, new Time
//                 {
//                     id = (ushort)authoring.time.ID,
//                     crossLinkType = authoring.time.crossLinkType.ToCrossLinkType()
//                 });
//             }
//         }
//     }
// }