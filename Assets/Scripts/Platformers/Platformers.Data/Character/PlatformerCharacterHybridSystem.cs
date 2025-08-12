using Follows.Follows.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.CharacterController;
using Unity.Transforms;
using UnityEngine;

[AlwaysSynchronizeSystem]
[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
[UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))]
public partial class PlatformerCharacterHybridSystem : SystemBase
{


    protected override void OnUpdate()
    {
        var ecb = SystemAPI.GetSingletonRW<EndSimulationEntityCommandBufferSystem.Singleton>().ValueRW
            .CreateCommandBuffer(World.Unmanaged);

        // Create
        foreach (var (characterAnimation, hybridData, entity) in SystemAPI
                     .Query<RefRW<PlatformerCharacterAnimation>, PlatformerCharacterHybridData>()
                     .WithNone<PlatformerCharacterHybridLink>()
                     .WithEntityAccess())
        {
            var tmpObject = GameObject.Instantiate(hybridData.MeshPrefab);
            var animator = tmpObject.GetComponent<Animator>();

            ecb.AddComponent(entity, new PlatformerCharacterHybridLink
            {
                Object = tmpObject,
                Animator = animator
            });
        }


        // Update
        foreach ((
                     RefRW<PlatformerCharacterAnimation> characterAnimation, KinematicCharacterBody characterBody,
                     LocalTransform characterTransform, PlatformerCharacterComponent characterComponent,
                     PlatformerCharacterStateMachine characterStateMachine, PlatformerCharacterControl characterControl,
                     PlatformerCharacterHybridLink hybridLink,
                     Entity entity
                 )
                 in SystemAPI.Query<
                     RefRW<PlatformerCharacterAnimation>,
                     KinematicCharacterBody,
                     LocalTransform,
                     PlatformerCharacterComponent,
                     PlatformerCharacterStateMachine,
                     PlatformerCharacterControl,
                     PlatformerCharacterHybridLink
                 >().WithEntityAccess()
                )
            if (hybridLink.Object)
            {
                // Transform
                var meshRootLTW = SystemAPI.GetComponent<LocalToWorld>(characterComponent.MeshRootEntity);
                hybridLink.Object.transform.SetLocalPositionAndRotation(meshRootLTW.Position, meshRootLTW.Rotation);
                var followEnableComponent = SystemAPI.GetComponentRO<FollowEnableComponent>(entity);

                // Animation
                PlatformerCharacterAnimationHandler.UpdateAnimation(
                    hybridLink.Animator,
                    ref characterAnimation.ValueRW,
                    in characterBody,
                    in characterComponent,
                    in characterStateMachine,
                    in characterControl,
                    in characterTransform,
                    in followEnableComponent.ValueRO
                    );

                // Mesh enabling
                if (characterStateMachine.CurrentState == CharacterState.Rolling)
                {
                    if (hybridLink.Object.activeSelf) hybridLink.Object.SetActive(false);
                }
                else
                {
                    if (!hybridLink.Object.activeSelf) hybridLink.Object.SetActive(true);
                }
            }

        // Destroy
        foreach (var (hybridLink, entity) in SystemAPI.Query<PlatformerCharacterHybridLink>()
                     .WithNone<PlatformerCharacterHybridData>()
                     .WithEntityAccess())
        {
            GameObject.Destroy(hybridLink.Object);
            ecb.RemoveComponent<PlatformerCharacterHybridLink>(entity);
        }
    }
}