using BovineLabs.Essence.Authoring;
using Goals.Goals.Data.Enum;
using Goals.Goals.Data.Goals;
using UnityEngine;

namespace Goals.Goals.Authoring.Schema
{
    [CreateAssetMenu(menuName = "Hold/Goal/Create GoalRangeFloatData", fileName = "GoalRangeFloatData")]
    public class GoalRangeFloatSchema : GoalSchema<GoalRangeFloat>
    {
        public StatSchemaObject schemaObject;
        public ERangeCheckType rangeCheckType;
        public float lowerLimit;
        public float upperLimit;

        public override GoalRangeFloat ToData()
        {
            return new GoalRangeFloat
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