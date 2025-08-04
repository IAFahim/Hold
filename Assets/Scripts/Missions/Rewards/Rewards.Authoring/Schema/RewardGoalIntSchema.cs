
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
        AutoRef(nameof(RewardGoalIntSettings), nameof(RewardGoalIntSettings.schemas),
            FieldName, TypeString + "/" + FieldName, createNull: false
        )
    ]
    public class RewardGoalIntSchema : RewardSchema<GoalRewardInt>
    {
        private const string FieldName = nameof(RewardGoalIntSchema);

        public IntrinsicSchemaObject schemaObject;
        public ERewardGoalType rewardGoalType;
        public int reward;

        public override GoalRewardInt ToData()
        {
            return new GoalRewardInt
            {
                id = (ushort)ID,
                key = schemaObject.Key,
                rewardGoalType = rewardGoalType,
                reward = reward,
            };
        }
    }
}
