using BovineLabs.Core.Input;
using Moves.Move.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Inputs.Inputs
{
    public partial struct CharacterInputSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            InputComponent inputComponent = SystemAPI.GetSingleton<InputComponent>();
            new CharacterInputJobEntity
            {
                Side = inputComponent.SwipeDelta.x
            }.ScheduleParallel();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }

    public partial struct CharacterInputJobEntity : IJobEntity
    {
        [ReadOnly] public float Side;

        private void Execute(ref GroundMoveDirectionComponent groundMoveDirection)
        {
            groundMoveDirection.Value.x = Side;
        }
    }
}