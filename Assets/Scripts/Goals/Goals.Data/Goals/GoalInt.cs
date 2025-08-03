using System.Runtime.CompilerServices;
using Goals.Goals.Data.Enum;
using Unity.Burst;
using Unity.Mathematics;

namespace Goals.Goals.Data.Goals
{
    [BurstCompile]
    public struct GoalInt
    {
        public ushort Key;

        /// <summary>
        /// The type of check to perform
        /// </summary>
        public ECheckType CheckType;

        /// <summary>
        /// The target/expected value
        /// </summary>
        public int TargetValue;


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
                case ECheckType.GreaterOrEqual:
                    conditionMet = currentValue >= TargetValue;
                    if (TargetValue > 0)
                    {
                        progress = math.min(1f, (float)currentValue / TargetValue);
                    }
                    else if (TargetValue < 0)
                    {
                        // Handle negative targets
                        progress = currentValue >= TargetValue
                            ? 1f
                            : math.max(0f, 1f + ((float)currentValue / math.abs(TargetValue)));
                    }
                    else
                    {
                        progress = conditionMet ? 1f : 0f;
                    }

                    break;

                case ECheckType.GreaterThan:
                    conditionMet = currentValue > TargetValue;
                    if (TargetValue > 0)
                    {
                        progress = math.min(1f, (float)currentValue / (TargetValue + 1));
                    }
                    else if (TargetValue < 0)
                    {
                        progress = currentValue > TargetValue
                            ? 1f
                            : math.max(0f, 1f + ((float)currentValue / math.abs(TargetValue)));
                    }
                    else
                    {
                        progress = conditionMet ? 1f : 0f;
                    }

                    break;

                case ECheckType.LessOrEqual:
                    conditionMet = currentValue <= TargetValue;
                    if (currentValue <= TargetValue)
                    {
                        progress = 1f;
                    }
                    else
                    {
                        int overshoot = currentValue - TargetValue;
                        progress = math.max(0f, 1f - ((float)overshoot / math.max(1, math.abs(TargetValue))));
                    }

                    break;

                case ECheckType.LessThan:
                    conditionMet = currentValue < TargetValue;
                    if (currentValue < TargetValue)
                    {
                        progress = 1f;
                    }
                    else
                    {
                        int overshoot = currentValue - TargetValue + 1;
                        progress = math.max(0f, 1f - ((float)overshoot / math.max(1, math.abs(TargetValue))));
                    }

                    break;

                case ECheckType.Equals:
                    conditionMet = currentValue == TargetValue;
                    if (TargetValue == 0)
                    {
                        progress = conditionMet ? 1f : math.max(0f, 1f - math.abs(currentValue) * 0.1f);
                    }
                    else
                    {
                        int distance = math.abs(currentValue - TargetValue);
                        progress = math.max(0f, 1f - ((float)distance / math.abs(TargetValue)));
                        if (conditionMet) progress = 1f;
                    }

                    break;

                case ECheckType.NotEqual:
                    conditionMet = currentValue != TargetValue;
                    progress = conditionMet ? 1f : 0f;
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