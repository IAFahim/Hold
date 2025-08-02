using System;
using Goals.Goals.Data.Enum;

namespace Goals.Goals.Data.GoalTable
{
    public struct GoalTableFloat
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
        /// Optional secondary value for range checks (Between/NotBetween)
        /// </summary>
        public float UpperLimit;

        /// <summary>
        /// Tolerance for floating-point comparisons
        /// </summary>
        public float Tolerance;

        /// <summary>
        /// Evaluates the goal condition and calculates progress
        /// </summary>
        /// <param name="currentValue">The current value to check</param>
        /// <param name="progress">Progress as a value between 0.0 and 1.0</param>
        /// <returns>True if the goal condition is met, false otherwise</returns>
        public bool TryProgress(float currentValue, out float progress)
        {
            progress = 0f;
            bool conditionMet = false;
            float tolerance = Tolerance > 0 ? Tolerance : 0.001f;

            switch (CheckType)
            {
                case ECheckType.GreaterOrEqual:
                    conditionMet = currentValue >= TargetValue - tolerance;
                    if (TargetValue > tolerance)
                    {
                        progress = Math.Min(1f, currentValue / TargetValue);
                    }
                    else if (TargetValue < -tolerance)
                    {
                        progress = currentValue >= TargetValue - tolerance
                            ? 1f
                            : Math.Max(0f, 1f + (currentValue / Math.Abs(TargetValue)));
                    }
                    else
                    {
                        progress = conditionMet ? 1f : 0f;
                    }

                    break;

                case ECheckType.GreaterThan:
                    conditionMet = currentValue > TargetValue + tolerance;
                    if (TargetValue > tolerance)
                    {
                        progress = Math.Min(1f, currentValue / (TargetValue + tolerance));
                    }
                    else if (TargetValue < -tolerance)
                    {
                        progress = currentValue > TargetValue + tolerance
                            ? 1f
                            : Math.Max(0f, 1f + (currentValue / Math.Abs(TargetValue)));
                    }
                    else
                    {
                        progress = conditionMet ? 1f : 0f;
                    }

                    break;

                case ECheckType.LessOrEqual:
                    conditionMet = currentValue <= TargetValue + tolerance;
                    if (currentValue <= TargetValue + tolerance)
                    {
                        progress = 1f;
                    }
                    else
                    {
                        float overshoot = currentValue - TargetValue;
                        progress = Math.Max(0f, 1f - (overshoot / Math.Max(tolerance, Math.Abs(TargetValue))));
                    }

                    break;

                case ECheckType.LessThan:
                    conditionMet = currentValue < TargetValue - tolerance;
                    if (currentValue < TargetValue - tolerance)
                    {
                        progress = 1f;
                    }
                    else
                    {
                        float overshoot = currentValue - TargetValue + tolerance;
                        progress = Math.Max(0f, 1f - (overshoot / Math.Max(tolerance, Math.Abs(TargetValue))));
                    }

                    break;

                case ECheckType.Equals:
                    conditionMet = Math.Abs(currentValue - TargetValue) <= tolerance;
                    if (Math.Abs(TargetValue) <= tolerance)
                    {
                        progress = conditionMet ? 1f : Math.Max(0f, 1f - (Math.Abs(currentValue) / (tolerance * 10)));
                    }
                    else
                    {
                        float distance = Math.Abs(currentValue - TargetValue);
                        progress = Math.Max(0f, 1f - (distance / Math.Abs(TargetValue)));
                        if (conditionMet) progress = 1f;
                    }

                    break;

                case ECheckType.NotEqual:
                    conditionMet = Math.Abs(currentValue - TargetValue) > tolerance;
                    progress = conditionMet ? 1f : 0f;
                    break;

                case ECheckType.Between:
                    conditionMet = currentValue >= TargetValue - tolerance && currentValue <= UpperLimit + tolerance;
                    if (UpperLimit > TargetValue)
                    {
                        if (currentValue < TargetValue - tolerance)
                        {
                            float distance = TargetValue - currentValue;
                            float rangeSize = UpperLimit - TargetValue;
                            progress = Math.Max(0f, 1f - (distance / rangeSize));
                        }
                        else if (currentValue > UpperLimit + tolerance)
                        {
                            float distance = currentValue - UpperLimit;
                            float rangeSize = UpperLimit - TargetValue;
                            progress = Math.Max(0f, 1f - (distance / rangeSize));
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
                    conditionMet = currentValue < TargetValue - tolerance || currentValue > UpperLimit + tolerance;
                    if (UpperLimit > TargetValue)
                    {
                        if (currentValue < TargetValue - tolerance)
                        {
                            float distance = TargetValue - currentValue;
                            float rangeSize = UpperLimit - TargetValue;
                            progress = Math.Min(1f, distance / rangeSize);
                        }
                        else if (currentValue > UpperLimit + tolerance)
                        {
                            float distance = currentValue - UpperLimit;
                            float rangeSize = UpperLimit - TargetValue;
                            progress = Math.Min(1f, distance / rangeSize);
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