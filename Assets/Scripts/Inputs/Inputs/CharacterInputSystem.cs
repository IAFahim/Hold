using System.Runtime.CompilerServices;
using BovineLabs.Core.Input;
using Focuses.Focuses.Data;
using Inputs.Inputs.Data;
using Moves.Move.Data;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Inputs.Inputs
{
    /// <summary>
    /// Processes raw swipe input from the input system and translates it into a high-level
    /// character input command (e.g., Up, Down, Left, Right).
    /// This system ensures that input is only processed when not interacting with UI.
    /// </summary>
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
            var inputCommon = SystemAPI.GetSingleton<InputCommon>();

            var playerEntity = SystemAPI.GetSingletonRW<FocusSingletonComponent>().ValueRW.Entity;
            ref var characterInput = ref SystemAPI.GetComponentRW<CharacterInputComponent>(playerEntity).ValueRW;
            var localTransform = SystemAPI.GetComponentRO<LocalTransform>(playerEntity).ValueRO;

            // If there's no input or the input is over a UI element, reset the character input and do nothing.
            // This prevents unintended character movement while interacting with menus.
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
                    characterInput.Value |= ECharacterInput.Right;
                    if (localTransform.Position.x == 0)
                    {
                        characterInput.ClearLine();
                        
                        characterInput.Value |= ECharacterInput.GoToRight;
                    }
                    else if (localTransform.Position.x < 0)
                    {
                        characterInput.ClearLine();
                        characterInput.Value |= ECharacterInput.GoToMiddle;
                    }
                    return;
                }

                characterInput.Value |= ECharacterInput.Left;
                if (localTransform.Position.x == 0)
                {
                    characterInput.ClearLine();
                    characterInput.Value |= ECharacterInput.GoToLeft;
                }
                else if (localTransform.Position.x > 0)
                {
                    characterInput.ClearLine();
                    characterInput.Value |= ECharacterInput.GoToMiddle;
                }
                return;
            }

            if (swipeDelta.y > 0)
            {
                characterInput.Value |= ECharacterInput.Jump;
                return;
            }

            characterInput.Value |= ECharacterInput.Slide;
        }


        private void Clear(ref CharacterInputComponent characterInput)
        {
            characterInput.ClearDirection();
        }


        /// <inheritdoc/>
        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}