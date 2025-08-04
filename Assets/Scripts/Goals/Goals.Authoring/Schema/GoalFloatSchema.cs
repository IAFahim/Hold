using BovineLabs.Core.ObjectManagement;
using BovineLabs.Essence.Authoring;
using Goals.Goals.Authoring.Settings;
using Goals.Goals.Data.Enum;
using Goals.Goals.Data.Goals;
using UnityEngine;

namespace Goals.Goals.Authoring.Schema
{
    [CreateAssetMenu(menuName = "Hold/Goal/Create " + nameof(GoalFloatSchema), fileName = nameof(GoalFloatSchema))]
    [AutoRef(nameof(GoalFloatSettings), nameof(GoalFloatSettings.schemas), nameof(GoalFloatSchema),
        "Goal/" + nameof(GoalFloatSchema))]
    public class GoalFloatSchema : GoalSchema<GoalFloat>
    {
        public StatSchemaObject schemaObject;
        public ECheckType checkType;
        public float targetValue;

        public override GoalFloat ToData()
        {
            return new GoalFloat
            {
                id = (ushort)ID,
                goalKey = schemaObject.Key,
                checkType = checkType,
                targetValue = targetValue
            };
        }
    }
}