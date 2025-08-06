using BovineLabs.Core.ObjectManagement;
using Missions.Missions.Authoring.Settings;
using UnityEngine;
using Time = Missions.Missions.Authoring.Time;

namespace Missions.Missions.Authoring.Schemas
{
    [CreateAssetMenu(menuName = "Hold/" + TypeString + "/Create " + FieldName, fileName = FieldName)]
    [
        AutoRef(
            nameof(TimeSettings), nameof(TimeSettings.schemas),
            FieldName, TypeString + "/" + FieldName
        )
    ]
    public class TimeSchema : BakingSchema<Time>
    {
        private const string FieldName = nameof(TimeSchema);
        private const string TypeString = "Time";

        public BaseSchema crossLinkType;

        public override Time ToData()
        {
            return new Time
            {
                id = (ushort)ID,
                crossLinkType = crossLinkType.ToCrossLinkType()
            };
        }
    }
}
