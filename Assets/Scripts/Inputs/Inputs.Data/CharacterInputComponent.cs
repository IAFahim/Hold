using System;
using System.Runtime.CompilerServices;
using Moves.Move.Data;
using Unity.Burst;
using Unity.Entities;

namespace Inputs.Inputs.Data
{
    /// <summary>
    /// Stores the current input state for a character as a bitmask.
    /// This component is designed for high-performance access in Burst-compiled jobs.
    /// </summary>
    /// <remarks>
    /// Conforms to:
    /// - Rule 0.1: Performance by Default (struct, BurstCompile, unmanaged)
    /// - Rule 5.1.1: Pure Data (methods are simple bitwise helpers)
    /// - Rule 5.1.2: Use `unmanaged` Types
    /// </remarks>
    [BurstCompile]
    public struct CharacterInputComponent : IComponentData
    {
        /// <summary>
        /// The bitmask representing the current inputs.
        /// </summary>
        public ECharacterInput Value;

        // --- Setters ---

        /// <summary>
        /// Sets the state of a specific input flag.
        /// </summary>
        /// <param name="flag">The input flag to modify.</param>
        /// <param name="isActive">The desired state for the flag.</param>
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetActive(ECharacterInput flag, bool isActive)
        {
            if (isActive)
            {
                Value |= flag;
            }
            else
            {
                Value &= ~flag;
            }
        }

        /// <summary>Sets the Left input state.</summary>
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLeftActive(bool isActive) => SetActive(ECharacterInput.Left, isActive);

        /// <summary>Sets the Right input state.</summary>
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRightActive(bool isActive) => SetActive(ECharacterInput.Right, isActive);

        /// <summary>Sets the Jump input state.</summary>
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetJumpActive(bool isActive) => SetActive(ECharacterInput.Jump, isActive);

        /// <summary>Sets the Slide input state.</summary>
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSlideActive(bool isActive) => SetActive(ECharacterInput.Slide, isActive);


        // --- Getters ---

        /// <summary>
        /// Checks if a specific input flag is active.
        /// </summary>
        /// <param name="flag">The input flag to check.</param>
        /// <returns>True if the flag is set in the value.</returns>
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsActive(ECharacterInput flag)
        {
            return (Value & flag) == flag;
        }

        /// <summary>Checks if the Left input is active.</summary>
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsLeftActive() => IsActive(ECharacterInput.Left);

        /// <summary>Checks if the Right input is active.</summary>
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsRightActive() => IsActive(ECharacterInput.Right);

        /// <summary>Checks if the Jump input is active.</summary>
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsJumpActive() => IsActive(ECharacterInput.Jump);

        /// <summary>Checks if the Slide input is active.</summary>
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsSlideActive() => IsActive(ECharacterInput.Slide);


        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearDirection()
        {
            Value &= ~ECharacterInput.Direction;
        }
        
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetLine()
        {
             return (byte)(Value & ~ECharacterInput.Direction);
        }
        
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearLine()
        {
            Value &= ECharacterInput.ClearLane;
        }
        
        

        
    }

    /// <summary>
    /// A bitmask enum representing character inputs.
    /// </summary>
    [Flags]
    public enum ECharacterInput : byte
    {
        None = 0,
        Left = 0b0001_0000,
        Right = 0b0010_0000,
        Jump = 0b0100_0000,
        Slide = 0b1000_0000,
        Direction = Left | Right | Jump | Slide,
        GoToLeft = 3,
        GoToMiddle = 2,
        GoToRight = 1,
        ClearLane = 0b1111_1100,
        Reached = 0b0000_1000,
    }
}