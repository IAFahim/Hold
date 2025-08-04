using BovineLabs.Reaction.Authoring.Conditions;
using Goals.Goals.Data.Enum;
using Goals.Goals.Data.Goals;
using UnityEngine;

namespace Goals.Goals.Authoring.Schema
{
    [CreateAssetMenu(menuName = "Hold/Goal/Create GoalIntData", fileName = "GoalIntData")]
    public class GoalIntSchema : GoalSchema<GoalInt>
    {
        public ConditionSchemaObject conditionSchemaObject;
        public ECheckType checkType;
        public int targetValue;

        public override GoalInt ToGoal()
        {
            return new GoalInt
            {
                id = (ushort)ID,
                goalKey = conditionSchemaObject.Key,
                checkType = checkType,
                targetValue = targetValue,
            };
        }
    }
}