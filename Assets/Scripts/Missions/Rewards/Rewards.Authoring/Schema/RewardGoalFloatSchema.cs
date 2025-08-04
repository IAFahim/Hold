
using BovineLabs.Core.ObjectManagement;
using BovineLabs.Essence.Authoring;
using Goals.Goals.Authoring.Schema;
using Rewards.Rewards.Authoring.Settings;
using Rewards.Rewards.Data;
using Rewards.Rewards.Data.GoalReward;
using UnityEngine;

namespace Rewards.Rewards.Authoring.Schema
{
    [CreateAssetMenu(menuName = "Hold/" + TypeString + "/Create " + FieldName, fileName = FieldName)]
    [
        AutoRef(nameof(RewardGoalFloatSettings), nameof(RewardGoalFloatSettings.schemas),
            FieldName, TypeString + "/" + FieldName, createNull: false, defaultFileName: "Reward Goal Float"
        )
    ]
    public class RewardGoalFloatSchema : RewardSchema<RewardGoalFloat>
    {
        private const string FieldName = nameof(RewardGoalFloatSchema);

        public ERewardGoalType rewardGoalType;
        public GoalRangeFloatSchema goalRangeFloatSchema;
        public StatSchemaObject schemaObject;
        public float reward;

        public override RewardGoalFloat ToData()
        {
            return new RewardGoalFloat
            {
                id = (ushort)ID,
                statKey = schemaObject.Key,
                rewardGoalType = rewardGoalType,
                goalId = goalRangeFloatSchema.id,
                reward = reward,
            };
        }
    }
}
