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


        /// <inheritdoc/>
        // [BurstCompile]
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

            // Determine the dominant axis of the swipe to convert it into a directional command.
            var absX = math.abs(swipeDelta.x);
            var absY = math.abs(swipeDelta.y);
            if (absX + absY == 0)
            {
                Clear(ref characterInput);
                return;
            }

            if (absX > absY)
            {
                if (0 < swipeDelta.x)
                {
                    characterInput.SetRight();
                    if (localTransform.Position.x < 0)
                    {
                        if (characterInput.IsGoingToMiddleLine())
                        {
                            characterInput.GoToRightLine();
                            return;
                        }

                        characterInput.GoToMiddleLine();
                    }
                    else
                    {
                        characterInput.GoToRightLine();
                    }

                    return;
                }

                characterInput.SetLeft();
                if (0 < localTransform.Position.x)
                {
                    if (characterInput.IsGoingToMiddleLine())
                    {
                        characterInput.GoToLeftLine();
                        return;
                    }

                    characterInput.GoToMiddleLine();
                }
                else
                {
                    characterInput.GoToLeftLine();
                }

                return;
            }

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