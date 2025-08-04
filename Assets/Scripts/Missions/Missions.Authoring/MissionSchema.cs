using System.Linq;
using BovineLabs.Core.ObjectManagement;
using Goals.Goals.Authoring.Schema;
using Maps.Maps.Data;
using Rewards.Rewards.Authoring.Schema;
using SchemaSettings.SchemaSettings.Authoring;
using UnityEngine;

namespace Missions.Missions.Authoring
{
    [CreateAssetMenu(menuName = "Hold/" + TypeString + "/Create " + FieldName, fileName = FieldName)]
    [
        AutoRef(
            nameof(MissionSettings), nameof(MissionSettings.schemas),
            FieldName, TypeString + "/" + FieldName
        )
    ]
    public class MissionSchema : BaseSchema<Mission>
    {
        private const string FieldName = nameof(MissionSchema);
        private const string TypeString = "Mission";

        public Segment segment;
        public GoalRangeIntSchema[] goalRangeInts;
        public GoalRangeFloatSchema[] goalRangeFloats;
        public RewardGoalIntSchema[] rewardInts;
        public RewardGoalFloatSchema[] rewardFloats;

        public override Mission ToData()
        {
            return new Mission
            {
                id = (ushort)ID,
                segment = segment,
                goalRangeInts = goalRangeInts.Select(g => g.ToData()).ToArray(),
                goalRangeFloats = goalRangeFloats.Select(g => g.ToData()).ToArray(),
                rewardInts = rewardInts.Select(g => g.ToData()).ToArray(),
                rewardFloats = rewardFloats.Select(g => g.ToData()).ToArray(),
            };
        }
    }
}