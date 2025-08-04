using System;
using System.Runtime.CompilerServices;
using Goals.Goals.Data.Enum;
using Unity.Burst;
using Unity.Mathematics;

namespace Goals.Goals.Data.Goals
{
    [BurstCompile]
    [Serializable]
    public struct GoalInt
    {
        public ushort id;

        public ushort ID
        {
            readonly get => id;
            set => id = value;
        }
        
        public ushort goalKey;

        /// <summary>
        /// The type of check to perform
        /// </summary>
        public ECheckType checkType;

        /// <summary>
        /// The target/expected value
        /// </summary>
        public int targetValue;


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

            switch (checkType)
            {
                case ECheckType.GreaterOrEqual:
                    conditionMet = currentValue >= targetValue;
                    if (targetValue > 0)
                    {
                        progress = math.min(1f, (float)currentValue / targetValue);
                    }
                    else if (targetValue < 0)
                    {
                        // Handle negative targets
                        progress = currentValue >= targetValue
                            ? 1f
                            : math.max(0f, 1f + ((float)currentValue / math.abs(targetValue)));
                    }
                    else
                    {
                        progress = conditionMet ? 1f : 0f;
                    }

                    break;

                case ECheckType.GreaterThan:
                    conditionMet = currentValue > targetValue;
                    if (targetValue > 0)
                    {
                        progress = math.min(1f, (float)currentValue / (targetValue + 1));
                    }
                    else if (targetValue < 0)
                    {
                        progress = currentValue > targetValue
                            ? 1f
                            : math.max(0f, 1f + ((float)currentValue / math.abs(targetValue)));
                    }
                    else
                    {
                        progress = conditionMet ? 1f : 0f;
                    }

                    break;

                case ECheckType.LessOrEqual:
                    conditionMet = currentValue <= targetValue;
                    if (conditionMet)
                    {
                        progress = 1f;
                    }
                    else
                    {
                        int overshoot = currentValue - targetValue;
                        progress = math.max(0f, 1f - ((float)overshoot / math.max(1, math.abs(targetValue))));
                    }

                    break;

                case ECheckType.LessThan:
                    conditionMet = currentValue < targetValue;
                    if (conditionMet)
                    {
                        progress = 1f;
                    }
                    else
                    {
                        int overshoot = currentValue - targetValue + 1;
                        progress = math.max(0f, 1f - ((float)overshoot / math.max(1, math.abs(targetValue))));
                    }

                    break;

                case ECheckType.Equals:
                    conditionMet = currentValue == targetValue;
                    if (targetValue == 0)
                    {
                        progress = conditionMet ? 1f : math.max(0f, 1f - math.abs(currentValue) * 0.1f);
                    }
                    else
                    {
                        int distance = math.abs(currentValue - targetValue);
                        progress = math.max(0f, 1f - ((float)distance / math.abs(targetValue)));
                        if (conditionMet) progress = 1f;
                    }

                    break;

                case ECheckType.NotEqual:
                    conditionMet = currentValue != targetValue;
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