using BovineLabs.Core.Input;
using Focuses.Focuses.Data;
using Inputs.Inputs.Data;
using Moves.Move.Data;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
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
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var inputCommon = SystemAPI.GetSingleton<InputCommon>();

            var playerEntity = SystemAPI.GetSingletonRW<FocusSingletonComponent>().ValueRW.Entity;
            ref var characterInput = ref SystemAPI.GetComponentRW<CharacterInputComponent>(playerEntity).ValueRW;

            // If there's no input or the input is over a UI element, reset the character input and do nothing.
            // This prevents unintended character movement while interacting with menus.
            var groundCharacter = SystemAPI.GetComponentRW<GroundMoveDirectionComponent>(playerEntity);
            if (!inputCommon.AnyButtonPress)
            {
                Clear(ref characterInput);
                return;
            }


            var inputComponent = SystemAPI.GetSingleton<InputComponent>();
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
                    characterInput.Value = ECharacterInput.Right;
                    groundCharacter.ValueRW.Value.x = 1;
                    Debug.Log(ECharacterInput.Right);
                    return;
                }

                characterInput.Value = ECharacterInput.Left;
                groundCharacter.ValueRW.Value.x = -1;
                Debug.Log(ECharacterInput.Left);
                return;
            }

            if (swipeDelta.y > 0)
            {
                characterInput.Value = ECharacterInput.Jump;
                return;
            }

            characterInput.Value = ECharacterInput.Slide;
        }

        private void Clear(ref CharacterInputComponent characterInput)
        {
            characterInput.Value = ECharacterInput.None;
        }


        /// <inheritdoc/>
        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}