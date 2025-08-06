using BovineLabs.Core.ObjectManagement;
using Data;
using Missions.Missions.Authoring.Settings;
using UnityEngine;
using RangeInt = Data.RangeInt;

namespace Missions.Missions.Authoring.Schemas
{
    [CreateAssetMenu(menuName = "Hold/" + TypeString + "/Create " + FieldName, fileName = FieldName)]
    [
        AutoRef(
            nameof(RangeIntSettings), nameof(RangeIntSettings.schemas),
            FieldName, TypeString + "/" + FieldName
        )
    ]
    public class RangeIntSchema : BakingSchema<RangeInt>
    {
        private const string FieldName = nameof(RangeIntSchema);
        private const string TypeString = "RangeInt";

        public CheckType checkType;
        public int lower;
        public int upper;

        public override RangeInt ToData()
        {
            return new RangeInt
            {
                id = (ushort)ID,
                checkType = checkType,
                lower = lower,
                upper = upper
            };
        }
    }
}
