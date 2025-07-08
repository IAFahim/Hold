using System.Runtime.CompilerServices;
using Animations.Animation.Data;
using Animations.Animation.Data.Classes;
using Animations.Animation.Data.enums;
using BovineLabs.Core.LifeCycle;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Animations.Animation
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateBefore(typeof(EndInitializationEntityCommandBufferSystem))]
    public partial struct AnimationSync : ISystem
    {
        private static readonly int ClipIndex = Animator.StringToHash("ClipIndex");

        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = SystemAPI.GetSingletonRW<EndSimulationEntityCommandBufferSystem.Singleton>()
                .ValueRW.CreateCommandBuffer(state.WorldUnmanaged);

            foreach (
                var (animationComponent, localTransform, entity) in
                SystemAPI.Query<RefRO<AnimatorAssetIndexDisposeComponent>, RefRO<LocalTransform>>()
                    .WithEntityAccess()
            )
            {
                if (!AsyncAddressableGameObjectPool.Instance.TryGetRequest(
                        entity, animationComponent.ValueRO.Index,
                        localTransform.ValueRO.Position,
                        localTransform.ValueRO.Rotation,
                        out GameObject gameObject
                    )) continue;
                ecb.AddComponent(entity, new AnimatorHybridLink
                {
                    Ref = gameObject.GetComponent<Animator>()
                });
                ecb.RemoveComponent<AnimatorAssetIndexDisposeComponent>(entity);
            }

            foreach (var (animatorComponent, characterState, entity) in
                     SystemAPI.Query<
                             RefRO<AnimatorHybridLink>,
                             RefRO<CharacterStateAnimation>
                         >()
                         .WithPresent<DestroyEntity>()
                         .WithEntityAccess()
                    )
            {
                var destroyEntity = SystemAPI.IsComponentEnabled<DestroyEntity>(entity);
                if (destroyEntity)
                {
                    AsyncAddressableGameObjectPool.Instance.ReturnAsset(animatorComponent.ValueRO.Ref.Value.gameObject);
                    ecb.RemoveComponent<AnimatorHybridLink>(entity);
                    ecb.DestroyEntity(entity);
                }
                else
                {
                    // float2 moveVector = new float2();
                    // float velocityMagnitude = 0;
                    // bool isSprinting = false;
                    // float groundSprintMaxSpeed = 0;
                    // float groundRunMaxSpeed = 0;
                    // float crouchedMaxSpeed = 0;
                    // float climbingSpeed = 0;
                    // float ledgeMoveSpeed = 0;
                    // float swimmingMaxSpeed = 0;
                    // quaternion rotation = quaternion.identity;
                    // float3 lastKnownWallNormal = 0;

                    characterState.ValueRO.Update(
                        characterState.ValueRO.moveVector,
                        characterState.ValueRO.velocityMagnitude,
                        characterState.ValueRO.isSprinting,
                        characterState.ValueRO.groundSprintMaxSpeed,
                        characterState.ValueRO.groundRunMaxSpeed,
                        characterState.ValueRO.crouchedMaxSpeed,
                        characterState.ValueRO.climbingSpeed,
                        characterState.ValueRO.ledgeMoveSpeed,
                        characterState.ValueRO.swimmingMaxSpeed,
                        characterState.ValueRO.rotation,
                        characterState.ValueRO.lastKnownWallNormal,
                        out float speed,
                        out EAnimationState animationState
                    );
                    animatorComponent.ValueRO.Ref.Value.speed = speed;
                    if (characterState.ValueRO.Current == characterState.ValueRO.Previous) return;
                    animatorComponent.ValueRO.Ref.Value.SetInteger(ClipIndex, (int)animationState);
                }
            }
        }

        public void OnStopRunning(ref SystemState state)
        {
            foreach (var animatorComponent in SystemAPI.Query<RefRO<AnimatorHybridLink>>())
            {
                ReturnToPool(animatorComponent);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ReturnToPool(RefRO<AnimatorHybridLink> animatorComponent)
        {
            AsyncAddressableGameObjectPool.Instance.ReturnAsset(animatorComponent.ValueRO.Ref.Value.gameObject);
        }
    }
}