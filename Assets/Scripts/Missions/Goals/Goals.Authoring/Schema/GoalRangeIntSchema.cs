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
            nameof(GoalIntRangeSettings), nameof(GoalIntRangeSettings.schemas), 
            FieldName, TypeString + "/" + FieldName, createNull: false
            )
    ]
    public class GoalRangeIntSchema : GoalSchema<GoalRangeInt>
    {
        private const string FieldName = nameof(GoalRangeIntSchema);

        public IntrinsicSchemaObject schemaObject;
        public ECheckType rangeCheckType;
        public int lowerLimit;
        public int upperLimit;

        public override GoalRangeInt ToData()
        {
            return new GoalRangeInt
            {
                id = (ushort)ID,
                key = schemaObject.Key,
                checkType = rangeCheckType,
                lowerLimit = lowerLimit,
                upperLimit = upperLimit
            };
        }
    }
}