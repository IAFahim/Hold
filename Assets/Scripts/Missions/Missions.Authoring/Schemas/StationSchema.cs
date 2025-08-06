using BovineLabs.Core.ObjectManagement;
using Missions.Missions.Authoring.Settings;
using Unity.Mathematics;
using UnityEngine;

namespace Missions.Missions.Authoring.Schemas
{
    [CreateAssetMenu(menuName = "Hold/" + TypeString + "/Create " + FieldName, fileName = FieldName)]
    [
        AutoRef(
            nameof(StationSettings), nameof(StationSettings.schemas),
            FieldName, TypeString + "/" + FieldName
        )
    ]
    public class StationSchema : BakingSchema<Station>
    {
        private const string FieldName = nameof(StationSchema);
        private const string TypeString = "Station";

        public NameSchema nameSchema;
        public float3 position;

        public override Station ToData()
        {
            return new Station
            {
                id = (ushort)ID,
                nameId = (ushort)nameSchema.ID,
                position = position
            };
        }
    }
}