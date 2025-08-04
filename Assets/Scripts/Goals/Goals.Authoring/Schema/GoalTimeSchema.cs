using BovineLabs.Core.ObjectManagement;
using Goals.Goals.Authoring.Settings;
using Goals.Goals.Data.Enum;
using Goals.Goals.Data.Goals;
using UnityEngine;

namespace Goals.Goals.Authoring.Schema
{
    [CreateAssetMenu(menuName = "Hold/" + TypeString + "/Create " + FieldName, fileName = FieldName)]
    [
        AutoRef(
            nameof(GoalTimeSettings), nameof(GoalTimeSettings.schemas), 
            FieldName, "Goal/" + FieldName, createNull: false
        )
    ]
    public class GoalTimeSchema : GoalSchema<GoalTime>
    {
        private const string FieldName = nameof(GoalTimeSchema);
        public ECheckType checkType = ECheckType.LessOrEqual;
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