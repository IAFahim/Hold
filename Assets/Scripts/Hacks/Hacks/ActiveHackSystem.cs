using BovineLabs.Core.PhysicsStates;
using Focuses.Focuses.Data;
using Hacks.Hacks.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Hacks.Hacks
{
    [BurstCompile]
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
            if (mainEntity == Entity.Null) return;

            SystemAPI.GetComponentRW<HackActive>(mainEntity).ValueRW.Enable = false;
            new ActiveHackJob
            {
                MainEntity = mainEntity,
                Active = true,
                DeltaTime = SystemAPI.Time.DeltaTime,
                HackActive = SystemAPI.GetComponentLookup<HackActive>()
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
        [NativeDisableParallelForRestriction] public ComponentLookup<HackActive> HackActive;
        public Entity MainEntity;
        public bool Active;
        public float DeltaTime;

        void Execute(
            EnabledRefRW<HackReady> hackReadyEnable,
            ref HackReady hackReady,
            in DynamicBuffer<StatefulTriggerEvent> triggerEvents
        )
        {
            foreach (var statefulTriggerEvent in triggerEvents)
            {
                if (MainEntity == statefulTriggerEvent.EntityB)
                {
                    if (statefulTriggerEvent.State == StatefulEventState.Exit)
                    {
                        hackReadyEnable.ValueRW = true;
                    }
                }

                if (hackReadyEnable.ValueRO)
                {
                    if (hackReady.ActiveDuration > 0)
                    {
                        hackReady.ActiveDuration -= DeltaTime;
                        if (Active)
                        {
                            HackActive.GetRefRW(MainEntity).ValueRW.Enable = true;
                        }
                    }
                    else
                    {
                        hackReadyEnable.ValueRW = false;
                    }
                }
            }
        }
    }
}