using BovineLabs.Core.ObjectManagement;
using BovineLabs.Core.PropertyDrawers;
using Missions.Missions.Authoring.Scriptable;
using Missions.Missions.Authoring.Settings;
using Unity.Mathematics;
using UnityEngine;
using RangeInt = Missions.Missions.Authoring.RangeInt;

namespace Missions.Missions.Authoring.Schemas
{
    [CreateAssetMenu(menuName = "Hold/" + TypeString + "/Create " + FieldName, fileName = FieldName)]
    [
        AutoRef(
            nameof(RangeIntSettings), nameof(RangeIntSettings.schemas),
            FieldName, "Schemas/" + TypeString + "/" + FieldName
        )
    ]
    public class RangeIntSchema : BakingSchema<RangeInt>
    {
        private const string FieldName = nameof(RangeIntSchema);
        private const string TypeString = "RangeInt";

        public ECheckType checkType;
        public int min;
        public int max;

        public override RangeInt ToData()
        {
            return new RangeInt
            {
                id = (ushort)ID,
                checkType = checkType,
                min = min,
                max = max
            };
        }
    }
}