using BovineLabs.Core.ObjectManagement;
using Data;
using UnityEngine;

namespace Missions.Missions.Authoring
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
        public override Name ToData()
        {
            return new Name
            {
                id = (ushort)ID,
                name = name
            };
        }
    }
}