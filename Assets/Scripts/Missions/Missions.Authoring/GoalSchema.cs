using System;
using BovineLabs.Core.ObjectManagement;
using BovineLabs.Core.Settings;
using Data;
using UnityEngine;

namespace Missions.Missions.Authoring
{
    [CreateAssetMenu(menuName = "Hold/" + TypeString + "/Create " + FieldName, fileName = FieldName)]
    [
        AutoRef(
            nameof(GoalSettings), nameof(GoalSettings.schemas),
            FieldName, TypeString + "/" + FieldName, createNull: false, defaultFileName: "Goal Range Int"
        )
    ]
    public class GoalSchema : BakingSchema<Goal>
    {
        private const string FieldName = nameof(GoalSchema);
        private const string TypeString = "Goals";

        public TargetType targetType;
        public BaseSchema rangeSchema;

        public override Goal ToData()
        {
            return new Goal
            {
                id = (ushort)ID,
                rangeType = rangeSchema.ToNumType(),
                targetType = targetType,
                rangeId = (ushort)rangeSchema.ID
            };
        }
    }

    public class GoalSettings : ScriptableObject, ISettings
    {
        public GoalSchema[] schemas = Array.Empty<GoalSchema>();
    }
}