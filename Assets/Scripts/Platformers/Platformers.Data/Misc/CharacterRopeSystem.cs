using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.CharacterController;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(TransformSystemGroup))]
[BurstCompile]
public partial struct CharacterRopeSystem : ISystem
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
        var ecb = SystemAPI.GetSingletonRW<EndSimulationEntityCommandBufferSystem.Singleton>().ValueRW
            .CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (characterRope, entity) in SystemAPI.Query<CharacterRope>().WithEntityAccess())
        {
            if (characterRope.OwningCharacterEntity == Entity.Null) return;

            if (SystemAPI.HasComponent<PlatformerCharacterComponent>(characterRope.OwningCharacterEntity) &&
                SystemAPI.HasComponent<PlatformerCharacterStateMachine>(characterRope.OwningCharacterEntity))
            {
                var platformerCharacter =
                    SystemAPI.GetComponent<PlatformerCharacterComponent>(characterRope.OwningCharacterEntity);
                var characterStateMachine =
                    SystemAPI.GetComponent<PlatformerCharacterStateMachine>(characterRope.OwningCharacterEntity);
                var characterLocalToWorld = SystemAPI.GetComponent<LocalToWorld>(characterRope.OwningCharacterEntity);

                // Handle rope positioning
                {
                    var characterTransform =
                        new RigidTransform(characterLocalToWorld.Rotation, characterLocalToWorld.Position);
                    var anchorPointOnCharacter =
                        math.transform(characterTransform, platformerCharacter.LocalRopeAnchorPoint);
                    var ropeVector = characterStateMachine.RopeSwingState.AnchorPoint - anchorPointOnCharacter;
                    var ropeLength = math.length(ropeVector);
                    var ropeMidPoint = anchorPointOnCharacter + ropeVector * 0.5f;

                    SystemAPI.SetComponent(entity,
                        new LocalToWorld
                        {
                            Value = math.mul(
                                new float4x4(
                                    MathUtilities.CreateRotationWithUpPriority(math.normalizesafe(ropeVector),
                                        math.forward()), ropeMidPoint),
                                float4x4.Scale(new float3(0.04f, ropeLength * 0.5f, 0.04f)))
                        });
                }

                // Destroy self when not in rope swing state anymore
                if (characterStateMachine.CurrentState != CharacterState.RopeSwing) ecb.DestroyEntity(entity);
            }
        }
    }
}