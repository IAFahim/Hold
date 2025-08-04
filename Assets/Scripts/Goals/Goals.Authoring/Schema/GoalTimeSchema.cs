using BovineLabs.Core.ObjectManagement;
using Goals.Goals.Authoring.Settings;
using Goals.Goals.Data.Enum;
using Goals.Goals.Data.Goals;
using UnityEngine;

namespace Goals.Goals.Authoring.Schema
{
    [CreateAssetMenu(menuName = "Hold/Goal/Create " + nameof(GoalTimeSchema), fileName = nameof(GoalTimeSchema))]
    [AutoRef(nameof(GoalTimeSettings), nameof(GoalTimeSettings.schemas), nameof(GoalTimeSchema),
        "Goal/" + nameof(GoalTimeSchema), false)]
    public class GoalTimeSchema : GoalSchema<GoalTime>
    {
        public ECheckType checkType;
        public float targetValue;

        public override GoalTime ToData()
        {
            return new GoalTime
            {
                id = (ushort)ID,
                checkType = checkType,
                targetValue = targetValue
            };
        }
    }
}