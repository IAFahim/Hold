using System;
using Goals.Goals.Data.Enum;

namespace Goals.Goals.Data.GoalTable
{
    public struct GoalTableInt
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
        /// Optional secondary value for range checks (Between/NotBetween)
        /// </summary>
        public int UpperLimit;

        /// <summary>
        /// Evaluates the goal condition and calculates progress
        /// </summary>
        /// <param name="currentValue">The current value to check</param>
        /// <param name="progress">Progress as a value between 0.0 and 1.0</param>
        /// <returns>True if the goal condition is met, false otherwise</returns>
        public bool TryProgress(int currentValue, out float progress)
        {
            progress = 0f;
            bool conditionMet = false;

            switch (CheckType)
            {
                case ECheckType.GreaterOrEqual:
                    conditionMet = currentValue >= TargetValue;
                    if (TargetValue > 0)
                    {
                        progress = Math.Min(1f, (float)currentValue / TargetValue);
                    }
                    else if (TargetValue < 0)
                    {
                        // Handle negative targets
                        progress = currentValue >= TargetValue
                            ? 1f
                            : Math.Max(0f, 1f + ((float)currentValue / Math.Abs(TargetValue)));
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
                        progress = Math.Min(1f, (float)currentValue / (TargetValue + 1));
                    }
                    else if (TargetValue < 0)
                    {
                        progress = currentValue > TargetValue
                            ? 1f
                            : Math.Max(0f, 1f + ((float)currentValue / Math.Abs(TargetValue)));
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
                        progress = Math.Max(0f, 1f - ((float)overshoot / Math.Max(1, Math.Abs(TargetValue))));
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
                        progress = Math.Max(0f, 1f - ((float)overshoot / Math.Max(1, Math.Abs(TargetValue))));
                    }

                    break;

                case ECheckType.Equals:
                    conditionMet = currentValue == TargetValue;
                    if (TargetValue == 0)
                    {
                        progress = conditionMet ? 1f : Math.Max(0f, 1f - Math.Abs(currentValue) * 0.1f);
                    }
                    else
                    {
                        int distance = Math.Abs(currentValue - TargetValue);
                        progress = Math.Max(0f, 1f - ((float)distance / Math.Abs(TargetValue)));
                        if (conditionMet) progress = 1f;
                    }

                    break;

                case ECheckType.NotEqual:
                    conditionMet = currentValue != TargetValue;
                    progress = conditionMet ? 1f : 0f;
                    break;

                case ECheckType.Between:
                    conditionMet = currentValue >= TargetValue && currentValue <= UpperLimit;
                    if (UpperLimit > TargetValue)
                    {
                        if (currentValue < TargetValue)
                        {
                            int distance = TargetValue - currentValue;
                            int rangeSize = UpperLimit - TargetValue;
                            progress = Math.Max(0f, 1f - ((float)distance / rangeSize));
                        }
                        else if (currentValue > UpperLimit)
                        {
                            int distance = currentValue - UpperLimit;
                            int rangeSize = UpperLimit - TargetValue;
                            progress = Math.Max(0f, 1f - ((float)distance / rangeSize));
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
                    conditionMet = currentValue < TargetValue || currentValue > UpperLimit;
                    if (UpperLimit > TargetValue)
                    {
                        if (currentValue < TargetValue)
                        {
                            int distance = TargetValue - currentValue;
                            int rangeSize = UpperLimit - TargetValue;
                            progress = Math.Min(1f, (float)distance / rangeSize);
                        }
                        else if (currentValue > UpperLimit)
                        {
                            int distance = currentValue - UpperLimit;
                            int rangeSize = UpperLimit - TargetValue;
                            progress = Math.Min(1f, (float)distance / rangeSize);
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

            progress = Math.Clamp(progress, 0f, 1f);
            return conditionMet;
        }
    }
}