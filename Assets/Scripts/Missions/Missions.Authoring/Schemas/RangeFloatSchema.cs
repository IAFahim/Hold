using BovineLabs.Core.ObjectManagement;
using Missions.Missions.Authoring.Scriptable;
using Missions.Missions.Authoring.Settings;
using UnityEngine;

namespace Missions.Missions.Authoring.Schemas
{
    [CreateAssetMenu(menuName = "Hold/" + TypeString + "/Create " + FieldName, fileName = FieldName)]
    [
        AutoRef(
            nameof(RangeFloatSettings), nameof(RangeFloatSettings.schemas),
            FieldName, "Schemas/" + TypeString + "/" + FieldName
        )
    ]
    public class RangeFloatSchema : BakingSchema<RangeFloat>
    {
        private const string FieldName = nameof(RangeFloatSchema);
        private const string TypeString = "RangeFloat";

        public ECheckType checkType;
        public float min;
        public float max;

        public override RangeFloat ToData()
        {
            return new RangeFloat
            {
                id = (ushort)ID,
                checkType = checkType,
                min = min,
                max = max
            };
        }
    }
}
