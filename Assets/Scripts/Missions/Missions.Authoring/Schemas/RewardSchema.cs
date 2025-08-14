
using BovineLabs.Core.ObjectManagement;
using Missions.Missions.Authoring.Scriptable;
using Missions.Missions.Authoring.Settings;
using UnityEngine;

namespace Missions.Missions.Authoring.Schemas
{
    [CreateAssetMenu(menuName = "Hold/" + TypeString + "/Create " + FieldName, fileName = FieldName)]
    [
        AutoRef(
            nameof(RewardSettings), nameof(RewardSettings.schemas),
            FieldName, "Schemas/" + TypeString + "/" + FieldName
        )
    ]
    public class RewardSchema : BakingSchema<Reward>
    {
        private const string FieldName = nameof(RewardSchema);
        private const string TypeString = "Reward";

        public BaseSchema crossLink;
        public DataContainerSchema dataContainer;

        public override Reward ToData()
        {
            return new Reward
            {
                id = (ushort)ID,
                crossLinkType = crossLink.ToCrossLinkType(),
                crossLinkId = (ushort)crossLink.ID,
                dataContainerId = (ushort)dataContainer.ID
            };
        }
    }
}
