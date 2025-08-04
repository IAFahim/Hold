using System;
using System.Runtime.CompilerServices;
using BovineLabs.Essence.Data;
using Goals.Goals.Data.Enum;
using Unity.Burst;

namespace Goals.Goals.Data.Goals
{
    [BurstCompile]
    [Serializable]
    public struct GoalRangeInt
    {
        public ushort id;

        public ushort ID
        {
            readonly get => id;
            set => id = value;
        }

        public IntrinsicKey key;

        /// <summary>
        /// The type of check to perform
        /// </summary>
        public ECheckType checkType;

        public int lowerLimit;
        
        /// <summary>
        /// Optional secondary value for range checks (Between/NotBetween)
        /// </summary>
        public int upperLimit;

        /// <summary>
        /// Evaluates the goal condition and calculates progress
        /// </summary>
        /// <param name="currentValue">The current value to check</param>
        /// <param name="progress">Progress as a value between 0.0 and 1.0</param>
        /// <returns>True if the goal condition is met, false otherwise</returns>
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryProgress(int currentValue, out float progress)
        {
            return checkType.TryProgress(currentValue, lowerLimit, upperLimit, out progress);
        }
    }
}