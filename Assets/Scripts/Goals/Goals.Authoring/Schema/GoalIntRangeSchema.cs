using BovineLabs.Essence.Authoring;
using Goals.Goals.Data.Enum;
using Goals.Goals.Data.Goals;
using UnityEngine;

namespace Goals.Goals.Authoring.Schema
{
    [CreateAssetMenu(menuName = "Hold/Goal/Create GoalIntRangeData", fileName = "GoalIntRangeData")]
    public class GoalIntRangeSchema : GoalSchema<GoalRangeInt>
    {
        public IntrinsicSchemaObject schemaObject;
        public ERangeCheckType rangeCheckType;
        public int lowerLimit;
        public int upperLimit;

        public override GoalRangeInt ToGoal()
        {
            return new GoalRangeInt
            {
                id = (ushort)ID,
                goalKey = schemaObject.Key,
                checkType = rangeCheckType,
                lowerLimit = lowerLimit,
                upperLimit = upperLimit
            };
        }
    }
}