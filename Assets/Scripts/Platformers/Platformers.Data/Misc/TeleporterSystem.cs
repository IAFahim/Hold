using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.CharacterController;

using StatefulEventState = BovineLabs.Core.PhysicsStates.StatefulEventState;
using StatefulTriggerEvent = BovineLabs.Core.PhysicsStates.StatefulTriggerEvent;

[UpdateInGroup(typeof(PhysicsSystemGroup))]
[BurstCompile]
public partial struct TeleporterSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var job = new TeleporterJob
        {
            LocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(false),
            CharacterBodyLookup = SystemAPI.GetComponentLookup<KinematicCharacterBody>(true),
            CharacterInterpolationLookup = SystemAPI.GetComponentLookup<CharacterInterpolation>(false)
        };
        job.Schedule();
    }

    [BurstCompile]
    public partial struct TeleporterJob : IJobEntity
    {
        public ComponentLookup<LocalTransform> LocalTransformLookup;
        [ReadOnly] public ComponentLookup<KinematicCharacterBody> CharacterBodyLookup;
        public ComponentLookup<CharacterInterpolation> CharacterInterpolationLookup;

        private void Execute(Entity entity, in Teleporter teleporter,
            in DynamicBuffer<StatefulTriggerEvent> triggerEventsBuffer)
        {
            // Only teleport if there is a destination
            if (teleporter.DestinationEntity != Entity.Null)
                for (var i = 0; i < triggerEventsBuffer.Length; i++)
                {
                    var triggerEvent = triggerEventsBuffer[i];
                    var otherEntity = triggerEvent.EntityB;

                    // If a character has entered the trigger, move its translation to the destination
                    if (triggerEvent.State == StatefulEventState.Enter && CharacterBodyLookup.HasComponent(otherEntity))
                    {
                        var t = LocalTransformLookup[otherEntity];
                        t.Position = LocalTransformLookup[teleporter.DestinationEntity].Position;
                        t.Rotation = LocalTransformLookup[teleporter.DestinationEntity].Rotation;
                        LocalTransformLookup[otherEntity] = t;

                        // Bypass interpolation
                        if (CharacterInterpolationLookup.HasComponent(otherEntity))
                        {
                            var interpolation = CharacterInterpolationLookup[otherEntity];
                            interpolation.SkipNextInterpolation();
                            CharacterInterpolationLookup[otherEntity] = interpolation;
                        }
                    }
                }
        }
    }
}