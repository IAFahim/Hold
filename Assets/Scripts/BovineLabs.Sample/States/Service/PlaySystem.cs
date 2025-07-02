using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace BovineLabs.Sample.States.Service
{
    public partial struct PlaySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            Debug.Log("PlaySystem::OnUpdate");
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}