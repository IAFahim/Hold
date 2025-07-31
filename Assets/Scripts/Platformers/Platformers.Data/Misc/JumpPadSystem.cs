using BovineLabs.Core.PhysicsStates;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.CharacterController;
using StatefulEventState = BovineLabs.Core.PhysicsStates.StatefulEventState;
using StatefulTriggerEvent = BovineLabs.Core.PhysicsStates.StatefulTriggerEvent;

// [UpdateInGroup(typeof(AfterPhysicsSystemGroup))]

[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateBefore(typeof(KinematicCharacterPhysicsUpdateGroup))]
[BurstCompile]
public partial struct JumpPadSystem : ISystem
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
        var job = new JumpPadJob
        {
            KinematicCharacterBodyLookup = SystemAPI.GetComponentLookup<KinematicCharacterBody>(false)
        };
        job.Schedule();
    }

    [BurstCompile]
    public partial struct JumpPadJob : IJobEntity
    {
        public ComponentLookup<KinematicCharacterBody> KinematicCharacterBodyLookup;

        private void Execute(Entity entity, in LocalTransform localTransform, in JumpPad jumpPad,
            in DynamicBuffer<StatefulTriggerEvent> triggerEventsBuffer)
        {
            for (var i = 0; i < triggerEventsBuffer.Length; i++)
            {
                var triggerEvent = triggerEventsBuffer[i];
                var otherEntity = triggerEvent.EntityB;

                // If a character has entered the trigger, add jumppad power to it
                if (triggerEvent.State == StatefulEventState.Enter &&
                    KinematicCharacterBodyLookup.TryGetComponent(otherEntity, out var characterBody))
                {
                    var jumpVelocity = MathUtilities.GetForwardFromRotation(localTransform.Rotation) *
                                       jumpPad.JumpPower;
                    characterBody.RelativeVelocity = jumpVelocity;

                    // Unground the character
                    if (characterBody.IsGrounded &&
                        math.dot(math.normalizesafe(jumpVelocity), characterBody.GroundHit.Normal) >
                        jumpPad.UngroundingDotThreshold) characterBody.IsGrounded = false;

                    KinematicCharacterBodyLookup[otherEntity] = characterBody;
                }
            }
        }
    }
}