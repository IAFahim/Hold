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
        AutoRef(nameof(GoalRangeFloatSettings), nameof(GoalRangeFloatSettings.schemas),
            FieldName, TypeString + "/" + FieldName, createNull: false
        )
    ]
    public class GoalRangeFloatSchema : GoalSchema<GoalRangeFloat>
    {
        private const string FieldName = nameof(GoalRangeFloatSchema);

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