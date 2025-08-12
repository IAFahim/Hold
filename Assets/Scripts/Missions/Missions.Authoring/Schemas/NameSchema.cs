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
            nameof(NameSettings), nameof(NameSettings.schemas),
            FieldName, TypeString + "/" + FieldName
        )
    ]
    public class NameSchema : BakingSchema<Name>
    {
        private const string FieldName = nameof(NameSchema);
        private const string TypeString = "Name";

        public string fixed32;

        private void OnValidate()
        {
            FixedString32Bytes test = fixed32;
        }

        public override Name ToData()
        {
            return new Name
            {
                id = (ushort)ID,
                name = fixed32
            };
        }
    }
}