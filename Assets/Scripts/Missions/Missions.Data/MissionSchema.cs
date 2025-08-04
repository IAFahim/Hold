using System.Linq;
using BovineLabs.Core.ObjectManagement;
using Goals.Goals.Authoring.Schema;
using Rewards.Rewards.Data.GoalReward;
using SchemaSettings.SchemaSettings.Authoring;
using UnityEngine;

namespace Maps.Maps.Data
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
        public GoalRewardInt[] rewardInts;
        public GoalRewardFloat[] rewardFloats;

        public override Mission ToData()
        {
            return new Mission
            {
                id = (ushort)ID,
                segment = segment,
                goalRangeInts = goalRangeInts.Select(g => g.ToData()).ToArray(),
                goalRangeFloats = goalRangeFloats.Select(g => g.ToData()).ToArray(),
                rewardInts = rewardInts.Select(g => g).ToArray(),
                rewardFloats = rewardFloats.Select(g => g).ToArray()
            };
        }
    }
}