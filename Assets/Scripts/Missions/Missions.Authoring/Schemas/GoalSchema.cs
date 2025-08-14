using BovineLabs.Core.ObjectManagement;
using Missions.Missions.Authoring.Scriptable;
using Missions.Missions.Authoring.Settings;
using UnityEngine;
using UnityEngine.Search;

namespace Missions.Missions.Authoring.Schemas
{
    [CreateAssetMenu(menuName = "Hold/" + TypeString + "/Create " + FieldName, fileName = FieldName)]
    [
        AutoRef(
            nameof(GoalSettings), nameof(GoalSettings.schemas),
            FieldName, "Schemas/" + TypeString + "/" + FieldName
        )
    ]
    public class GoalSchema : BakingSchema<Goal>
    {
        private const string FieldName = nameof(GoalSchema);
        private const string TypeString = "Goals";

        public ETargetType targetType;
        [SearchContext("range")]
        public BaseSchema rangeSchema;

        public override Goal ToData()
        {
            return new Goal
            {
                id = (ushort)ID,
                rangeType = rangeSchema.ToRangeType(),
                targetType = targetType,
                rangeId = (ushort)rangeSchema.ID
            };
        }
    }
}