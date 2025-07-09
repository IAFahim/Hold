using System.Runtime.CompilerServices;
using Animations.Animation.Data;
using Animations.Animation.Data.Classes;
using Animations.Animation.Data.enums;
using BovineLabs.Core.LifeCycle;
using States.States.Data;
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


            foreach (var (localTransform, hybridLink, animationState, characterState, entity) in
                     SystemAPI.Query<
                             RefRO<LocalTransform>,
                             RefRO<AnimatorHybridLinkComponent>,
                             RefRO<AnimationStateComponent>,
                             RefRO<CharacterStateComponent>
                         >()
                         .WithPresent<DestroyEntity>()
                         .WithEntityAccess()
                    )
            {
                var destroyEntity = SystemAPI.IsComponentEnabled<DestroyEntity>(entity);
                if (destroyEntity)
                {
                    AsyncAddressableGameObjectPool.Instance.ReturnAsset(hybridLink.ValueRO.Ref.Value.gameObject);
                    ecb.RemoveComponent<AnimatorHybridLinkComponent>(entity);
                    ecb.DestroyEntity(entity);
                }
                else
                {
                    hybridLink.ValueRO.Ref.Value.transform.SetLocalPositionAndRotation(
                        localTransform.ValueRO.Position,
                        localTransform.ValueRO.Rotation
                    );
                    if (characterState.ValueRO.Current == characterState.ValueRO.Previous) return;
                    hybridLink.ValueRO.Ref.Value.SetInteger(ClipIndex, (int)animationState.ValueRO.Animation);
                    hybridLink.ValueRO.Ref.Value.speed = animationState.ValueRO.Speed;
                }
            }

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
                ecb.AddComponent(entity, new AnimatorHybridLinkComponent
                {
                    Ref = gameObject.GetComponent<Animator>()
                });
                ecb.RemoveComponent<AnimatorAssetIndexDisposeComponent>(entity);
            }
        }

        public void OnStopRunning(ref SystemState state)
        {
            foreach (var animatorComponent in SystemAPI.Query<RefRO<AnimatorHybridLinkComponent>>())
            {
                ReturnToPool(animatorComponent);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ReturnToPool(RefRO<AnimatorHybridLinkComponent> animatorComponent)
        {
            AsyncAddressableGameObjectPool.Instance.ReturnAsset(animatorComponent.ValueRO.Ref.Value.gameObject);
        }
    }
}