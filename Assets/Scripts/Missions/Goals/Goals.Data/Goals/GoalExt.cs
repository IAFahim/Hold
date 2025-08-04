using System.Runtime.CompilerServices;
using Goals.Goals.Data.Enum;
using Unity.Burst;
using Unity.Mathematics;

namespace Goals.Goals.Data.Goals
{
    [BurstCompile]
    public static class GoalExt
    {
        /// <summary>
        /// Evaluates the goal condition and calculates progress
        /// </summary>
        /// <param name="currentValue">The current value to check</param>
        /// <param name="lowerLimit">the target value to calculate progress against</param>
        /// <param name="progress">Progress as a value between 0.0 and 1.0</param>
        /// <param name="checkType">GreaterOrEqual, GreaterThan, LessOrEqual, LessThan, Equals, NotEqual</param>
        /// <returns>True if the goal condition is met, false otherwise</returns>
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryProgress(this ECheckType checkType, int currentValue, int lowerLimit, int upperLimit,
            out float progress)
        {
            progress = 0f;
            bool conditionMet;

            switch (checkType)
            {
                case ECheckType.GreaterOrEqual:
                    conditionMet = currentValue >= lowerLimit;
                    if (lowerLimit > 0)
                    {
                        progress = math.min(1f, (float)currentValue / lowerLimit);
                    }
                    else if (lowerLimit < 0)
                    {
                        // Handle negative targets
                        progress = currentValue >= lowerLimit
                            ? 1f
                            : math.max(0f, 1f + ((float)currentValue / math.abs(lowerLimit)));
                    }
                    else
                    {
                        progress = conditionMet ? 1f : 0f;
                    }

                    break;

                case ECheckType.GreaterThan:
                    conditionMet = currentValue > lowerLimit;
                    if (lowerLimit > 0)
                    {
                        progress = math.min(1f, (float)currentValue / (lowerLimit + 1));
                    }
                    else if (lowerLimit < 0)
                    {
                        progress = currentValue > lowerLimit
                            ? 1f
                            : math.max(0f, 1f + ((float)currentValue / math.abs(lowerLimit)));
                    }
                    else
                    {
                        progress = conditionMet ? 1f : 0f;
                    }

                    break;

                case ECheckType.LessOrEqual:
                    conditionMet = currentValue <= lowerLimit;
                    if (conditionMet)
                    {
                        progress = 1f;
                    }
                    else
                    {
                        int overshoot = currentValue - lowerLimit;
                        progress = math.max(0f, 1f - ((float)overshoot / math.max(1, math.abs(lowerLimit))));
                    }

                    break;

                case ECheckType.LessThan:
                    conditionMet = currentValue < lowerLimit;
                    if (conditionMet)
                    {
                        progress = 1f;
                    }
                    else
                    {
                        int overshoot = currentValue - lowerLimit + 1;
                        progress = math.max(0f, 1f - ((float)overshoot / math.max(1, math.abs(lowerLimit))));
                    }

                    break;

                case ECheckType.Equals:
                    conditionMet = currentValue == lowerLimit;
                    if (lowerLimit == 0)
                    {
                        progress = conditionMet ? 1f : math.max(0f, 1f - math.abs(currentValue) * 0.1f);
                    }
                    else
                    {
                        int distance = math.abs(currentValue - lowerLimit);
                        progress = math.max(0f, 1f - ((float)distance / math.abs(lowerLimit)));
                        if (conditionMet) progress = 1f;
                    }

                    break;

                case ECheckType.NotEqual:
                    conditionMet = currentValue != lowerLimit;
                    progress = conditionMet ? 1f : 0f;
                    break;

                case ECheckType.Between:
                    conditionMet = currentValue >= lowerLimit && currentValue <= upperLimit;
                    if (upperLimit > lowerLimit)
                    {
                        if (currentValue < lowerLimit)
                        {
                            int distance = lowerLimit - currentValue;
                            int rangeSize = upperLimit - lowerLimit;
                            progress = math.max(0f, 1f - ((float)distance / rangeSize));
                        }
                        else if (currentValue > upperLimit)
                        {
                            int distance = currentValue - upperLimit;
                            int rangeSize = upperLimit - lowerLimit;
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

                case ECheckType.NotBetween:
                    conditionMet = currentValue < lowerLimit || currentValue > upperLimit;
                    if (upperLimit > lowerLimit)
                    {
                        if (currentValue < lowerLimit)
                        {
                            int distance = lowerLimit - currentValue;
                            int rangeSize = upperLimit - lowerLimit;
                            progress = math.min(1f, (float)distance / rangeSize);
                        }
                        else if (currentValue > upperLimit)
                        {
                            int distance = currentValue - upperLimit;
                            int rangeSize = upperLimit - lowerLimit;
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


        /// <summary>
        /// Tolerance for floating-point comparisons
        /// </summary>
        public const float Tolerance = math.EPSILON;

        /// <summary>
        /// Evaluates the goal condition and calculates progress
        /// </summary>
        /// <param name="currentValue">The current value to check</param>
        /// <param name="lowerLimit">the target value to calculate progress against</param>
        /// <param name="upperLimit">for Between, NotBetween</param>
        /// <param name="progress">Progress as a value between 0.0 and 1.0</param>
        /// <param name="checkType">GreaterOrEqual, GreaterThan, LessOrEqual, LessThan, Equals, NotEqual, Between*, NotBetween*</param>
        /// <returns>True if the goal condition is met, false otherwise</returns>
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryProgress(this ECheckType checkType, float currentValue, float lowerLimit, float upperLimit,
            out float progress)
        {
            progress = 0f;
            bool conditionMet;

            switch (checkType)
            {
                case ECheckType.GreaterOrEqual:
                    conditionMet = currentValue >= lowerLimit - Tolerance;
                    if (lowerLimit > Tolerance)
                    {
                        progress = math.min(1f, currentValue / lowerLimit);
                    }
                    else if (lowerLimit < -Tolerance)
                    {
                        progress = currentValue >= lowerLimit - Tolerance
                            ? 1f
                            : math.max(0f, 1f + (currentValue / math.abs(lowerLimit)));
                    }
                    else
                    {
                        progress = conditionMet ? 1f : 0f;
                    }

                    break;

                case ECheckType.GreaterThan:
                    conditionMet = currentValue > lowerLimit + Tolerance;
                    if (lowerLimit > Tolerance)
                    {
                        progress = math.min(1f, currentValue / (lowerLimit + Tolerance));
                    }
                    else if (lowerLimit < -Tolerance)
                    {
                        progress = currentValue > lowerLimit + Tolerance
                            ? 1f
                            : math.max(0f, 1f + (currentValue / math.abs(lowerLimit)));
                    }
                    else
                    {
                        progress = conditionMet ? 1f : 0f;
                    }

                    break;

                case ECheckType.LessOrEqual:
                    conditionMet = currentValue <= lowerLimit + Tolerance;
                    if (conditionMet)
                    {
                        progress = 1f;
                    }
                    else
                    {
                        float overshoot = currentValue - lowerLimit;
                        progress = math.max(0f, 1f - (overshoot / math.max(Tolerance, math.abs(lowerLimit))));
                    }

                    break;

                case ECheckType.LessThan:
                    conditionMet = currentValue < lowerLimit - Tolerance;
                    if (conditionMet)
                    {
                        progress = 1f;
                    }
                    else
                    {
                        float overshoot = currentValue - lowerLimit + Tolerance;
                        progress = math.max(0f, 1f - (overshoot / math.max(Tolerance, math.abs(lowerLimit))));
                    }

                    break;

                case ECheckType.Equals:
                    conditionMet = math.abs(currentValue - lowerLimit) <= Tolerance;
                    if (math.abs(lowerLimit) <= Tolerance)
                    {
                        progress = conditionMet ? 1f : math.max(0f, 1f - (math.abs(currentValue) / (Tolerance * 10)));
                    }
                    else
                    {
                        float distance = math.abs(currentValue - lowerLimit);
                        progress = math.max(0f, 1f - (distance / math.abs(lowerLimit)));
                        if (conditionMet) progress = 1f;
                    }

                    break;

                case ECheckType.NotEqual:
                    conditionMet = math.abs(currentValue - lowerLimit) > Tolerance;
                    progress = conditionMet ? 1f : 0f;
                    break;
                
                case ECheckType.Between:
                    conditionMet = currentValue >= lowerLimit - Tolerance && currentValue <= upperLimit + Tolerance;
                    if (upperLimit > lowerLimit)
                    {
                        if (currentValue < lowerLimit - Tolerance)
                        {
                            float distance = lowerLimit - currentValue;
                            float rangeSize = upperLimit - lowerLimit;
                            progress = math.max(0f, 1f - (distance / rangeSize));
                        }
                        else if (currentValue > upperLimit + Tolerance)
                        {
                            float distance = currentValue - upperLimit;
                            float rangeSize = upperLimit - lowerLimit;
                            progress = math.max(0f, 1f - (distance / rangeSize));
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

                case ECheckType.NotBetween:
                    conditionMet = currentValue < lowerLimit - Tolerance || currentValue > upperLimit + Tolerance;
                    if (upperLimit > lowerLimit)
                    {
                        if (currentValue < lowerLimit - Tolerance)
                        {
                            float distance = lowerLimit - currentValue;
                            float rangeSize = upperLimit - lowerLimit;
                            progress = math.min(1f, distance / rangeSize);
                        }
                        else if (currentValue > upperLimit + Tolerance)
                        {
                            float distance = currentValue - upperLimit;
                            float rangeSize = upperLimit - lowerLimit;
                            progress = math.min(1f, distance / rangeSize);
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