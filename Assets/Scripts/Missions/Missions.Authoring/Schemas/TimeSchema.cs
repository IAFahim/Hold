using BovineLabs.Core.ObjectManagement;
using Missions.Missions.Authoring.Scriptable;
using Missions.Missions.Authoring.Settings;
using UnityEngine;

namespace Missions.Missions.Authoring.Schemas
{
    [CreateAssetMenu(menuName = "Hold/" + TypeString + "/Create " + FieldName, fileName = FieldName)]
    [
        AutoRef(
            nameof(TimeSettings), nameof(TimeSettings.schemas),
            FieldName, "Schemas/" + TypeString + "/" + FieldName
        )
    ]
    public class TimeSchema : BakingSchema<TimeStruct>
    {
        private const string FieldName = nameof(TimeSchema);
        private const string TypeString = "Time";
        public BaseSchema crossLink;
        public RangeFloatSchema rangeFloat;
        

        public override TimeStruct ToData()
        {
            return new TimeStruct
            {
                id = (ushort)ID,
                crossLinkType = crossLink.ToCrossLinkType()
            };
        }
    }
}
