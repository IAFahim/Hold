using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics.Authoring;
using Unity.Physics.GraphicsIntegration;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SocialPlatforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct SceneInitializationSystem : ISystem
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
        // Game init
        if (SystemAPI.HasSingleton<SceneInitialization>())
        {
            ref var sceneInitializer = ref SystemAPI.GetSingletonRW<SceneInitialization>().ValueRW;

            // Spawn character at spawn point
            var characterEntity = state.EntityManager.Instantiate(sceneInitializer.CharacterPrefabEntity);
            var spawnTransform = SystemAPI.GetComponent<LocalTransform>(sceneInitializer.CharacterSpawnPointEntity);
            SystemAPI.SetComponent(characterEntity,
                LocalTransform.FromPositionRotation(spawnTransform.Position, spawnTransform.Rotation));

            // Spawn camera
            var cameraEntity = state.EntityManager.Instantiate(sceneInitializer.CameraPrefabEntity);
            state.EntityManager.AddComponentData(cameraEntity, new MainEntityCamera());


            // Spawn player
            var playerEntity = state.EntityManager.Instantiate(sceneInitializer.PlayerPrefabEntity);

            // Assign camera & character to player
            var player = SystemAPI.GetComponent<PlatformerPlayer>(playerEntity);
            player.ControlledCharacter = characterEntity;
            player.ControlledCamera = cameraEntity;
            SystemAPI.SetComponent(playerEntity, player);

            state.EntityManager.DestroyEntity(SystemAPI.GetSingletonEntity<SceneInitialization>());
        }
    }
}