using System.Runtime.CompilerServices;
using Goals.Goals.Data.Enum;
using Unity.Burst;
using Unity.Mathematics;

namespace Goals.Goals.Data.Goals
{
    [BurstCompile]
    public struct GoalFloat
    {
        public ushort Key;

        /// <summary>
        /// The type of check to perform
        /// </summary>
        public ECheckType CheckType;

        /// <summary>
        /// The target/expected value
        /// </summary>
        public float TargetValue;

        /// <summary>
        /// Tolerance for floating-point comparisons
        /// </summary>
        private const float Tolerance = math.EPSILON;

        /// <summary>
        /// Evaluates the goal condition and calculates progress
        /// </summary>
        /// <param name="currentValue">The current value to check</param>
        /// <param name="progress">Progress as a value between 0.0 and 1.0</param>
        /// <returns>True if the goal condition is met, false otherwise</returns>
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryProgress(float currentValue, out float progress)
        {
            progress = 0f;
            bool conditionMet;

            switch (CheckType)
            {
                case ECheckType.GreaterOrEqual:
                    conditionMet = currentValue >= TargetValue - Tolerance;
                    if (TargetValue > Tolerance)
                    {
                        progress = math.min(1f, currentValue / TargetValue);
                    }
                    else if (TargetValue < -Tolerance)
                    {
                        progress = currentValue >= TargetValue - Tolerance
                            ? 1f
                            : math.max(0f, 1f + (currentValue / math.abs(TargetValue)));
                    }
                    else
                    {
                        progress = conditionMet ? 1f : 0f;
                    }

                    break;

                case ECheckType.GreaterThan:
                    conditionMet = currentValue > TargetValue + Tolerance;
                    if (TargetValue > Tolerance)
                    {
                        progress = math.min(1f, currentValue / (TargetValue + Tolerance));
                    }
                    else if (TargetValue < -Tolerance)
                    {
                        progress = currentValue > TargetValue + Tolerance
                            ? 1f
                            : math.max(0f, 1f + (currentValue / math.abs(TargetValue)));
                    }
                    else
                    {
                        progress = conditionMet ? 1f : 0f;
                    }

                    break;

                case ECheckType.LessOrEqual:
                    conditionMet = currentValue <= TargetValue + Tolerance;
                    if (conditionMet)
                    {
                        progress = 1f;
                    }
                    else
                    {
                        float overshoot = currentValue - TargetValue;
                        progress = math.max(0f, 1f - (overshoot / math.max(Tolerance, math.abs(TargetValue))));
                    }

                    break;

                case ECheckType.LessThan:
                    conditionMet = currentValue < TargetValue - Tolerance;
                    if (conditionMet)
                    {
                        progress = 1f;
                    }
                    else
                    {
                        float overshoot = currentValue - TargetValue + Tolerance;
                        progress = math.max(0f, 1f - (overshoot / math.max(Tolerance, math.abs(TargetValue))));
                    }

                    break;

                case ECheckType.Equals:
                    conditionMet = math.abs(currentValue - TargetValue) <= Tolerance;
                    if (math.abs(TargetValue) <= Tolerance)
                    {
                        progress = conditionMet ? 1f : math.max(0f, 1f - (math.abs(currentValue) / (Tolerance * 10)));
                    }
                    else
                    {
                        float distance = math.abs(currentValue - TargetValue);
                        progress = math.max(0f, 1f - (distance / math.abs(TargetValue)));
                        if (conditionMet) progress = 1f;
                    }

                    break;

                case ECheckType.NotEqual:
                    conditionMet = math.abs(currentValue - TargetValue) > Tolerance;
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