// using BovineLabs.Core.PhysicsStates;
// using Focuses.Focuses.Data;
// using Unity.Burst;
// using Unity.Entities;
// using UnityEngine;
//
// namespace Stations.Stations
// {
//     public partial struct StationReachedSystem : ISystem
//     {
//         [BurstCompile]
//         public void OnCreate(ref SystemState state)
//         {
//         }
//
//         [BurstCompile]
//         public void OnUpdate(ref SystemState state)
//         {
//             var mainEntity = SystemAPI.GetSingleton<FocusSingletonComponent>().Entity;
//             new StationReachedSystemJob
//             {
//                 MainEntity = mainEntity
//             }.ScheduleParallel();
//         }
//
//         [BurstCompile]
//         public void OnDestroy(ref SystemState state)
//         {
//         }
//     }
//
//     [BurstCompile]
//     public partial struct StationReachedSystemJob : IJobEntity
//     {
//         public Entity MainEntity;
//
//         private void Execute(in DynamicBuffer<StatefulTriggerEvent> triggerEvents)
//         {
//             foreach (var statefulTriggerEvent in triggerEvents)
//             {
//                 if (MainEntity == statefulTriggerEvent.EntityB)
//                 {
//                     if (statefulTriggerEvent.State == StatefulEventState.Enter)
//                     {
//                         Debug.Log("Reached");
//                     }
//                 }
//             }
//         }
//     }
// }