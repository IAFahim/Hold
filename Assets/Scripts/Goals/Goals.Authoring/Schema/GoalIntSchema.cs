using BovineLabs.Core.ObjectManagement;
using BovineLabs.Reaction.Authoring.Conditions;
using Goals.Goals.Authoring.Settings;
using Goals.Goals.Data.Enum;
using Goals.Goals.Data.Goals;
using UnityEngine;

namespace Goals.Goals.Authoring.Schema
{
    [CreateAssetMenu(menuName = "Hold/" + TypeString + "/Create " + FieldName, fileName = FieldName)]
    [
        AutoRef(
            nameof(GoalIntSettings), nameof(GoalIntSettings.schemas),
            FieldName, TypeString + "/" + FieldName, createNull: false
        )
    ]
    public class GoalIntSchema : GoalSchema<GoalInt>
    {
        private const string FieldName = nameof(GoalIntSchema);

        public ConditionSchemaObject conditionSchemaObject;
        public ECheckType checkType;
        public int targetValue;

        public override GoalInt ToData()
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