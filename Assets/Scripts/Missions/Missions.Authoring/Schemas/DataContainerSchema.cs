using BovineLabs.Core.ObjectManagement;
using Missions.Missions.Authoring.Scriptable;
using Missions.Missions.Authoring.Settings;
using UnityEngine;

namespace Missions.Missions.Authoring.Schemas
{
    [CreateAssetMenu(menuName = "Hold/" + TypeString + "/Create " + FieldName, fileName = FieldName)]
    [
        AutoRef(
            nameof(DataContainerSettings), nameof(DataContainerSettings.schemas),
            FieldName, "Schemas/" + TypeString + "/" + FieldName
        )
    ]
    public class DataContainerSchema : BakingSchema<DataContainer>
    {
        private const string FieldName = nameof(DataContainerSchema);
        private const string TypeString = "DataContainer";

        public ETargetType targetType;
        public ENumType numType;
        public float valueFloat;
        public int valueInt;

        public override DataContainer ToData()
        {
            return new DataContainer
            {
                id = (ushort)ID,
                targetType = targetType,
                numType = numType,
                valueFloat = valueFloat,
                valueInt = valueInt
            };
        }
    }
}