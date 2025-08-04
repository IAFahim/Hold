using BovineLabs.Core.ObjectManagement;
using BovineLabs.Essence.Authoring;
using Goals.Goals.Authoring.Settings;
using Goals.Goals.Data.Enum;
using Goals.Goals.Data.Goals;
using UnityEngine;

namespace Goals.Goals.Authoring.Schema
{
    [CreateAssetMenu(menuName = "Hold/" + TypeString + "/Create " + FieldName, fileName = FieldName)]
    [
        AutoRef(
            nameof(GoalFloatSettings), nameof(GoalFloatSettings.schemas), 
            FieldName, TypeString + "/" + FieldName, createNull: false
        )
    ]
    public class GoalFloatSchema : GoalSchema<GoalFloat>
    {
        private const string FieldName = nameof(GoalFloatSchema);


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