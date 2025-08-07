using BovineLabs.Core.ObjectManagement;
using Missions.Missions.Authoring.Scriptable;
using Missions.Missions.Authoring.Settings;
using Unity.Collections;
using UnityEngine;

namespace Missions.Missions.Authoring.Schemas
{
    [CreateAssetMenu(menuName = "Hold/" + TypeString + "/Create " + FieldName, fileName = FieldName)]
    [
        AutoRef(
            nameof(DescriptionSettings), nameof(DescriptionSettings.schemas),
            FieldName, TypeString + "/" + FieldName
        )
    ]
    public class DescriptionSchema : BakingSchema<Description>
    {
        private const string FieldName = nameof(DescriptionSchema);
        private const string TypeString = "Name";

        public string fixed64;

        private void OnValidate()
        {
            FixedString32Bytes test = fixed64;
        }

        public override Description ToData()
        {
            return new Description
            {
                id = (ushort)ID,
                description = fixed64
            };
        }
    }
}