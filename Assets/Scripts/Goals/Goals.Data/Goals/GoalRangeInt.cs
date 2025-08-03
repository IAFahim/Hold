using System.Runtime.CompilerServices;
using Goals.Goals.Data.Enum;
using Unity.Burst;
using Unity.Mathematics;

namespace Goals.Goals.Data.Goals
{
    [BurstCompile]
    public struct GoalRangeInt
    {
        public ushort Key;

        /// <summary>
        /// The type of check to perform
        /// </summary>
        public ERangeCheckType CheckType;

        public int LowerLimit;
        public int UpperLimit;

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
            progress = 0f;
            bool conditionMet;

            switch (CheckType)
            {
                case ERangeCheckType.Between:
                    conditionMet = currentValue >= LowerLimit && currentValue <= UpperLimit;
                    if (UpperLimit > LowerLimit)
                    {
                        if (currentValue < LowerLimit)
                        {
                            int distance = LowerLimit - currentValue;
                            int rangeSize = UpperLimit - LowerLimit;
                            progress = math.max(0f, 1f - ((float)distance / rangeSize));
                        }
                        else if (currentValue > UpperLimit)
                        {
                            int distance = currentValue - UpperLimit;
                            int rangeSize = UpperLimit - LowerLimit;
                            progress = math.max(0f, 1f - ((float)distance / rangeSize));
                        }
                        else
                        {
                            progress = 1f;
                        }
                    }
                    else
                    {
                        progress = conditionMet ? 1f : 0f;
                    }

                    break;

                case ERangeCheckType.NotBetween:
                    conditionMet = currentValue < LowerLimit || currentValue > UpperLimit;
                    if (UpperLimit > LowerLimit)
                    {
                        if (currentValue < LowerLimit)
                        {
                            int distance = LowerLimit - currentValue;
                            int rangeSize = UpperLimit - LowerLimit;
                            progress = math.min(1f, (float)distance / rangeSize);
                        }
                        else if (currentValue > UpperLimit)
                        {
                            int distance = currentValue - UpperLimit;
                            int rangeSize = UpperLimit - LowerLimit;
                            progress = math.min(1f, (float)distance / rangeSize);
                        }
                        else
                        {
                            progress = 0f;
                        }
                    }
                    else
                    {
                        progress = conditionMet ? 1f : 0f;
                    }

                    break;

                default:
                    progress = 0f;
                    conditionMet = false;
                    break;
            }

            progress = math.clamp(progress, 0f, 1f);
            return conditionMet;
        }
    }
}