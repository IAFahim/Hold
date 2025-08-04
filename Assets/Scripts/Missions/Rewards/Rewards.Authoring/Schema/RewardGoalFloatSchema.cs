
using BovineLabs.Core.ObjectManagement;
using BovineLabs.Essence.Authoring;
using Rewards.Rewards.Authoring.Settings;
using Rewards.Rewards.Data;
using Rewards.Rewards.Data.GoalReward;
using UnityEngine;

namespace Rewards.Rewards.Authoring.Schema
{
    [CreateAssetMenu(menuName = "Hold/" + TypeString + "/Create " + FieldName, fileName = FieldName)]
    [
        AutoRef(nameof(RewardGoalFloatSettings), nameof(RewardGoalFloatSettings.schemas),
            FieldName, TypeString + "/" + FieldName, createNull: false
        )
    ]
    public class RewardGoalFloatSchema : RewardSchema<GoalRewardFloat>
    {
        private const string FieldName = nameof(RewardGoalFloatSchema);

        public StatSchemaObject schemaObject;
        public ERewardGoalType rewardGoalType;
        public float reward;

        public override GoalRewardFloat ToData()
        {
            return new GoalRewardFloat
            {
                id = (ushort)ID,
                statKey = schemaObject.Key,
                rewardGoalType = rewardGoalType,
                reward = reward,
            };
        }
    }
}
