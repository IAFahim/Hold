using BovineLabs.Core.ObjectManagement;
using Missions.Missions.Authoring.Scriptable;
using Missions.Missions.Authoring.Settings;
using Unity.Mathematics;
using UnityEngine;

namespace Missions.Missions.Authoring.Schemas
{
    [CreateAssetMenu(menuName = "Hold/" + TypeString + "/Create " + FieldName, fileName = FieldName)]
    [
        AutoRef(
            nameof(LocationSettings), nameof(LocationSettings.schemas),
            FieldName, TypeString + "/" + FieldName
        )
    ]
    public class LocationSchema : BakingSchema<Location>
    {
        private const string FieldName = nameof(LocationSchema);
        private const string TypeString = "Station";

        public NameSchema nameSchema;
        public float3 position;
        public float range;

        public override Location ToData()
        {
            return new Location
            {
                id = (ushort)ID,
                nameId = (ushort)nameSchema.ID,
                position = position,
                range = range
            };
        }
    }
}