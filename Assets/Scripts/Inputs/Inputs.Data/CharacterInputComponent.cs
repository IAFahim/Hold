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
        public byte Value;
        private const byte Right = 0b0001_0000;
        private const byte Slide = 0b0010_0000;
        private const byte Jump = 0b0100_0000;
        private const byte Left = 0b1000_0000;
        
        private const byte Sprint = 0b0000_1000;
        private const byte Couch = 0b0000_0100;

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearFirst4Bit()
        {
            Value &= 0b0000_1111;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRight()
        {
            ClearFirst4Bit();
            Value |= Right;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsRightActivatedThisFrame()
        {
            return (Value & Right) != 0;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsLeftActivatedThisFrame()
        {
            return (Value & Left) != 0;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsJumping()
        {
            return (Value & Jump) != 0;
        }
        
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsSprinting()
        {
            return (Value & Sprint) != 0;
        }
        
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsCrouching()
        {
            return (Value & Couch) != 0;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLeft()
        {
            ClearFirst4Bit();
            Value |= Left;
        }


        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetJump()
        {
            ClearFirst4Bit();
            Value |= Jump;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSlide()
        {
            ClearFirst4Bit();
            Value |= Slide;
        }
        

        private const byte LeftLane = 0b0000_0001;
        public const byte MiddleLane = 0b0000_0010;
        private const byte RightLane = 0b0000_0011;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsGoingToLeftLine()
        {
            var last4Only = Value & 0b0000_1111;
            return last4Only == LeftLane;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsGoingToMiddleLine()
        {
            var last4Only = Value & 0b0000_1111;
            return last4Only == MiddleLane;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsGoingToRightLine()
        {
            var last4Only = Value & 0b0000_1111;
            return last4Only == RightLane;
        }
        
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearLine()
        {
            Value &= 0b1111_1100;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GoToRightLine()
        {
            ClearLine();
            Value |= RightLane;
        }
        
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GoToLeftLine()
        {
            ClearLine();
            Value |= LeftLane;
        }
        
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GoToMiddleLine()
        {
            ClearLine();
            Value |= MiddleLane;
        }
        
        
    }
}