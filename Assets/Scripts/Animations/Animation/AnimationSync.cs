using System.Runtime.CompilerServices;
using Animations.Animation.Data;
using BovineLabs.Core.LifeCycle;
using Unity.Entities;
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

            foreach (var (animatorComponent, entity) in
                     SystemAPI.Query<RefRO<AnimatorHybridLink>>()
                         .WithAbsent<DestroyEntity>()
                         .WithEntityAccess()
                    )
            {
                AsyncAddressableGameObjectPool.Instance.ReturnAsset(animatorComponent.ValueRO.Ref.Value.gameObject);
                ecb.RemoveComponent<AnimatorHybridLink>(entity);
                ecb.DestroyEntity(entity);
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