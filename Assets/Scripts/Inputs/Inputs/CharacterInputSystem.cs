using System.Runtime.CompilerServices;
using BovineLabs.Core.Input;
using Focuses.Focuses.Data;
using Inputs.Inputs.Data;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Inputs.Inputs
{
    [BurstCompile]
    public partial struct CharacterInputSystem : ISystem
    {
        /// <inheritdoc/>
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        }


        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var playerEntity = SystemAPI.GetSingletonRW<FocusSingletonComponent>().ValueRW.Entity;
            ref var characterInput = ref SystemAPI.GetComponentRW<CharacterInputComponent>(playerEntity).ValueRW;
            var localTransform = SystemAPI.GetComponentRO<LocalTransform>(playerEntity).ValueRO;
            var inputComponent = SystemAPI.GetSingleton<InputComponent>();

            if (!inputComponent.Click)
            {
                Clear(ref characterInput);
                return;
            }

            var swipeDelta = inputComponent.SwipeDelta;

            var absX = math.abs(swipeDelta.x);
            var absY = math.abs(swipeDelta.y);

            if (absX + absY == 0)
            {
                Clear(ref characterInput);
                return;
            }

            if (absX > absY)
            {
                HandleHorizontalSwipe(swipeDelta.x, localTransform.Position.x, ref characterInput);
            }
            else
            {
                HandleJumpSlide(swipeDelta, ref characterInput);
            }
        }

        private void HandleHorizontalSwipe(float swipeDeltaX, float currentX,
            ref CharacterInputComponent characterInput)
        {
            const float leftLaneX = -1f;
            const float middleLaneX = 0f;
            const float rightLaneX = 1f;
            const float laneThreshold = 0.5f;

            if (swipeDeltaX > 0) // Swiping right
            {
                characterInput.SetRight();

                if (currentX < leftLaneX + laneThreshold) // Currently in left lane
                {
                    characterInput.GoToMiddleLine();
                }
                else if (currentX < middleLaneX + laneThreshold) // Currently in middle lane
                {
                    characterInput.GoToRightLine();
                }
                // If already in right lane, do nothing or handle as needed
            }
            else // Swiping left
            {
                characterInput.SetLeft();

                if (currentX > rightLaneX - laneThreshold) // Currently in right lane
                {
                    characterInput.GoToMiddleLine();
                }
                else if (currentX > middleLaneX - laneThreshold) // Currently in middle lane
                {
                    characterInput.GoToLeftLine();
                }
                // If already in left lane, do nothing or handle as needed
            }
        }


        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void HandleJumpSlide(in float2 swipeDelta, ref CharacterInputComponent characterInput)
        {
            if (swipeDelta.y > 0)
            {
                characterInput.SetJump();
                return;
            }

            characterInput.SetSlide();
        }


        private void Clear(ref CharacterInputComponent characterInput)
        {
            characterInput.ClearFirst4Bit();
        }


        /// <inheritdoc/>
        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}