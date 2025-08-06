using BovineLabs.Core.ObjectManagement;
using Data;
using Missions.Missions.Authoring.Settings;
using UnityEngine;

namespace Missions.Missions.Authoring.Schemas
{
    [CreateAssetMenu(menuName = "Hold/" + TypeString + "/Create " + FieldName, fileName = FieldName)]
    [
        AutoRef(
            nameof(RangeFloatSettings), nameof(RangeFloatSettings.schemas),
            FieldName, TypeString + "/" + FieldName
        )
    ]
    public class RangeFloatSchema : BakingSchema<RangeFloat>
    {
        private const string FieldName = nameof(RangeFloatSchema);
        private const string TypeString = "RangeFloat";

        public CheckType checkType;
        public float lower;
        public float upper;

        public override RangeFloat ToData()
        {
            return new RangeFloat
            {
                id = (ushort)ID,
                checkType = checkType,
                lower = lower,
                upper = upper
            };
        }
    }
}
