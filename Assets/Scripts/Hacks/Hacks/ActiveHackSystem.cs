using Activates.Activates.Data;
using BovineLabs.Core.PhysicsStates;
using Focuses.Focuses.Data;
using Hacks.Hacks.Data;
using Unity.Burst;
using Unity.Entities;

namespace Hacks.Hacks
{
    public partial struct ActiveHackSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var mainEntity = SystemAPI.GetSingleton<FocusSingletonComponent>().Entity;
            new ActiveHackJob
            {
                MainEntity = mainEntity,
                Active = true
            }.ScheduleParallel();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }

    [BurstCompile]
    [WithPresent(typeof(HackReady))]
    public partial struct ActiveHackJob : IJobEntity
    {
        public Entity MainEntity;
        public bool Active;

        public void Execute(
            EnabledRefRW<HackReady> hackReady,
            in DynamicBuffer<StatefulTriggerEvent> triggerEvents,
            in DynamicBuffer<LinkedEntityGroup> linkedEntity
        )
        {
            foreach (var statefulTriggerEvent in triggerEvents)
            {
                if (MainEntity == statefulTriggerEvent.EntityB)
                {
                    if (statefulTriggerEvent.State == StatefulEventState.Enter)
                    {
                        hackReady.ValueRW = true;
                    }
                }
            }
        }
    }
}